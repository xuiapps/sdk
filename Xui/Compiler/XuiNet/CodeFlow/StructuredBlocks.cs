namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Reconstructs nested conditionals and single-entry loops from an analyzed instruction graph.</summary>
/// <remarks>Shared acyclic tails may be duplicated within a linear size budget. Multiple-entry cycles require a future dispatcher.</remarks>
public sealed class StructuredBlocks
{
    private readonly Dictionary<InstructionNode, LoopRegion> loops;
    private readonly HashSet<InstructionNode> emitted = [];
    private readonly Dictionary<LoopRegion, int> completedLoops = new();

    private readonly Dictionary<(InstructionNode Node, InstructionNode? Stop, LoopRegion? Region), bool> duplicateTails = new();
    private int remainingInstructions;

    private StructuredBlocks(InstructionGraph graph)
    {
        loops = LoopRegions.Find(graph);
        // Bound subtree expansion even when nested short-circuit diamonds share tails.
        remainingInstructions = checked(graph.Instructions.Count * 4);
    }

    /// <summary>Builds structured regions with typed inputs, results, and loop transfers.</summary>
    public static SequenceBlock Build(InstructionGraph graph)
    {
        var builder = new StructuredBlocks(graph);
        var continuations = new RegionContinuations(graph.Entry, builder.loops, null);
        return builder.Sequence(graph.Entry, null, null, continuations);
    }

    private SequenceBlock Sequence(InstructionNode? node, InstructionNode? stop, LoopRegion? region,
        RegionContinuations continuations, bool enteringLoop = false)
    {
        var children = new List<FlowNode>();
        while (node is not null && node != stop)
        {
            if (region is not null)
            {
                if (node == region.Block.Header && !enteringLoop)
                {
                    children.Add(new LoopTransfer(region.Block, LoopTransferKind.Continue));
                    break;
                }
                if (node == region.Block.Exit)
                {
                    children.Add(new LoopTransfer(region.Block, LoopTransferKind.Break));
                    break;
                }
            }
            enteringLoop = false;
            if (loops.TryGetValue(node, out var nested) && nested != region)
            {
                if (completedLoops.TryGetValue(nested, out int size))
                {
                    remainingInstructions -= size;
                    if (remainingInstructions < 0) throw Unsupported(node, "shared-tail expansion exceeds the structured code size budget");
                }
                else
                {
                    int before = remainingInstructions;
                    var innerContinuations = new RegionContinuations(node, loops, nested);
                    nested.Block.Body = Sequence(node, null, nested, innerContinuations, enteringLoop: true);
                    completedLoops.Add(nested, before - remainingInstructions);
                }
                children.Add(nested.Block);
                node = nested.Block.Exit;
                continue;
            }
            // Roslyn shares else/terminal tails between short-circuit branches. A tail
            // can itself contain conditionals; copying an acyclic tail preserves its
            // side effects because only one of the mutually exclusive arms executes.
            if (!emitted.Add(node) && !CanDuplicateTail(node, stop, region)) throw Unsupported(node, "overlapping regions");
            if (remainingInstructions-- <= 0) throw Unsupported(node, "shared-tail expansion exceeds the structured code size budget");
            if (node.Code == DotNet.Op.OpCode.Switch)
            {
                var join = continuations.Join(node, stop);
                var arms = node.Successors.Distinct().Select(entry => new SwitchArm(entry,
                    Sequence(entry, join, region, continuations), ReachesJoin(entry, join, region))).ToArray();
                int depth = arms.Min(arm => Lowest(arm.Body, node.After!.Depth));
                var preserved = node.After!;
                while (preserved.Depth > depth) preserved = preserved.Pop();
                children.Add(new SwitchBlock(node, arms, join, preserved));
                node = join;
            }
            else if (node.Successors.Length == 2)
            {
                var join = continuations.Join(node, stop);
                var taken = Sequence(node.Successors[1], join, region, continuations);
                var notTaken = Sequence(node.Successors[0], join, region, continuations);
                int depth = Math.Min(Lowest(taken, node.After!.Depth), Lowest(notTaken, node.After.Depth));
                var preserved = node.After;
                while (preserved.Depth > depth) preserved = preserved.Pop();
                children.Add(new ConditionalBlock(node, taken, notTaken, join, preserved,
                    ReachesJoin(node.Successors[1], join, region), ReachesJoin(node.Successors[0], join, region)));
                node = join;
            }
            else
            {
                children.Add(new InstructionBlock(node));
                node = node.Successors.Length == 0 ? null : node.Successors[0];
            }
        }
        return new SequenceBlock(children.AsReadOnly());
    }

    private bool CanDuplicateTail(InstructionNode node, InstructionNode? stop, LoopRegion? region)
    {
        if (node == stop || region is not null && (node == region.Block.Header || node == region.Block.Exit)) return true;
        var key = (node, stop, region);
        if (duplicateTails.TryGetValue(key, out bool found)) return found;
        // A completed natural loop can appear in mutually exclusive tails. Reuse
        // its structured body; each emission creates its own lexical WASM labels.
        if (loops.TryGetValue(node, out var nested))
            return duplicateTails[key] = CanDuplicateTail(nested.Block.Exit, stop, region);
        // RegionContinuations already rejected unstructured cycles. Remaining edges
        // form a DAG terminating at the join, return/throw, or a region transfer.
        return duplicateTails[key] = node.Successors.Length == 0
            ? node.Code is DotNet.Op.OpCode.Ret or DotNet.Op.OpCode.Throw or DotNet.Op.OpCode.Rethrow or
                DotNet.Op.OpCode.Leave or DotNet.Op.OpCode.Leave_s or DotNet.Op.OpCode.Endfinally
            : node.Successors.All(next => CanDuplicateTail(next, stop, region));
    }

    private static bool ReachesJoin(InstructionNode entry, InstructionNode? join, LoopRegion? region)
    {
        if (join is null) return false;
        var seen = new HashSet<InstructionNode>();
        var pending = new Stack<InstructionNode>();
        pending.Push(entry);
        while (pending.TryPop(out var node))
        {
            if (node == join) return true;
            if (!seen.Add(node) || region is not null && (node == region.Block.Header || node == region.Block.Exit)) continue;
            foreach (var next in node.Successors) pending.Push(next);
        }
        return false;
    }

    private static int Lowest(SequenceBlock block, int depth)
    {
        foreach (var child in block.Children)
        {
            int touched = child switch
            {
                InstructionBlock instruction => instruction.Instruction.Lowest!.Depth,
                ConditionalBlock conditional => Math.Min(conditional.Condition.Lowest!.Depth, conditional.Preserved.Depth),
                SwitchBlock selection => Math.Min(selection.Selector.Lowest!.Depth, selection.Preserved.Depth),
                LoopBlock loop => loop.Preserved.Depth,
                // WASM branches read their label arguments even though MSIL br preserves its stack.
                LoopTransfer transfer => transfer.Target.Preserved.Depth,
                _ => throw new NotSupportedException($"Unsupported structured node {child.GetType().Name}.")
            };
            depth = Math.Min(depth, touched);
        }
        return depth;
    }

    private static NotSupportedException Unsupported(InstructionNode node, string reason) =>
        new($"Code flow jumps unsupported at IL_{node.Offset:x4}: {reason}.");
}
