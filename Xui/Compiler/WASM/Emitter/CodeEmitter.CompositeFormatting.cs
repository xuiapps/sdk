using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryCompositeFormatting(Instruction.Token instruction, int offset)
    {
        var call = CompositeFormatting.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (call is null) return false;
        int count = call.Signature.Parameters.Length - (call.HasProvider ? 2 : 1);
        for (int i = count - 1; i >= 0; i--)
        {
            if (!call.HasArray && stack.Peek().TypeCode == SignatureTypeCode.String)
            {
                Pop(SignatureTypeCode.String, instruction.Code, offset);
                writer.Call(method.Storage.Formatting.TextValue).LocalSet(method.FormatArguments[i]);
            }
            else
            {
                PopAssignable(method.Storage.Bind(call.Signature.Parameters[i + (call.HasProvider ? 2 : 1)]), instruction.Code, offset);
                if (call.HasArray) writer.LocalSet(method.ArrayReference);
                else writer.Call(method.Storage.Formatting.BoxValue).LocalSet(method.FormatArguments[i]);
            }
        }
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.FormatText);
        if (call.HasProvider)
        {
            PopAssignable(method.Storage.Bind(call.Signature.Parameters[0]), instruction.Code, offset);
            writer.LocalSet(method.GcSource);
        }
        writer.LocalGet(method.FormatText).RefIsNull().If();
        ThrowFormatArgumentNull("format");
        writer.End();
        if (call.HasArray)
        {
            writer.LocalGet(method.ArrayReference).RefIsNull().If();
            ThrowFormatArgumentNull("args");
            writer.End();
        }
        if (call.HasProvider)
        {
            writer.LocalGet(method.GcSource).RefIsNull().I32Eqz().If();
            ThrowException(ExceptionKind.NotSupportedException);
            writer.End();
        }
        var runtime = method.Storage.Formatting;
        if (call.HasArray)
        {
            var array = method.Storage.ArrayShape(call.Signature.Parameters[^1].Array!);
            writer.LocalGet(method.ArrayReference).RefCast(array.Type).ArrayLen().Call(runtime.Start).LocalSet(method.FormatState);
            writer.I32Const(0).LocalSet(method.ArrayIndex).Block(method.Storage.EmptyBlock).Loop();
            writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(array.Type).ArrayLen().I32LtU().I32Eqz().BrIf(new(1));
            writer.LocalGet(method.FormatState).LocalGet(method.ArrayIndex)
                .LocalGet(method.ArrayReference).RefCast(array.Type).LocalGet(method.ArrayIndex).ArrayGet(array.Type)
                .Call(runtime.BoxValue).Call(runtime.Set);
            writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0)).End().End();
        }
        else
        {
            writer.I32Const(count).Call(runtime.Start).LocalSet(method.FormatState);
            for (int i = 0; i < count; i++)
                writer.LocalGet(method.FormatState).I32Const(i).LocalGet(method.FormatArguments[i]).Call(runtime.Set);
        }
        writer.LocalGet(method.FormatText).LocalGet(method.FormatState).Call(runtime.Finish)
            .LocalSet(method.GcI32).LocalSet(method.GcExtern);
        writer.LocalGet(method.GcI32).I32Const(1).I32Eq().If();
        ThrowException(ExceptionKind.FormatException);
        writer.End().LocalGet(method.GcI32).I32Const(2).I32Eq().If();
        ThrowException(ExceptionKind.NotSupportedException);
        writer.End().LocalGet(method.GcExtern);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }

    private void ThrowFormatArgumentNull(string name)
    {
        EmitString(name); writer.LocalSet(method.ExceptionParamName);
        EmitString(ExceptionTypes.Message(ExceptionKind.ArgumentNullException)); writer.LocalSet(method.GcExtern);
        WriteArgumentException(ExceptionKind.ArgumentNullException);
        writer.Throw(method.Storage.ExceptionTag);
    }
}
