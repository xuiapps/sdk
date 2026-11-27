using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringSpan(Instruction.Token instruction, int offset)
    {
        var access = StringSpan.Read(source,MetadataTokens.EntityHandle(instruction.Value));
        if (access is null) return false;
        if (access.Constructor)
        {
            if(instruction.Code!=OpCode.Newobj) throw new NotSupportedException("String span construction requires newobj.");
            PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]),instruction.Code,offset);
            EmitCharacterSpanString();
            return true;
        }
        int count = access.Signature.Parameters.Length;
        if (count==3) { Pop(SignatureTypeCode.Int32,instruction.Code,offset); writer.LocalSet(method.CopyLength); }
        if (count>=2) Pop(SignatureTypeCode.Int32,instruction.Code,offset);
        else writer.I32Const(0);
        writer.LocalSet(Frame.I32Scratch);
        Pop(SignatureTypeCode.String,instruction.Code,offset);
        writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If(method.Storage.AddressIndexType)
            .I32Const(0).Else().LocalGet(method.GcExtern).Call(method.Storage.StringAccess.Length).End().LocalSet(method.GcI32);
        writer.LocalGet(Frame.I32Scratch).LocalGet(method.GcI32).I32GtU().If();
        ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        writer.LocalGet(method.GcI32).LocalGet(Frame.I32Scratch).I32Sub();
        if (count==3)
        {
            writer.LocalGet(method.CopyLength).I32LtU().If();
            ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        }
        else writer.LocalSet(method.CopyLength);
        writer.LocalGet(method.GcExtern); EmitStringObject(); writer.LocalSet(method.GcOwner);
        FrameAddress(Frame.Values[offset]); writer.I32Load().LocalGet(method.GcOwner).TableSet(Runtime.ObjectReferences!.Value);
        FrameAddress(Frame.Values[offset]+4); writer.LocalGet(Frame.I32Scratch).I32Store();
        FrameAddress(Frame.Values[offset]+8); writer.LocalGet(method.CopyLength).I32Store();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle,access.Signature.Result.Identity));
        return true;
    }

    private void EmitSpanString(SpanAccess access, OpCode code, int offset)
    {
        ReadSpanAddress(access,code,offset);
        EmitCharacterSpanString();
    }

    private void EmitCharacterSpanString()
    {
        writer.LocalTee(Frame.AddressScratch).I32Const(8).I32Add().I32Load().LocalSet(method.CopyLength);
        ReadSpanBacking(Frame.AddressScratch,method.SourceOwner,method.SourceSelector);
        writer.LocalGet(method.CopyLength).ArrayNewDefault(Runtime.Utf16ArrayType).LocalSet(method.GcSource);
        writer.I32Const(0).LocalSet(Frame.I32Scratch).Block(method.Storage.EmptyBlock).Loop();
        writer.LocalGet(Frame.I32Scratch).LocalGet(method.CopyLength).I32LtU().I32Eqz().BrIf(new(1));
        writer.LocalGet(method.GcSource).RefCast(Runtime.Utf16ArrayType).LocalGet(Frame.I32Scratch);
        ProjectSpanElement(method.SourceOwner,method.SourceSelector,Frame.I32Scratch,new(SignatureTypeCode.Char),new(SignatureTypeCode.Char));
        writer.Call(method.Storage.ReferenceAccessor(new(SignatureTypeCode.Char),false)).ArraySet(Runtime.Utf16ArrayType);
        writer.LocalGet(Frame.I32Scratch).I32Const(1).I32Add().LocalSet(Frame.I32Scratch).Br(new(0)).End().End();
        writer.LocalGet(method.GcSource).RefCast(Runtime.Utf16ArrayType).I32Const(0).LocalGet(method.CopyLength).Call(Runtime.FromCharCodeArrayFunction);
        stack.Push(new(SignatureTypeCode.String));
    }
}
