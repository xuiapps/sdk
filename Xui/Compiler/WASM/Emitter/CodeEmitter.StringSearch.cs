using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool EmitStringSearch(StringAccess call, Instruction.Token instruction, int offset)
    {
        var runtime = method.Storage.StringAccess;
        var (text, value) = method.EqualityTemporaries[new(SignatureTypeCode.String)];
        if (call.HasComparison)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(method.StringCount);
        }
        bool character = call.Signature.Parameters[0].Code == SignatureTypeCode.Char;
        Pop(character ? SignatureTypeCode.Int32 : SignatureTypeCode.String, instruction.Code, offset);
        if (character) writer.Call(runtime.FromChar);
        writer.LocalSet(value);
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(text).LocalGet(text).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End().LocalGet(value).RefIsNull().If();
        ThrowStringArgument(ExceptionKind.ArgumentNullException, "value");
        writer.End();
        if (call.HasComparison)
        {
            writer.LocalGet(method.StringCount).I32Const((int)StringComparison.OrdinalIgnoreCase).I32GtU().If();
            ThrowStringArgument(ExceptionKind.ArgumentException, "comparisonType");
            writer.End().LocalGet(method.StringCount).I32Const((int)StringComparison.Ordinal).I32Eq().I32Eqz().If();
            ThrowException(ExceptionKind.NotSupportedException);
            writer.End();
        }
        writer.LocalGet(text).LocalGet(value).I32Const((int)call.Search!.Value).Call(runtime.Search);
        if (call.Operation == "Contains") writer.I32Const(-1).I32Eq().I32Eqz();
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
