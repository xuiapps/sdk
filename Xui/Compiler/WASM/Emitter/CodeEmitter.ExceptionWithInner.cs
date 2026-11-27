using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitExceptionWithInner(ExceptionKind kind, Instruction.Token instruction, int offset)
    {
        _ = Pop(instruction.Code, offset);
        writer.LocalSet(method.GcSource);
        Pop(System.Reflection.Metadata.SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.GcExtern);
        if (kind == ExceptionKind.AggregateException)
        {
            writer.LocalGet(method.GcSource).RefIsNull().If();
            ThrowArgumentException(ExceptionKind.ArgumentNullException, "innerException");
            writer.End();
        }
        writer.I32Const((int)kind).LocalGet(method.GcExtern).RefIsNull().If();
        EmitString(ExceptionTypes.Message(kind));
        writer.LocalSet(method.GcExtern).End().LocalGet(method.GcExtern);
        if (method.Storage.ArgumentExceptionsEnabled) writer.RefNull(ValueTypeCode.ExternRef);
        writer.LocalGet(method.GcSource);
        if (method.Storage.ActualValueExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        method.Storage.WriteXmlExceptionDefaults(writer);
        writer.StructNew(method.Storage.ExceptionType);
    }
}
