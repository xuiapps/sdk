using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using Writer = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Runtime.DelegateField;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public bool DelegatesEnabled { get; set; }
    public TypeIndex DelegateType { get; private set; }
    public TypeIndex DelegateListType { get; private set; }
    public TypeIndex DelegateMethodsType { get; private set; }
    public TypeIndex? DelegateMethodChunksType { get; private set; }
    public GlobalIndex DelegateMethodIds { get; set; }
    public HashSet<string> DelegateOperations { get; } = new();
    public Dictionary<DelegateHelper, DelegateFunction> DelegateFunctions { get; } = new();

    private void DefineMulticastTypes(ref TypeWriter writer)
    {
        if (!DelegatesEnabled) return;
        DelegateFunctions.Add(DelegateHelper.Count, new(writer.FunctionType([EqRef], I32), []));
        DelegateFunctions.Add(DelegateHelper.Element, new(writer.FunctionType([EqRef, I32], EqRef), []));
        bool equality = DelegateOperations.Contains("op_Equality") || DelegateOperations.Contains("op_Inequality");
        if (equality || DelegateOperations.Contains("Remove"))
            DelegateFunctions.Add(DelegateHelper.SameEntry, new(writer.FunctionType([EqRef, EqRef], I32), []));
        if (equality) DelegateFunctions.Add(DelegateHelper.Equal, new(writer.FunctionType([EqRef, EqRef], I32), [I32, I32]));
        if (DelegateOperations.Contains("Combine"))
            DelegateFunctions.Add(DelegateHelper.Combine, new(writer.FunctionType([EqRef, EqRef], EqRef), [I32, I32, I32, EqRef]));
        if (DelegateOperations.Contains("Remove"))
            DelegateFunctions.Add(DelegateHelper.Remove, new(writer.FunctionType([EqRef, EqRef], EqRef), [I32, I32, I32, I32, I32, EqRef]));
    }

    public int[] DelegateTableMethods(IReadOnlyDictionary<MethodIdentity, MethodPlan> methods)
    {
        var ids = new int[TableSize];
        foreach (var target in DelegateTargets.Values) ids[target.TableSlot] = (int)target.Method.Function.Value;
        foreach (var type in Classes.Types)
        {
            foreach (var slot in type.VirtualSlots)
                if (methods.TryGetValue(slot.Implementation, out var method) && !method.IsDeclaration)
                    ids[type.TableStart + (uint)Classes.DispatchMethods.Count + slot.Index] = (int)method.Function.Value;
            foreach (var slot in type.GenericVirtualSlots)
                if (methods.TryGetValue(slot.Implementation, out var method) && !method.IsDeclaration)
                    ids[type.TableStart + slot.Index] = (int)method.Function.Value;
            foreach (var slot in type.InterfaceSlots)
                if (methods.TryGetValue(slot.Implementation, out var method) && !method.IsDeclaration)
                    ids[type.TableStart + slot.Index] = (int)method.Function.Value;
        }
        return ids;
    }

    private Writer DelegateField(Writer writer, uint local, Runtime.DelegateField field) =>
        writer.LocalGet(new(local)).RefCast(DelegateType).StructGet(DelegateType, new((uint)field));

    private void NewInvocationList(Writer writer, uint source, uint list)
    {
        DelegateField(writer, source, Descriptor).RefNull(EqRef).I32Const(0).I32Const(0).LocalGet(new(list)).StructNew(DelegateType);
    }

    public void EmitDelegateHelper(Writer w, DelegateHelper helper)
    {
        FunctionIndex Fn(DelegateHelper kind) => DelegateFunctions[kind].Function;
        switch (helper)
        {
            case DelegateHelper.Count:
                DelegateField(w, 0, InvocationList).RefIsNull().If().I32Const(1).Return().End();
                DelegateField(w, 0, InvocationList).RefCast(DelegateListType).ArrayLen();
                break;
            case DelegateHelper.Element:
                DelegateField(w, 0, InvocationList).RefIsNull().If().LocalGet(new(0)).Return().End();
                DelegateField(w, 0, InvocationList).RefCast(DelegateListType).LocalGet(new(1)).ArrayGet(DelegateListType);
                break;
            case DelegateHelper.SameEntry:
                DelegateField(w, 0, Descriptor); DelegateField(w, 1, Descriptor).RefEq();
                DelegateField(w, 0, Target); DelegateField(w, 1, Target).RefEq().I32And();
                DelegateField(w, 0, Method); DelegateField(w, 1, Method).I32Eq().I32And();
                break;
            case DelegateHelper.Equal:
                w.LocalGet(new(0)).LocalGet(new(1)).RefEq().If().I32Const(1).Return().End();
                w.LocalGet(new(0)).RefIsNull().LocalGet(new(1)).RefIsNull().I32Or().If().I32Const(0).Return().End();
                w.LocalGet(new(0)).Call(Fn(DelegateHelper.Count)).LocalTee(new(2))
                    .LocalGet(new(1)).Call(Fn(DelegateHelper.Count)).I32Eq().I32Eqz().If().I32Const(0).Return().End();
                w.Block(EmptyBlock).Loop().LocalGet(new(3)).LocalGet(new(2)).I32LtU().I32Eqz().BrIf(new(1));
                w.LocalGet(new(0)).LocalGet(new(3)).Call(Fn(DelegateHelper.Element))
                    .LocalGet(new(1)).LocalGet(new(3)).Call(Fn(DelegateHelper.Element))
                    .Call(Fn(DelegateHelper.SameEntry)).I32Eqz().If().I32Const(0).Return().End();
                w.LocalGet(new(3)).I32Const(1).I32Add().LocalSet(new(3)).Br(new(0)).End().End().I32Const(1);
                break;
            case DelegateHelper.Combine:
                // Arguments a,b; locals aCount,total,index,list. Null combinations preserve identity.
                w.LocalGet(new(0)).RefIsNull().If().LocalGet(new(1)).Return().End();
                w.LocalGet(new(1)).RefIsNull().If().LocalGet(new(0)).Return().End();
                w.LocalGet(new(0)).Call(Fn(DelegateHelper.Count)).LocalTee(new(2))
                    .LocalGet(new(1)).Call(Fn(DelegateHelper.Count)).I32Add().LocalTee(new(3))
                    .ArrayNewDefault(DelegateListType).LocalSet(new(5));
                w.Block(EmptyBlock).Loop().LocalGet(new(4)).LocalGet(new(3)).I32LtU().I32Eqz().BrIf(new(1));
                w.LocalGet(new(5)).RefCast(DelegateListType).LocalGet(new(4));
                w.LocalGet(new(4)).LocalGet(new(2)).I32LtU().If(ExceptionPayloadBlock)
                    .LocalGet(new(0)).LocalGet(new(4)).Call(Fn(DelegateHelper.Element)).Else()
                    .LocalGet(new(1)).LocalGet(new(4)).LocalGet(new(2)).I32Sub().Call(Fn(DelegateHelper.Element)).End()
                    .ArraySet(DelegateListType);
                w.LocalGet(new(4)).I32Const(1).I32Add().LocalSet(new(4)).Br(new(0)).End().End();
                NewInvocationList(w, 0, 5);
                break;
            case DelegateHelper.Remove:
                EmitDelegateRemove(w);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(helper), helper, "Unknown delegate helper.");
        }
    }

    private void EmitDelegateRemove(Writer w)
    {
        FunctionIndex Fn(DelegateHelper kind) => DelegateFunctions[kind].Function;
        // Arguments source,value; locals sourceCount,valueCount,start,j,resultCount,list.
        w.LocalGet(new(0)).RefIsNull().LocalGet(new(1)).RefIsNull().I32Or().If().LocalGet(new(0)).Return().End();
        w.LocalGet(new(0)).Call(Fn(DelegateHelper.Count)).LocalSet(new(2));
        w.LocalGet(new(1)).Call(Fn(DelegateHelper.Count)).LocalTee(new(3)).LocalGet(new(2)).I32GtU()
            .If().LocalGet(new(0)).Return().End();
        w.LocalGet(new(2)).LocalGet(new(3)).I32Sub().LocalSet(new(4));
        w.Block(EmptyBlock).Loop(); // Matching a whole sequence exits the outer block.
        w.I32Const(0).LocalSet(new(5)).Block(EmptyBlock).Loop();
        w.LocalGet(new(5)).LocalGet(new(3)).I32Eq().BrIf(new(3));
        w.LocalGet(new(0)).LocalGet(new(4)).LocalGet(new(5)).I32Add().Call(Fn(DelegateHelper.Element))
            .LocalGet(new(1)).LocalGet(new(5)).Call(Fn(DelegateHelper.Element))
            .Call(Fn(DelegateHelper.SameEntry)).I32Eqz().BrIf(new(1));
        w.LocalGet(new(5)).I32Const(1).I32Add().LocalSet(new(5)).Br(new(0)).End().End();
        w.LocalGet(new(4)).I32Eqz().If().LocalGet(new(0)).Return().End();
        w.LocalGet(new(4)).I32Const(1).I32Sub().LocalSet(new(4)).Br(new(0)).End().End();
        w.LocalGet(new(2)).LocalGet(new(3)).I32Sub().LocalTee(new(6)).I32Eqz().If().RefNull(EqRef).Return().End();
        w.LocalGet(new(6)).I32Const(1).I32Eq().If().LocalGet(new(0))
            .LocalGet(new(3)).I32Const(0).LocalGet(new(4)).I32Eqz().Select().Call(Fn(DelegateHelper.Element)).Return().End();
        w.LocalGet(new(6)).ArrayNewDefault(DelegateListType).LocalSet(new(7)).I32Const(0).LocalSet(new(5));
        w.Block(EmptyBlock).Loop().LocalGet(new(5)).LocalGet(new(6)).I32LtU().I32Eqz().BrIf(new(1));
        w.LocalGet(new(7)).RefCast(DelegateListType).LocalGet(new(5)).LocalGet(new(0));
        // Skip the removed range when reading the original immutable list.
        w.LocalGet(new(5)).LocalGet(new(3)).I32Add().LocalGet(new(5))
            .LocalGet(new(5)).LocalGet(new(4)).I32LtU().I32Eqz().Select().Call(Fn(DelegateHelper.Element)).ArraySet(DelegateListType);
        w.LocalGet(new(5)).I32Const(1).I32Add().LocalSet(new(5)).Br(new(0)).End().End();
        NewInvocationList(w, 0, 7);
    }

    public ValueTypeCode[] DelegateInvokeLocals(MethodPlan method) => method.Signature.ReturnType == SignatureTypeCode.Void
        ? [EqRef, I32, I32] : method.Signature.Result.IsByReference
        ? [EqRef, I32, I32, EqRef, I32] : [EqRef, I32, I32, method.Signature.Result.WasmType];

    public void EmitDelegateInvoke(Writer w, MethodPlan method)
    {
        var signature = method.Signature;
        uint first = (uint)signature.WasmParameterCount;
        LocalIndex current = new(first), index = new(first + 1), count = new(first + 2), result = new(first + 3);
        w.LocalGet(new(0)).Call(DelegateFunctions[DelegateHelper.Count].Function).LocalSet(count).Loop();
        w.LocalGet(new(0)).LocalGet(index).Call(DelegateFunctions[DelegateHelper.Element].Function).LocalSet(current);
        DelegateField(w, current.Value, Target);
        for (int i = 1; i < signature.WasmParameterCount; i++)
        {
            if (i < signature.Parameters.Length && signature.Parameters[i] is { UsesGc: true, IsByReference: false, Identity: { IsValueType: true } identity })
            {
                // Each handler receives its own by-value GC struct copy; ref arguments stay aliased.
                var shape = Shape(identity);
                for (int field = 0; field < shape.Leaves.Count; field++)
                    w.LocalGet(new((uint)i)).RefCast(shape.Type).StructGet(shape.Type, new((uint)field));
                w.StructNew(shape.Type);
            }
            else w.LocalGet(new((uint)i));
        }
        DelegateField(w, current.Value, Callable).CallIndirect(method.Type);
        if (signature.Result.IsByReference) w.LocalSet(new(first + 4));
        if (signature.ReturnType != SignatureTypeCode.Void) w.LocalSet(result);
        w.LocalGet(index).I32Const(1).I32Add().LocalTee(index).LocalGet(count).I32LtU().BrIf(new(0)).End();
        if (signature.ReturnType != SignatureTypeCode.Void) w.LocalGet(result);
        if (signature.Result.IsByReference) w.LocalGet(new(first + 4));
    }
}
