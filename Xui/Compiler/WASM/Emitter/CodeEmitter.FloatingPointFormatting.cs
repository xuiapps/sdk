using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitInvariantFloatingPointFormatting(Instruction.Token instruction, int offset, SignatureTypeCode type)
    {
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.GcExtern);
        ReadScalarAddress(Pop(instruction.Code, offset), new(type));
        if (type == SignatureTypeCode.Single) writer.F32DemoteF64().F64PromoteF32();
        writer.LocalGet(method.GcExtern);
        FormatFloatingPoint(type == SignatureTypeCode.Single);
        stack.Push(new(SignatureTypeCode.String));
    }

    // Receives the scalar and format; leaves a JS string or throws a managed exception.
    private void FormatFloatingPoint(bool single)
    {
        writer.Call(single ? method.Storage.Formatting.Single : method.Storage.Formatting.Double);
        ReadFormattedResult();
    }

    private void ReadFormattedResult()
    {
        writer.LocalSet(method.GcI32).LocalSet(method.GcExtern);
        writer.LocalGet(method.GcI32).I32Const(1).I32Eq().If();
        ThrowException(ExceptionKind.FormatException);
        writer.End().LocalGet(method.GcI32).I32Const(2).I32Eq().If();
        ThrowException(ExceptionKind.NotSupportedException);
        writer.End().LocalGet(method.GcExtern);
    }
}
