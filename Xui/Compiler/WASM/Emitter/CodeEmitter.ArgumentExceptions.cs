using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitArgumentException(ExceptionKind kind, ExceptionMember member, Instruction.Token instruction, int offset)
    {
        if (member == ExceptionMember.ParameterValueMessageConstructor)
        {
            Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(method.ExceptionMessage);
            PopAssignable(new(SignatureTypeCode.Object, UsesGc: true), instruction.Code, offset); writer.LocalSet(method.ExceptionActualValue);
            Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(method.ExceptionParamName);
            writer.LocalGet(method.ExceptionMessage).LocalSet(method.GcExtern);
            WriteArgumentException(kind, suppliedActualValue: true);
            return;
        }
        // ArgumentNullException takes (paramName, message); ArgumentException reverses those strings.
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        if (member == ExceptionMember.ParameterMessageConstructor)
        {
            writer.LocalSet(method.GcExtern);
            Pop(SignatureTypeCode.String, instruction.Code, offset);
            writer.LocalSet(method.ExceptionParamName);
        }
        else
        {
            writer.LocalSet(method.ExceptionParamName);
            if (member == ExceptionMember.MessageParameterConstructor)
            {
                Pop(SignatureTypeCode.String, instruction.Code, offset);
                writer.LocalSet(method.GcExtern);
            }
            else
            {
                EmitString(ExceptionTypes.Message(kind));
                writer.LocalSet(method.GcExtern);
            }
        }
        WriteArgumentException(kind);
    }

    private void WriteArgumentException(ExceptionKind kind, bool suppliedActualValue = false)
    {
        writer.LocalGet(method.GcExtern).RefIsNull().If();
        EmitString(ExceptionTypes.Message(kind));
        writer.LocalSet(method.GcExtern).End();
        writer.LocalGet(method.ExceptionParamName).RefIsNull().I32Eqz().If();
        writer.LocalGet(method.ExceptionParamName).Call(Runtime.StringLength!.Value).If();
        writer.LocalGet(method.GcExtern);
        EmitString(kind == ExceptionKind.ObjectDisposedException ? "\nObject name: '" : " (Parameter '");
        writer.Call(Runtime.StringConcat!.Value).LocalGet(method.ExceptionParamName).Call(Runtime.StringConcat.Value);
        EmitString(kind == ExceptionKind.ObjectDisposedException ? "'." : "')");
        writer.Call(Runtime.StringConcat.Value).LocalSet(method.GcExtern).End().End();
        writer.I32Const((int)kind).LocalGet(method.GcExtern).LocalGet(method.ExceptionParamName);
        if (method.Storage.InnerExceptionsEnabled) writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
        if (method.Storage.ActualValueExceptionsEnabled)
        {
            if (suppliedActualValue) writer.LocalGet(method.ExceptionActualValue);
            else writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
        }
        method.Storage.WriteXmlExceptionDefaults(writer);
        writer.StructNew(method.Storage.ExceptionType);
    }
    private void ThrowStringArgument(ExceptionKind kind, string parameter)
    {
        EmitString(parameter); writer.LocalSet(method.ExceptionParamName);
        EmitString(ExceptionTypes.Message(kind)); writer.LocalSet(method.GcExtern);
        WriteArgumentException(kind);
        writer.Throw(method.Storage.ExceptionTag);
    }
}
