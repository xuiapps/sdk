using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryNullGuard(Instruction.Token instruction, int offset)
    {
        if (!NullGuard.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.ExceptionParamName);
        var value = Pop(instruction.Code, offset);
        if (value.IsByReference || value.TypeCode is not (SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray) &&
            value.Identity is not { IsValueType: false }) throw new InvalidProgramException("ThrowIfNull requires a reference value.");
        writer.RefIsNull().If();
        EmitString(ExceptionTypes.Message(ExceptionKind.ArgumentNullException));
        writer.LocalSet(method.GcExtern);
        WriteArgumentException(ExceptionKind.ArgumentNullException);
        writer.Throw(method.Storage.ExceptionTag).End();
        return true;
    }
}
