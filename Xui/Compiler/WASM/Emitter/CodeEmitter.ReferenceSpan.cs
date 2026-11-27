using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitReferenceSpan(SpanAccess access, OpCode code, int offset)
    {
        PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), code, offset);
        writer.LocalSet(method.SourceSelector).LocalSet(method.SourceOwner);
        bool suppliedAddress = code == OpCode.Call;
        if (suppliedAddress) ReadSpanAddress(access, code, offset);
        else FrameAddress(Frame.Values[offset]);
        writer.LocalSet(Frame.AddressScratch);
        writer.LocalGet(Frame.AddressScratch).I32Load().LocalGet(method.SourceOwner).TableSet(Runtime.ObjectReferences!.Value);
        writer.LocalGet(Frame.AddressScratch).I32Const(4).I32Add();
        if (StorageModel.IsReference(method.Storage.Bind(access.Owner.Arguments!.Values[0])))
            writer.LocalGet(method.SourceOwner).RefIsNull().If(method.Storage.AddressIndexType)
                .LocalGet(method.SourceSelector).I32Const(ReferenceSlotSelector).I32Or()
                .Else().LocalGet(method.SourceSelector).End();
        else writer.LocalGet(method.SourceSelector);
        writer.I32Store();
        writer.LocalGet(Frame.AddressScratch).I32Const(8).I32Add().I32Const(1).I32Store();
        if (!suppliedAddress)
        {
            writer.LocalGet(Frame.AddressScratch);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
        }
    }
}
