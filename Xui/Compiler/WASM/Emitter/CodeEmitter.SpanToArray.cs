using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitSpanToArray(SpanAccess access, OpCode code, int offset)
    {
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(Frame.AddressScratch).I32Const(8).I32Add().I32Load().LocalSet(method.CopyLength);
        ReadSpanBacking(Frame.AddressScratch, method.SourceOwner, method.SourceSelector);
        var array = method.Storage.ArrayShape(access.Signature.Result.Array!);
        writer.LocalGet(method.CopyLength);
        NewArray(array);
        writer.LocalSet(method.AddressOwner).I32Const(0).LocalSet(method.AddressSelector);
        CopySpanElements(array.Signature.Element, array.Element);
        writer.LocalGet(method.AddressOwner);
        stack.Push(new(SignatureTypeCode.SZArray, array: array.Signature));
    }
}
