using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitForwardRegion(ForwardRegion region)
    {
        var segments = region.Segments;
        var destinations = segments.Select((segment, index) => (segment, index))
            .ToDictionary(pair => pair.segment.Instructions[0], pair => pair.index);
        var initialized = new bool[segments.Count][];
        initialized[0] = (bool[])initializedLocals.Clone();
        // All labels open at method entry with an empty operand stack. Each returns
        // its destination's entire analyzed stack, including two-part managed byrefs.
        for (int i = segments.Count - 1; i > 0; i--)
        {
            writer.Block(method.BlockTypes[segments[i]]);
            labels.Push((null, default));
        }
        for (int i = 0; i < segments.Count; i++)
        {
            if (i != 0) { writer.End(); labels.Pop(); }
            stack.Clear(); RestorePrefix(segments[i].Input);
            initialized[i].CopyTo(initializedLocals, 0);
            var instructions = segments[i].Instructions;
            foreach (var node in instructions)
            {
                CheckStack(node.Before!, node.Offset);
                if (node.Code == OpCode.Switch)
                {
                    Pop(SignatureTypeCode.Int32, node.Code, node.Offset);
                    writer.BrTable(node.Successors.Skip(1).Select(next => Target(next, i, destinations)).ToArray(),
                        Target(node.Successors[0], i, destinations));
                }
                else if (node.Successors.Length == 2)
                {
                    EmitCondition(node);
                    writer.BrIf(Target(node.Successors[1], i, destinations));
                    writer.Br(Target(node.Successors[0], i, destinations));
                }
                else if (node.Code is not (OpCode.Br or OpCode.Br_s))
                {
                    var reader = method.Body.Instructions;
                    reader.Seek(node.Offset);
                    EmitInstruction(ref reader);
                }
                CheckStack(node.After!, node.Offset);
            }
            var last = instructions[^1];
            foreach (var next in last.Successors.Distinct())
            {
                int destination = destinations[next];
                if (initialized[destination] is not { } state) initialized[destination] = (bool[])initializedLocals.Clone();
                else for (int local = 0; local < state.Length; local++) state[local] &= initializedLocals[local];
            }
            if (last.Successors.Length == 1 && last.Code != OpCode.Switch)
                writer.Br(Target(last.Successors[0], i, destinations));
        }
        // Every reachable terminal instruction returns or throws. The method must
        // not appear to fall through after closing result-producing destination blocks.
        writer.Unreachable();
    }

    private static LabelIndex Target(InstructionNode node, int current, IReadOnlyDictionary<InstructionNode, int> destinations) =>
        new(checked((uint)(destinations[node] - current - 1)));
}
