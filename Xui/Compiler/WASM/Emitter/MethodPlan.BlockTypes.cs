using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using WasmType = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class MethodPlan
{
    public void DefineBlockTypes(ref TypeWriter types)
    {
        if (Flow is not null) DefineBlockTypes(Flow, ref types);
    }

    private void DefineBlockTypes(SequenceBlock sequence, ref TypeWriter types)
    {
        foreach (var child in sequence.Children)
        {
            switch (child)
            {
                case ExceptionBlock exception:
                    DefineBlockTypes(exception.Body, ref types);
                    foreach (var handler in exception.Handlers) DefineBlockTypes(handler.Body, ref types);
                    break;
                case ConditionalBlock block:
                    BlockTypes.Add(block, types.FunctionType(block.Parameters.Select(Type).ToArray(), block.Results.Select(Type).ToArray()));
                    DefineBlockTypes(block.Taken, ref types);
                    DefineBlockTypes(block.NotTaken, ref types);
                    break;
                case LoopBlock loop:
                    BlockTypes.Add(loop, types.FunctionType(loop.Parameters.Select(Type).ToArray(), loop.Results.Select(Type).ToArray()));
                    DefineBlockTypes(loop.Body, ref types);
                    break;
                case InstructionBlock or LoopTransfer:
                    break;
                default:
                    throw new NotSupportedException($"Unsupported structured node {child.GetType().Name}.");
            }
        }

        WasmType Type(StackEntry type) => type.Type switch
        {
            StackType.Int32 or StackType.NativeInt => ValueTypeCode.I32,
            StackType.Int64 => ValueTypeCode.I64,
            StackType.Float => ValueTypeCode.F64,
            StackType.String => ValueTypeCode.ExternRef,
            StackType.ObjectReference or StackType.ArrayReference => ValueTypeCode.EqRef,
            StackType.ValueType when Storage.IsGc(type.DeclaredType!.Value.Identity!.Value) => ValueTypeCode.EqRef,
            _ => throw new NotSupportedException($"Unsupported block type {type}.")
        };
    }
}
