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
                    BlockTypes.Add(block, types.FunctionType(block.Parameters.SelectMany(Types).ToArray(), block.Results.SelectMany(Types).ToArray()));
                    DefineBlockTypes(block.Taken, ref types);
                    DefineBlockTypes(block.NotTaken, ref types);
                    break;
                case SwitchBlock selection:
                    var parameters = selection.Parameters.SelectMany(Types).ToArray();
                    BlockTypes.Add(selection, types.FunctionType(parameters, selection.Results.SelectMany(Types).ToArray()));
                    SwitchArmTypes.Add(selection, types.FunctionType(parameters, parameters));
                    foreach (var arm in selection.Arms) DefineBlockTypes(arm.Body, ref types);
                    break;
                case LoopBlock loop:
                    BlockTypes.Add(loop, types.FunctionType(loop.Parameters.SelectMany(Types).ToArray(), loop.Results.SelectMany(Types).ToArray()));
                    DefineBlockTypes(loop.Body, ref types);
                    break;
                case InstructionBlock or LoopTransfer:
                    break;
                default:
                    throw new NotSupportedException($"Unsupported structured node {child.GetType().Name}.");
            }
        }

        IEnumerable<WasmType> Types(StackEntry type)
        {
            if (type.Type == StackType.ManagedPointer && type.DeclaredType is { } declared && Storage.Bind(declared).IsReferenceAddress)
                return [(WasmType)ValueTypeCode.EqRef, (WasmType)ValueTypeCode.I32];
            return [Type(type)];
        }

        WasmType Type(StackEntry type) => type.Type switch
        {
            StackType.ManagedPointer when !Storage.Bind(type.DeclaredType!.Value).UsesGc => ValueTypeCode.I32,
            StackType.Int32 or StackType.NativeInt or StackType.MethodPointer => ValueTypeCode.I32,
            StackType.Int64 => ValueTypeCode.I64,
            StackType.Float => ValueTypeCode.F64,
            StackType.String => ValueTypeCode.ExternRef,
            StackType.ObjectReference or StackType.ArrayReference or StackType.RuntimeTypeHandle or StackType.Null => ValueTypeCode.EqRef,
            StackType.ValueType => Storage.IsGc(type.DeclaredType!.Value.Identity!.Value) ? ValueTypeCode.EqRef : ValueTypeCode.I32,
            _ => throw new NotSupportedException($"Unsupported block type {type}.")
        };
    }
}
