using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using WasmType = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class MethodPlan
{
    private void RegisterExceptionDispatches(SequenceBlock sequence)
    {
        foreach (var node in sequence.Children)
            switch (node)
            {
                case ExceptionDispatch dispatch:
                    UsesSwitch = true;
                    ExceptionSelectors.TryAdd(dispatch.Region.Start, default);
                    RegisterExceptionDispatches(new([dispatch.Region, dispatch.Selection]));
                    break;
                case ExceptionBlock region:
                    RegisterExceptionDispatches(region.Body);
                    foreach (var handler in region.Handlers) RegisterExceptionDispatches(handler.Body);
                    break;
                case ConditionalBlock conditional:
                    RegisterExceptionDispatches(conditional.Taken); RegisterExceptionDispatches(conditional.NotTaken);
                    break;
                case SwitchBlock selection:
                    foreach (var arm in selection.Arms) RegisterExceptionDispatches(arm.Body);
                    break;
                case LoopBlock loop: RegisterExceptionDispatches(loop.Body); break;
            }
    }

    public void DefineBlockTypes(ref TypeWriter types)
    {
        if (Flow is not null) DefineBlockTypes(Flow, ref types, new HashSet<FlowNode>());
    }

    private void DefineBlockTypes(SequenceBlock sequence, ref TypeWriter types, HashSet<FlowNode> defined)
    {
        foreach (var child in sequence.Children)
        {
            if (!defined.Add(child)) continue;
            switch (child)
            {
                case ExceptionDispatch dispatch:
                    DefineBlockTypes(new([dispatch.Region, dispatch.Selection]), ref types, defined);
                    break;
                case ExceptionTransfer:
                    break;
                case ExceptionBlock exception:
                    DefineBlockTypes(exception.Body, ref types, defined);
                    foreach (var handler in exception.Handlers) DefineBlockTypes(handler.Body, ref types, defined);
                    break;
                case ConditionalBlock block:
                    BlockTypes.Add(block, types.FunctionType(block.Parameters.SelectMany(Types).ToArray(), block.Results.SelectMany(Types).ToArray()));
                    DefineBlockTypes(block.Taken, ref types, defined);
                    DefineBlockTypes(block.NotTaken, ref types, defined);
                    break;
                case SwitchBlock selection:
                    var parameters = selection.Parameters.SelectMany(Types).ToArray();
                    BlockTypes.Add(selection, types.FunctionType(parameters, selection.Results.SelectMany(Types).ToArray()));
                    SwitchArmTypes.Add(selection, types.FunctionType(parameters, parameters));
                    foreach (var arm in selection.Arms) DefineBlockTypes(arm.Body, ref types, defined);
                    break;
                case LoopBlock loop:
                    BlockTypes.Add(loop, types.FunctionType(loop.Parameters.SelectMany(Types).ToArray(), loop.Results.SelectMany(Types).ToArray()));
                    DefineBlockTypes(loop.Body, ref types, defined);
                    break;
                case ForwardRegion forward:
                    foreach (var segment in forward.Segments.Skip(1))
                        BlockTypes.Add(segment, types.FunctionType([], segment.Results.SelectMany(Types).ToArray()));
                    foreach (var node in forward.Segments.SelectMany(s=>s.Instructions))
                    {
                        if (node.Successors.Length<2 || !node.Successors.Any(s=>NeedsStringMerge(node.After,s.Before))) continue;
                        var entries=new List<StackEntry>();
                        for(var state=node.After!;state.Entry is { } entry;state=state.Pop()) entries.Add(entry);
                        entries.Reverse();
                        var input=entries.SelectMany(Types).ToArray();
                        MergeBranchTypes.Add(node,types.FunctionType(input,node.Code==XuiNet.DotNet.Op.OpCode.Switch?input:[]));
                    }
                    break;
                case InstructionBlock or LoopTransfer:
                    break;
                default:
                    throw new NotSupportedException($"Unsupported structured node {child.GetType().Name}.");
            }
        }

        IEnumerable<WasmType> Types(StackEntry type)
        {
            if (type.Type == StackType.ManagedPointer && type.DeclaredType is { } declared && Storage.Bind(declared).IsByReference)
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
