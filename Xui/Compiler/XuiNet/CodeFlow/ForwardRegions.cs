using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Provides a linear-size fallback for whole-method acyclic instruction graphs.</summary>
public static class ForwardRegions
{
    /// <summary>Returns a forward region, or null if cycles require the loop structurer or a future dispatcher.</summary>
    public static ForwardRegion? TryBuild(InstructionGraph graph)
    {
        if (graph.Entry.Before?.Depth != 0) return null;
        var pending = new Queue<InstructionNode>();
        var incoming = new Dictionary<InstructionNode, int> { [graph.Entry] = 0 };
        var seen = new HashSet<InstructionNode>();
        pending.Enqueue(graph.Entry);
        while (pending.TryDequeue(out var node))
        {
            if (!seen.Add(node)) continue;
            foreach (var next in node.Successors.Distinct())
            {
                incoming[next] = incoming.GetValueOrDefault(next) + 1;
                pending.Enqueue(next);
            }
        }
        var remaining = new Dictionary<InstructionNode, int>(incoming);
        foreach (var (node, count) in remaining) if (count == 0) pending.Enqueue(node);
        var ordered = new List<InstructionNode>();
        while (pending.TryDequeue(out var node))
        {
            ordered.Add(node);
            foreach (var next in node.Successors.Distinct()) if (--remaining[next] == 0) pending.Enqueue(next);
        }
        if (ordered.Count != seen.Count) return null;
        var segments = new List<ForwardSegment>();
        var run = new List<InstructionNode>();
        for (int i = 0; i < ordered.Count; i++)
        {
            var node = ordered[i];
            run.Add(node);
            if (i + 1 < ordered.Count && node.Successors.Length == 1 && node.Successors[0] == ordered[i + 1] &&
                incoming[ordered[i + 1]] == 1 && node.Code != OpCode.Switch) continue;
            segments.Add(new(run.AsReadOnly()));
            run = new();
        }
        return new(segments.AsReadOnly());
    }
}
