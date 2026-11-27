using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitXmlException(Instruction.Token instruction, int offset)
    {
        var token = MetadataTokens.EntityHandle(instruction.Value);
        var signature = SignatureReader.Method(source, token.Kind == HandleKind.MemberReference
            ? source.GetMemberReference((MemberReferenceHandle)token).Signature
            : source.GetMethod((MethodDefinitionHandle)token).Signature);
        var parameters = signature.Parameters;
        bool resource = XmlExceptionMembers.IsResource(signature);
        bool array = resource && parameters[1].Array is not null;
        writer.I32Const(0).LocalSet(method.XmlLine).I32Const(0).LocalSet(method.XmlPosition);
        writer.RefNull(ValueTypeCode.ExternRef).LocalSet(method.XmlSourceUri);
        writer.RefNull(ValueTypeCode.EqRef).LocalSet(method.XmlInner);
        for (int i = parameters.Length - 1; i >= 0; i--)
        {
            PopAssignable(method.Storage.Bind(parameters[i]), instruction.Code, offset);
            if (i == 0) writer.LocalSet(method.FormatText);
            else if (i == 1 && resource) writer.LocalSet(array ? method.ArrayReference : method.XmlArgument);
            else if (XmlExceptionMembers.IsException(parameters[i])) writer.LocalSet(method.XmlInner);
            else if (parameters[i].Code == SignatureTypeCode.Int32)
                writer.LocalSet(i > 0 && parameters[i - 1].Code == SignatureTypeCode.Int32 ? method.XmlPosition : method.XmlLine);
            else writer.LocalSet(method.XmlSourceUri);
        }
        if (parameters.Length == 0) writer.RefNull(ValueTypeCode.ExternRef).LocalSet(method.FormatText);
        if (resource)
        {
            writer.LocalGet(method.FormatText).LocalSet(method.XmlResource);
            if (array)
            {
                writer.LocalGet(method.ArrayReference).RefIsNull().I32Eqz().If();
                var shape = method.Storage.ArrayShape(parameters[1].Array!);
                writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().Call(method.Storage.Formatting.Start).LocalSet(method.FormatState);
                writer.I32Const(0).LocalSet(method.ArrayIndex).Block(method.Storage.EmptyBlock).Loop();
                writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32LtU().I32Eqz().BrIf(new(1));
                writer.LocalGet(method.FormatState).LocalGet(method.ArrayIndex)
                    .LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex).ArrayGet(shape.Type)
                    .Call(method.Storage.Formatting.TextValue).Call(method.Storage.Formatting.Set);
                writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0)).End().End();
                FinishXmlFormat();
                writer.End();
            }
            else
            {
                writer.I32Const(1).Call(method.Storage.Formatting.Start).LocalSet(method.FormatState);
                writer.LocalGet(method.FormatState).I32Const(0).LocalGet(method.XmlArgument)
                    .Call(method.Storage.Formatting.TextValue).Call(method.Storage.Formatting.Set);
                FinishXmlFormat();
            }
        }
        else
        {
            writer.LocalGet(method.FormatText).RefIsNull().If();
            EmitString(ExceptionTypes.Message(ExceptionKind.XmlException));
            writer.LocalTee(method.FormatText).LocalSet(method.XmlResource).Else();
            EmitString("{0}"); writer.LocalSet(method.XmlResource).End();
        }
        // XML includes a position suffix only when the line number is nonzero.
        writer.LocalGet(method.XmlLine).If().LocalGet(method.FormatText);
        EmitString(" Line "); writer.Call(Runtime.StringConcat!.Value);
        writer.LocalGet(method.XmlLine).Call(method.Storage.Formatting.Int32).Call(Runtime.StringConcat.Value);
        EmitString(", position "); writer.Call(Runtime.StringConcat.Value);
        writer.LocalGet(method.XmlPosition).Call(method.Storage.Formatting.Int32).Call(Runtime.StringConcat.Value);
        EmitString("."); writer.Call(Runtime.StringConcat.Value).LocalSet(method.FormatText).End();
        writer.I32Const((int)ExceptionKind.XmlException).LocalGet(method.FormatText);
        if (method.Storage.ArgumentExceptionsEnabled) writer.RefNull(ValueTypeCode.ExternRef);
        writer.LocalGet(method.XmlInner);
        if (method.Storage.ActualValueExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        writer.LocalGet(method.XmlLine).LocalGet(method.XmlPosition).LocalGet(method.XmlSourceUri).LocalGet(method.XmlResource)
            .StructNew(method.Storage.ExceptionType);
    }

    private void FinishXmlFormat()
    {
        writer.LocalGet(method.FormatText).LocalGet(method.FormatState).Call(method.Storage.Formatting.Finish)
            .LocalSet(method.GcI32).LocalSet(method.FormatText);
        writer.LocalGet(method.GcI32).I32Const(1).I32Eq().If();
        ThrowException(ExceptionKind.FormatException); writer.End();
        writer.LocalGet(method.GcI32).I32Const(2).I32Eq().If();
        ThrowException(ExceptionKind.NotSupportedException); writer.End();
    }
}
