using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using OpCode = Xui.Compiler.XuiNet.DotNet.Op.OpCode;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Indexes instruction boundaries and resolves direct successors without copying operands.</summary>
internal static class GraphReader
{
    private static readonly Dictionary<OpCode, System.Reflection.Emit.OpCode> Descriptors =
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(System.Reflection.Emit.OpCode))
            .Select(field => (System.Reflection.Emit.OpCode)field.GetValue(null)!)
            .Where(code => Enum.IsDefined((OpCode)unchecked((ushort)code.Value)))
            .ToDictionary(code => (OpCode)unchecked((ushort)code.Value));

    public static InstructionGraph Read(ILReader source)
    {
        var reader = source;
        var nodes = new List<InstructionNode>();
        var offsets = new Dictionary<int, InstructionNode>();
        while (reader.HasRemaining)
        {
            int offset = reader.Offset;
            var instruction = reader.Read();
            if (!Descriptors.TryGetValue(instruction.Code, out var descriptor))
                throw new NotSupportedException($"Unknown opcode at IL_{offset:x4}.");
            if (descriptor.OperandType == OperandType.InlineSwitch)
                _ = instruction.ReadSwitch(ref reader);
            else
            {
                int size = descriptor.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineI or OperandType.ShortInlineVar or OperandType.ShortInlineBrTarget => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineI or OperandType.ShortInlineR or OperandType.InlineBrTarget or
                    OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineSig or
                    OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType => 4,
                    _ => throw new NotSupportedException($"Unsupported operand encoding {descriptor.OperandType}.")
                };
                reader.Bytes.Offset += size;
            }
            var node = new InstructionNode(offset, reader.Offset, instruction.Code);
            nodes.Add(node);
            offsets.Add(offset, node);
        }
        if (nodes.Count == 0) throw new BadImageFormatException("Empty IL body.");
        foreach (var node in nodes)
        {
            reader = source;
            reader.Bytes.Offset = node.Offset;
            var instruction = reader.Read();
            var descriptor = Descriptors[node.Code];
            InstructionNode Target(int offset) => offsets.TryGetValue(offset, out var target) ? target :
                throw new BadImageFormatException($"Invalid successor IL_{offset:x4} from IL_{node.Offset:x4}.");
            switch (descriptor.OperandType)
            {
                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                    int target = descriptor.OperandType == OperandType.InlineBrTarget
                        ? instruction.ReadBranch(ref reader).Target : instruction.ReadShortBranch(ref reader).Target;
                    node.Successors = descriptor.FlowControl == FlowControl.Branch
                        ? [Target(target)] : [Target(node.End), Target(target)];
                    break;
                case OperandType.InlineSwitch:
                    var successors = ImmutableArray.CreateBuilder<InstructionNode>();
                    successors.Add(Target(node.End));
                    foreach (int destination in instruction.ReadSwitch(ref reader).Targets) successors.Add(Target(destination));
                    node.Successors = successors.ToImmutable();
                    break;
                default:
                    node.Successors = descriptor.FlowControl switch
                    {
                        FlowControl.Return or FlowControl.Throw => [],
                        FlowControl.Branch => throw new NotSupportedException($"Unsupported transfer {node.Code} at IL_{node.Offset:x4}."),
                        _ => [Target(node.End)]
                    };
                    break;
            }
        }
        return new InstructionGraph(nodes.AsReadOnly(), new StackTree());
    }
}
