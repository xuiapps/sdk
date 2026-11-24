using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitArgumentException(ExceptionKind kind, ExceptionMember member, Instruction.Token instruction, int offset)
    {
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
        writer.LocalGet(method.GcExtern).RefIsNull().If();
        EmitString(ExceptionTypes.Message(kind));
        writer.LocalSet(method.GcExtern).End();
        writer.LocalGet(method.ExceptionParamName).RefIsNull().I32Eqz().If();
        writer.LocalGet(method.ExceptionParamName).Call(Runtime.StringLength!.Value).If();
        writer.LocalGet(method.GcExtern);
        EmitString(" (Parameter '");
        writer.Call(Runtime.StringConcat!.Value).LocalGet(method.ExceptionParamName).Call(Runtime.StringConcat.Value);
        EmitString("')");
        writer.Call(Runtime.StringConcat.Value).LocalSet(method.GcExtern).End().End();
        writer.I32Const((int)kind).LocalGet(method.GcExtern).LocalGet(method.ExceptionParamName).StructNew(method.Storage.ExceptionType);
    }
}
