using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringEquality(Instruction.Token instruction, int offset)
    {
        var access = StringEquality.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (access is null) return false;
        var (left, right) = method.EqualityTemporaries[new(SignatureTypeCode.String)];
        Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(right);
        Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(left);
        if (access.Signature.HasThis)
        {
            writer.LocalGet(left).RefIsNull().If();
            ThrowException(ExceptionKind.NullReferenceException);
            writer.End();
        }
        CompareStrings(left, right);
        if (access.Operation == "op_Inequality") writer.I32Eqz();
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
