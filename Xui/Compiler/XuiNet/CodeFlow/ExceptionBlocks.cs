using System.Collections.Immutable;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Composes lexical exception regions with the existing conditional/loop reconstruction.</summary>
/// <remarks>Filters, overlapping scopes, and protected regions with multiple normal exits are rejected.</remarks>
public static class ExceptionBlocks
{
    public static SequenceBlock Build(Model.Module module, Model.MethodBody body, InstructionGraph graph)
    {
        var groups = body.ExceptionRegions.GroupBy(r => (r.TryOffset, r.TryLength))
            .Select(g => new Group(g.Key.TryOffset, g.Key.TryOffset + g.Key.TryLength, g.ToArray())).ToArray();
        if (groups.Any(g => g.Handlers.Any(h => h.Kind == ExceptionRegionKind.Filter)))
            throw new NotSupportedException("Exception filters are not implemented.");
        var instructions = graph.Instructions.ToDictionary(n => n.Offset);
        return BuildRange(0, graph.Instructions[^1].End, null);

        SequenceBlock BuildRange(int start, int end, Group? owner)
        {
            var candidates = groups.Where(g => g != owner && g.Start >= start && g.End <= end).ToArray();
            var nested = candidates.Where(g => !candidates.Any(parent => parent != g && parent.Start <= g.Start && parent.End >= g.End)).ToArray();
            var replacements = new Dictionary<int, ExceptionBlock>();
            var nodes = new List<InstructionNode>();
            var original = graph.Instructions.Where(n => n.Offset >= start && n.Offset < end).ToArray();
            foreach (var node in original)
            {
                var group = nested.FirstOrDefault(g => g.Start <= node.Offset && node.Offset < g.End);
                if (group is not null)
                {
                    if (node.Offset != group.Start) continue;
                    var exits = graph.Instructions.Where(n => n.Offset >= group.Start && n.Offset < group.End && n.IsReachable)
                        .SelectMany(n => n.Successors).Where(n => n.Offset < group.Start || n.Offset >= group.End).Select(n => n.Offset).Distinct().ToArray();
                    if (exits.Length > 1) throw new NotSupportedException($"Exception region at IL_{group.Start:x4} has multiple normal exits.");
                    var handlers = group.Handlers.Select(h => new ExceptionHandler(h.HandlerOffset, h.Kind,
                        h.Kind == ExceptionRegionKind.Catch ? SignatureReader.Resolve(module, h.CatchType).Identity : null,
                        BuildRange(h.HandlerOffset, h.HandlerOffset + h.HandlerLength, group))).ToArray();
                    replacements.Add(node.Offset, new(group.Start, group.End, exits.Length == 0 ? null : exits[0],
                        BuildRange(group.Start, group.TryEnd, group), handlers));
                    nodes.Add(new(node.Offset, group.End, OpCode.Nop) { Before = node.Before, After = graph.Stacks.Empty, Lowest = graph.Stacks.Empty });
                }
                else nodes.Add(new(node.Offset, node.End, node.Code) { Before = node.Before, After = node.After, Lowest = node.Lowest });
            }
            var byOffset = nodes.ToDictionary(n => n.Offset);
            foreach (var node in nodes)
            {
                var targets = replacements.TryGetValue(node.Offset, out var block)
                    ? block.CanContinue && block.Continuation is { } next ? new[] { next } : Array.Empty<int>()
                    : instructions[node.Offset].Successors.Select(n => n.Offset).ToArray();
                node.Successors = targets.Where(t => t >= start && t < end).Select(t => byOffset.TryGetValue(t, out var target)
                    ? target : throw new NotSupportedException($"Branch into an exception handler or protected scope at IL_{t:x4}.")).ToImmutableArray();
            }
            if (nodes.Count == 0) return new([]);
            return Replace(StructuredBlocks.Build(new(nodes.AsReadOnly(), graph.Stacks)));

            SequenceBlock Replace(SequenceBlock sequence) => new(sequence.Children.Select<FlowNode, FlowNode>(node => node switch
            {
                InstructionBlock instruction when replacements.TryGetValue(instruction.Instruction.Offset, out var block) => block,
                ConditionalBlock conditional => new ConditionalBlock(conditional.Condition, Replace(conditional.Taken), Replace(conditional.NotTaken), conditional.Join, conditional.Preserved),
                LoopBlock loop => ReplaceLoop(loop),
                _ => node
            }).ToArray());
            LoopBlock ReplaceLoop(LoopBlock loop) { loop.Body = Replace(loop.Body); return loop; }
        }
    }

    private sealed record Group(int Start, int TryEnd, ExceptionRegion[] Handlers)
    {
        public int End => Math.Max(TryEnd, Handlers.Max(h => h.HandlerOffset + h.HandlerLength));
    }
}
