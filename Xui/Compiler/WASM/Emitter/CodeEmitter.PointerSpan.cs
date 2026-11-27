using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitPointerSpan(SpanAccess access, OpCode code, int offset)
    {
        Pop(SignatureTypeCode.Int32, code, offset);
        writer.LocalSet(method.CopyLength);
        PopAssignable(new(SignatureTypeCode.IntPtr), code, offset);
        writer.LocalSet(Frame.I32Scratch).LocalGet(method.CopyLength).I32Const(0).I32LtS().If();
        ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        bool suppliedAddress = code == OpCode.Call;
        if (suppliedAddress) ReadSpanAddress(access, code, offset);
        else FrameAddress(Frame.Values[offset]);
        writer.LocalSet(Frame.AddressScratch);
        writer.LocalGet(Frame.AddressScratch).I32Load().RefNull(ValueTypeCode.EqRef).TableSet(Runtime.ObjectReferences!.Value);
        writer.LocalGet(Frame.AddressScratch).I32Const(4).I32Add().LocalGet(Frame.I32Scratch).I32Store();
        writer.LocalGet(Frame.AddressScratch).I32Const(8).I32Add().LocalGet(method.CopyLength).I32Store();
        if (!suppliedAddress)
        {
            writer.LocalGet(Frame.AddressScratch);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
        }
    }
}
