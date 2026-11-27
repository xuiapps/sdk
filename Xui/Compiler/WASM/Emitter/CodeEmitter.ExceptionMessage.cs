using Xui.Compiler.XuiNet.DotNet.Exceptions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Preserve ActualValue until Message is requested; construction never invokes value formatting.
    private void EmitExceptionMessage()
    {
        var storage = method.Storage;
        var runtime = storage.Formatting;
        writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefCast(storage.ExceptionType)
            .StructGet(storage.ExceptionType, new(1)).LocalSet(method.ExceptionMessage);
        writer.LocalGet(method.GcOwner).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(storage.ActualValueField))
            .LocalTee(method.ExceptionActualValue).RefIsNull().I32Eqz().If();
        writer.I32Const(1).Call(runtime.Start).LocalSet(method.FormatState);
        writer.LocalGet(method.FormatState).I32Const(0).LocalGet(method.ExceptionActualValue).Call(runtime.BoxValue).Call(runtime.Set);
        EmitString("\nActual value was {0}.");
        writer.LocalGet(method.FormatState).Call(runtime.Finish).LocalSet(method.GcI32).LocalSet(method.GcExtern);
        writer.LocalGet(method.GcI32).I32Const(1).I32Eq().If(); ThrowException(ExceptionKind.FormatException); writer.End();
        writer.LocalGet(method.GcI32).I32Const(2).I32Eq().If(); ThrowException(ExceptionKind.NotSupportedException); writer.End();
        writer.LocalGet(method.ExceptionMessage).LocalGet(method.GcExtern).Call(Runtime.StringConcat!.Value).LocalSet(method.ExceptionMessage);
        writer.End().LocalGet(method.ExceptionMessage);
    }
}
