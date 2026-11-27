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
        if (access.HasComparison)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.StringCount);
        }
        Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(right);
        Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(left);
        if (access.Signature.HasThis)
        {
            writer.LocalGet(left).RefIsNull().If();
            ThrowException(ExceptionKind.NullReferenceException);
            writer.End();
        }
        if (access.HasComparison)
        {
            writer.LocalGet(method.StringCount).I32Const(5).I32GtU().If();
            ThrowArgumentException(ExceptionKind.ArgumentException, "comparisonType"); writer.End();
            writer.LocalGet(method.StringCount).I32Const(4).I32LtU().If();
            ThrowException(ExceptionKind.NotSupportedException); writer.End();
            writer.LocalGet(method.StringCount).I32Const(4).I32Eq().If();
            CompareStrings(left, right); writer.LocalSet(method.GcI32).Else();
            writer.LocalGet(left).LocalGet(right).Call(method.Storage.StringAccess.IgnoreCaseEquality).LocalSet(method.GcI32);
            writer.End().LocalGet(method.GcI32);
        }
        else CompareStrings(left, right);
        if (access.Operation == "op_Inequality") writer.I32Eqz();
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
