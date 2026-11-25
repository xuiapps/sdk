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
    private readonly Dictionary<InstructionNode, int> ranks = new();

    public RegionContinuations(InstructionNode entry, Dictionary<InstructionNode, LoopRegion> loops, LoopRegion? region)
    {
        this.loops = loops;
        this.region = region;
        FindContinuation(entry);
    }

    public InstructionNode? Join(InstructionNode node, InstructionNode? enclosingStop)
    {
        var strict = continuations[node];
        if (strict is not null && !IsReturnTail(strict)) return strict;
        if (enclosingStop is not null) return enclosingStop;
        // Early returns can all converge at a Debug return epilogue. Prefer an
        // earlier shared continuation, but retain ordinary value-producing joins.
        var common = Reachable(node.Successors[0]);
        foreach (var successor in node.Successors.Skip(1)) common.IntersectWith(Reachable(successor));
        InstructionNode? shared = null;
        foreach (var candidate in common)
            if (shared is null || ranks[candidate] > ranks[shared]) shared = candidate;
        return shared;
    }

    private static bool IsReturnTail(InstructionNode node)
    {
        var seen = new HashSet<InstructionNode>();
        while (seen.Add(node))
        {
            if (node.Successors.Length == 0) return node.Code == OpCode.Ret;
            if (node.Successors.Length != 1) return false;
            node = node.Successors[0];
        }
        return false;
    }

    private HashSet<InstructionNode> Reachable(InstructionNode entry)
    {
        var result = new HashSet<InstructionNode>();
        var pending = new Queue<InstructionNode>();
        pending.Enqueue(entry);
        while (pending.TryDequeue(out var node))
        {
            if (IsBoundary(node) || !result.Add(node)) continue;
            if (loops.TryGetValue(node, out var nested) && nested != region) pending.Enqueue(nested.Block.Exit);
            else foreach (var next in node.Successors) pending.Enqueue(next);
        }
        return result;
    }

    private bool IsBoundary(InstructionNode node) => region is not null &&
        (node == region.Block.Header || node == region.Block.Exit);

    private void FindContinuation(InstructionNode node)
    {
        if (continuations.ContainsKey(node)) return;
        if (!visiting.Add(node)) throw Unsupported(node, "unstructured cycle");
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
        ranks.Add(node, ranks.Count);
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
