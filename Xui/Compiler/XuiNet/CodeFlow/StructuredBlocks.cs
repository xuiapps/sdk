namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Reconstructs nested conditionals and single-entry loops from an analyzed instruction graph.</summary>
/// <remarks>Multiple-entry cycles and overlapping nonterminal regions are rejected without a dispatcher.</remarks>
public sealed class StructuredBlocks
{
    private readonly Dictionary<InstructionNode, LoopRegion> loops;
    private readonly HashSet<InstructionNode> emitted = [];

    private StructuredBlocks(InstructionGraph graph) => loops = LoopRegions.Find(graph);

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
                var innerContinuations = new RegionContinuations(node, loops, nested);
                nested.Block.Body = Sequence(node, null, nested, innerContinuations, enteringLoop: true);
                children.Add(nested.Block);
                node = nested.Block.Exit;
                continue;
            }
            // Roslyn shares terminal tails between short-circuit branches. Each arm may
            // emit a straight-line tail ending at its join or returning/throwing.
            if (!emitted.Add(node) && !IsStraightTail(node, stop)) throw Unsupported(node, "overlapping regions");
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
                children.Add(new ConditionalBlock(node, taken, notTaken, join, preserved));
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

    private static bool IsStraightTail(InstructionNode node, InstructionNode? stop)
    {
        var visited = new HashSet<InstructionNode>();
        while (visited.Add(node))
        {
            if (node == stop) return true;
            if (node.Successors.Length == 0)
                return node.Code is DotNet.Op.OpCode.Ret or DotNet.Op.OpCode.Throw or DotNet.Op.OpCode.Rethrow;
            if (node.Successors.Length != 1) return false;
            node = node.Successors[0];
        }
        return false;
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
