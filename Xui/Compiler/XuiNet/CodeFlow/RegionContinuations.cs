using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Finds joins after collapsing nested loops and treating region transfers as terminal edges.</summary>
internal sealed class RegionContinuations
{
    private readonly Dictionary<InstructionNode, LoopRegion> loops;
    private readonly LoopRegion? region;
    private readonly Dictionary<InstructionNode, InstructionNode?> continuations = new();
    private readonly Dictionary<InstructionNode, int> depths = new();
    private readonly HashSet<InstructionNode> visiting = [];

    public RegionContinuations(InstructionNode entry, Dictionary<InstructionNode, LoopRegion> loops, LoopRegion? region)
    {
        this.loops = loops;
        this.region = region;
        FindContinuation(entry);
    }

    public InstructionNode? Join(InstructionNode node)
    {
        if (continuations[node] is { } join) return join;
        if (node.Successors.Length == 2 && node.Successors[0] != node.Successors[1])
        {
            // A guard may return on one path while its other path rejoins the
            // outer fallthrough. That continuation is not a strict postdominator.
            foreach (var candidate in node.Successors)
                if (!IsBoundary(candidate) && Reaches(node.Successors.First(s => s != candidate), candidate, []))
                    return candidate;
        }
        return null;
    }

    private bool Reaches(InstructionNode node, InstructionNode target, HashSet<InstructionNode> visited)
    {
        if (node == target) return true;
        if (IsBoundary(node) || !visited.Add(node)) return false;
        if (loops.TryGetValue(node, out var nested) && nested != region)
            return Reaches(nested.Block.Exit, target, visited);
        return node.Successors.Any(next => Reaches(next, target, visited));
    }

    private bool IsBoundary(InstructionNode node) => region is not null &&
        (node == region.Block.Header || node == region.Block.Exit);

    private void FindContinuation(InstructionNode node)
    {
        if (continuations.ContainsKey(node)) return;
        if (!visiting.Add(node)) throw Unsupported(node, "unstructured cycle");
        if (node.Code == OpCode.Switch) throw Unsupported(node, "switch is not implemented");
        var successors = loops.TryGetValue(node, out var nested) && nested != region
            ? new[] { nested.Block.Exit } : node.Successors.ToArray();
        InstructionNode? join = null;
        for (int i = 0; i < successors.Length; i++)
        {
            InstructionNode? successor = IsBoundary(successors[i]) ? null : successors[i];
            if (successor is not null) FindContinuation(successor);
            join = i == 0 ? successor : CommonContinuation(join, successor);
        }
        continuations.Add(node, join);
        depths.Add(node, join is null ? 1 : depths[join] + 1);
        visiting.Remove(node);
    }

    private InstructionNode? CommonContinuation(InstructionNode? left, InstructionNode? right)
    {
        while (left != right)
        {
            int leftDepth = left is null ? 0 : depths[left];
            int rightDepth = right is null ? 0 : depths[right];
            if (leftDepth >= rightDepth) left = continuations[left!];
            if (rightDepth >= leftDepth) right = continuations[right!];
        }
        return left;
    }

    private static NotSupportedException Unsupported(InstructionNode node, string reason) =>
        new($"Code flow jumps unsupported at IL_{node.Offset:x4}: {reason}.");
}
