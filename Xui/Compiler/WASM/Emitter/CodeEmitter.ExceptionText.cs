using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Formats supported payloads and inner chains. Managed source stack traces are not captured yet.
    private void EmitExceptionText()
    {
        var storage = method.Storage;
        writer.LocalSet(method.GcSource).RefNull(ValueTypeCode.ExternRef).LocalSet(method.ExceptionTextState);
        writer.Loop(storage.EmptyBlock);
        writer.LocalGet(method.ExceptionTextState);
        EmitExceptionTypeName();
        writer.LocalGet(method.GcSource);
        if (storage.ActualValueExceptionsEnabled) EmitExceptionMessage();
        else writer.RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(1));
        writer.LocalGet(method.GcSource).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(0))
            .I32Const((int)ExceptionKind.AggregateException).I32Eq().Call(storage.Formatting.ExceptionFrame)
            .LocalSet(method.ExceptionTextState);
        if (storage.InnerExceptionsEnabled)
            writer.LocalGet(method.GcSource).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(storage.InnerExceptionField))
                .LocalTee(method.GcSource).RefIsNull().I32Eqz().BrIf(new(0));
        writer.End().LocalGet(method.ExceptionTextState).Call(storage.Formatting.ExceptionFinish);
    }

    private void EmitExceptionTypeName()
    {
        var storage = method.Storage;
        EmitString(ExceptionTypes.TypeName(ExceptionKind.Exception));
        writer.LocalSet(method.GcExtern);
        foreach (var kind in Enum.GetValues<ExceptionKind>())
        {
            if (kind == ExceptionKind.Exception || kind > ExceptionKind.ArgumentException && !storage.ExtraExceptionKinds.Contains(kind)) continue;
            writer.LocalGet(method.GcSource).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(0))
                .I32Const((int)kind).I32Eq().If();
            EmitString(ExceptionTypes.TypeName(kind));
            writer.LocalSet(method.GcExtern).End();
        }
        writer.LocalGet(method.GcExtern);
    }
}
