using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitSpanFill(SpanAccess access, OpCode code, int offset)
    {
        var element = access.Owner.Arguments!.Values[0];
        var storage = method.Storage.Bind(element);
        bool clear = access.Operation == "Clear";
        if (!clear)
        {
            PopAssignable(storage, code, offset);
            StorePrimitive(storage, method.EqualityTemporaries[storage].Left);
        }
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(Frame.AddressScratch).I32Const(8).I32Add().I32Load().LocalSet(method.CopyLength);
        ReadSpanBacking(Frame.AddressScratch, method.AddressOwner, method.AddressSelector);
        writer.I32Const(0).LocalSet(Frame.AddressScratch);
        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        if (storage.Identity is { IsValueType: true } identity)
        {
            var leaves = method.Storage.Shape(identity).Leaves;
            for (int i = 0; i < leaves.Count; i++) FillSpanLeaf(element, leaves[i].Storage, true, leaves[i].MemoryOffset, i);
        }
        else FillSpanLeaf(element, storage, clear);
        writer.LocalGet(Frame.AddressScratch).I32Const(1).I32Add().LocalSet(Frame.AddressScratch);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
    }

    private void FillSpanLeaf(SignatureType element, LocalVariable storage, bool clear, int byteOffset = 0, int leafOffset = 0)
    {
        ProjectSpanElement(method.AddressOwner, method.AddressSelector, Frame.AddressScratch, element, storage, byteOffset, leafOffset);
        if (clear) DefaultValue(storage);
        else writer.LocalGet(method.EqualityTemporaries[storage].Left);
        writer.Call(method.Storage.ReferenceAccessor(storage, true));
    }
}
