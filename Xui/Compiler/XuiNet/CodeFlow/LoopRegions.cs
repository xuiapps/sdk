using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Finds natural loops from DFS back edges and rejects multiple-entry or overlapping cycles.</summary>
internal static class LoopRegions
{
    public static Dictionary<InstructionNode, LoopRegion> Find(InstructionGraph graph)
    {
        var predecessors = graph.Instructions.Where(node => node.IsReachable)
            .ToDictionary(node => node, _ => new List<InstructionNode>());
        foreach (var node in predecessors.Keys)
            foreach (var target in node.Successors) predecessors[target].Add(node);
        var visited = new HashSet<InstructionNode>();
        var active = new HashSet<InstructionNode>();
        var members = new Dictionary<InstructionNode, HashSet<InstructionNode>>();
        Visit(graph.Entry);
        var loops = new Dictionary<InstructionNode, LoopRegion>();
        foreach (var (header, nodes) in members)
        {
            if (header != graph.Entry && nodes.Contains(graph.Entry))
                throw Unsupported(header, "multiple-entry cycle");
            var exits = nodes.SelectMany(node => node.Successors).Where(node => !nodes.Contains(node)).Distinct().ToArray();
            if (exits.Length == 0) throw Unsupported(header, "loop requires a single continuation");
            // Debug IL can put a nop or an unconditional branch between break and the common exit.
            // Absorb only those forwarding instructions, stopping at the first shared continuation.
            var paths = exits.Select(ExitPath).ToArray();
            var exit = paths[0].FirstOrDefault(candidate => paths.All(path => path.Contains(candidate)))
                ?? throw Unsupported(header, "loop requires a single continuation");
            foreach (var path in paths)
                foreach (var forwarded in path.TakeWhile(node => node != exit)) nodes.Add(forwarded);
            if (nodes.Any(node => node != header && predecessors[node].Any(previous => !nodes.Contains(previous))))
                throw Unsupported(header, "multiple-entry loop region");
            int depth = nodes.Min(node => node.Lowest!.Depth);
            var preserved = header.Before!;
            while (preserved.Depth > depth) preserved = preserved.Pop();
            loops.Add(header, new(new LoopBlock(header, exit, preserved), nodes));
        }
        var regions = loops.Values.ToArray();
        for (int i = 0; i < regions.Length; i++)
            for (int j = i + 1; j < regions.Length; j++)
                if (regions[i].Members.Overlaps(regions[j].Members) &&
                    !regions[i].Members.IsSubsetOf(regions[j].Members) && !regions[j].Members.IsSubsetOf(regions[i].Members))
                    throw Unsupported(regions[i].Block.Header, "overlapping loops");
        return loops;

        static List<InstructionNode> ExitPath(InstructionNode node)
        {
            var path = new List<InstructionNode>();
            var seen = new HashSet<InstructionNode>();
            while (seen.Add(node))
            {
                path.Add(node);
                if (node.Code is not (OpCode.Nop or OpCode.Br or OpCode.Br_s) || node.Successors.Length != 1) break;
                node = node.Successors[0];
            }
            return path;
        }

        void Visit(InstructionNode node)
        {
            if (!visited.Add(node)) return;
            active.Add(node);
            foreach (var target in node.Successors)
            {
                if (!active.Contains(target)) { Visit(target); continue; }
                if (!members.TryGetValue(target, out var loop)) members.Add(target, loop = [target]);
                var work = new Stack<InstructionNode>();
                work.Push(node);
                while (work.TryPop(out var predecessor))
                {
                    if (!loop.Add(predecessor)) continue;
                    foreach (var previous in predecessors[predecessor]) work.Push(previous);
                }
            }
            active.Remove(node);
        }
    }

    private static NotSupportedException Unsupported(InstructionNode node, string reason) =>
        new($"Code flow jumps unsupported at IL_{node.Offset:x4}: {reason}.");
}
