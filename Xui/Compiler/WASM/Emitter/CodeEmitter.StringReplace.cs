using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool EmitStringReplace(StringAccess call, Instruction.Token instruction, int offset)
    {
        var runtime = method.Storage.StringAccess;
        var (oldValue, newValue) = method.EqualityTemporaries[new(SignatureTypeCode.String)];
        bool character = call.Signature.Parameters[0].Code == SignatureTypeCode.Char;
        Pop(character ? SignatureTypeCode.Int32 : SignatureTypeCode.String, instruction.Code, offset);
        if (character) writer.Call(runtime.FromChar);
        writer.LocalSet(newValue);
        Pop(character ? SignatureTypeCode.Int32 : SignatureTypeCode.String, instruction.Code, offset);
        if (character) writer.Call(runtime.FromChar);
        writer.LocalSet(oldValue);
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End();
        if (!character)
        {
            writer.LocalGet(oldValue).RefIsNull().If();
            ThrowStringArgument(ExceptionKind.ArgumentNullException, "oldValue");
            writer.End().LocalGet(oldValue).Call(runtime.Length).I32Eqz().If();
            ThrowStringArgument(ExceptionKind.ArgumentException, "oldValue");
            writer.End();
        }
        writer.LocalGet(method.GcExtern).LocalGet(oldValue).LocalGet(newValue).Call(runtime.Replace);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
