using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringAccess(Instruction.Token instruction, int offset)
    {
        if (CharFormatting.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is { } character)
        {
            if (character.HasThis) ReadScalarAddress(Pop(instruction.Code, offset), new(SignatureTypeCode.Char));
            else Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.Call(method.Storage.StringAccess.FromChar);
            stack.Push(new(SignatureTypeCode.String));
            return true;
        }
        var call = StringAccess.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (call is null) return false;
        if (call.Operation == "Replace") return EmitStringReplace(call, instruction, offset);
        if (call.Search is not null) return EmitStringSearch(call, instruction, offset);
        if (call.Operation is "IndexOf" or "Contains") return EmitStringIndexOf(call, instruction, offset);
        var runtime = method.Storage.StringAccess;
        if (call.Signature.Parameters.Length == 2)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(method.StringCount);
        }
        if (call.Signature.Parameters.Length > 0)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(method.StringIndex);
        }
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End();
        if (call.Operation == "get_Length") writer.LocalGet(method.GcExtern).Call(runtime.Length);
        else if (call.Operation == "ToLowerInvariant") writer.LocalGet(method.GcExtern).Call(runtime.LowerInvariant);
        else if (call.Operation == "get_Chars")
        {
            writer.LocalGet(method.StringIndex).LocalGet(method.GcExtern).Call(runtime.Length).I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException);
            writer.End().LocalGet(method.GcExtern).LocalGet(method.StringIndex).Call(runtime.Character);
        }
        else
        {
            writer.LocalGet(method.StringIndex).LocalGet(method.GcExtern).Call(runtime.Length).I32GtU().If();
            ThrowStringArgument(ExceptionKind.ArgumentOutOfRangeException, "startIndex");
            writer.End();
            if (call.Signature.Parameters.Length == 2)
            {
                writer.LocalGet(method.StringCount).LocalGet(method.GcExtern).Call(runtime.Length).LocalGet(method.StringIndex).I32Sub().I32GtU().If();
                ThrowStringArgument(ExceptionKind.ArgumentOutOfRangeException, "length");
                writer.End();
                writer.LocalGet(method.GcExtern).LocalGet(method.StringIndex).LocalGet(method.StringIndex).LocalGet(method.StringCount).I32Add();
            }
            else writer.LocalGet(method.GcExtern).LocalGet(method.StringIndex).LocalGet(method.GcExtern).Call(runtime.Length);
            writer.Call(runtime.Slice);
        }
        stack.Push(new(call.Signature.Result.Code == SignatureTypeCode.Char ? SignatureTypeCode.Int32 : call.Signature.Result.Code));
        return true;
    }

    private bool EmitStringIndexOf(StringAccess call, Instruction.Token instruction, int offset)
    {
        var runtime = method.Storage.StringAccess;
        int count = call.Signature.Parameters.Length;
        if (count == 3)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(method.StringCount);
        }
        if (count >= 2) Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        else writer.I32Const(0);
        writer.LocalSet(method.StringIndex);
        Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        writer.LocalSet(method.GcI32);
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End().LocalGet(method.StringIndex).LocalGet(method.GcExtern).Call(runtime.Length).I32GtU().If();
        ThrowStringArgument(ExceptionKind.ArgumentOutOfRangeException, "startIndex");
        writer.End();
        if (count == 3)
        {
            writer.LocalGet(method.StringCount).LocalGet(method.GcExtern).Call(runtime.Length).LocalGet(method.StringIndex).I32Sub().I32GtU().If();
            ThrowStringArgument(ExceptionKind.ArgumentOutOfRangeException, "count");
            writer.End().LocalGet(method.StringCount).LocalGet(method.StringIndex).I32Add();
        }
        else writer.LocalGet(method.GcExtern).Call(runtime.Length);
        writer.LocalSet(method.StringCount).Block(method.Storage.EmptyBlock).Loop();
        writer.LocalGet(method.StringIndex).LocalGet(method.StringCount).I32LtU().I32Eqz().If()
            .I32Const(-1).LocalSet(method.StringIndex).Br(new(2)).End();
        writer.LocalGet(method.GcExtern).LocalGet(method.StringIndex).Call(runtime.Character).LocalGet(method.GcI32).I32Eq().BrIf(new(1));
        writer.LocalGet(method.StringIndex).I32Const(1).I32Add().LocalSet(method.StringIndex).Br(new(0)).End().End();
        writer.LocalGet(method.StringIndex);
        if (call.Operation == "Contains") writer.I32Const(-1).I32Eq().I32Eqz();
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
