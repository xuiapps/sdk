using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitArraySpan(SpanAccess access, OpCode code, int offset)
    {
        bool range = access.Signature.Parameters.Length == 3;
        if (range)
        {
            Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(method.GcI32);
            Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(Frame.I32Scratch);
        }
        else writer.I32Const(0).LocalSet(Frame.I32Scratch);
        PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), code, offset);
        var shape = method.Storage.ArrayShape(access.Signature.Parameters[0].Array!);
        writer.LocalSet(method.ArrayReference).LocalGet(method.ArrayReference).RefIsNull().If(method.Storage.AddressIndexType)
            .I32Const(0).Else().LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().End().LocalSet(method.CopyLength);
        if (range)
        {
            writer.LocalGet(Frame.I32Scratch).LocalGet(method.CopyLength).I32GtU()
                .LocalGet(method.GcI32).LocalGet(method.CopyLength).LocalGet(Frame.I32Scratch).I32Sub().I32GtU().I32Or().If();
            ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        }
        else writer.LocalGet(method.CopyLength).LocalSet(method.GcI32);
        bool suppliedAddress = access.Signature.HasThis && code == OpCode.Call;
        if (suppliedAddress) ReadSpanAddress(access, code, offset);
        else FrameAddress(Frame.Values[offset]);
        writer.LocalSet(Frame.AddressScratch);
        writer.LocalGet(Frame.AddressScratch).I32Load().LocalGet(method.ArrayReference).TableSet(Runtime.ObjectReferences!.Value);
        writer.LocalGet(Frame.AddressScratch).I32Const(4).I32Add().LocalGet(Frame.I32Scratch)
            .I32Const(ElementLeaves(shape.Element)).I32Mul().I32Store();
        writer.LocalGet(Frame.AddressScratch).I32Const(8).I32Add().LocalGet(method.GcI32).I32Store();
        if (!suppliedAddress)
        {
            writer.LocalGet(Frame.AddressScratch);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
        }
    }
}
