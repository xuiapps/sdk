using Xui.Compiler.XuiNet.DotNet.Exceptions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // AggregateException.Message reads inner messages on demand, including nested
    // aggregates and mutable boxed ActualValue payloads. Construction does not format.
    private void EmitAggregateMessage()
    {
        var storage = method.Storage;
        writer.LocalSet(method.GcOwner);
        EmitString(""); writer.LocalSet(method.AggregateMessage);
        writer.I32Const(0).LocalSet(method.AggregateDepth).Block(storage.EmptyBlock).Loop(storage.EmptyBlock);
        writer.LocalGet(method.GcOwner);
        if (storage.ActualValueExceptionsEnabled) EmitExceptionMessage();
        else writer.RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(1));
        writer.LocalSet(method.GcExtern).LocalGet(method.AggregateMessage).LocalGet(method.GcExtern)
            .Call(Runtime.StringConcat!.Value).LocalSet(method.AggregateMessage);
        writer.LocalGet(method.GcOwner).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(0))
            .I32Const((int)ExceptionKind.AggregateException).I32Eq().I32Eqz().BrIf(new(1));
        writer.LocalGet(method.GcOwner).RefCast(storage.ExceptionType).StructGet(storage.ExceptionType, new(storage.InnerExceptionField))
            .LocalTee(method.GcOwner).RefIsNull().BrIf(new(1));
        writer.LocalGet(method.AggregateMessage); EmitString(" (");
        writer.Call(Runtime.StringConcat!.Value).LocalSet(method.AggregateMessage)
            .LocalGet(method.AggregateDepth).I32Const(1).I32Add().LocalSet(method.AggregateDepth).Br(new(0)).End().End();
        writer.Block(storage.EmptyBlock).Loop(storage.EmptyBlock).LocalGet(method.AggregateDepth).I32Eqz().BrIf(new(1));
        writer.LocalGet(method.AggregateMessage); EmitString(")");
        writer.Call(Runtime.StringConcat!.Value).LocalSet(method.AggregateMessage)
            .LocalGet(method.AggregateDepth).I32Const(1).I32Sub().LocalSet(method.AggregateDepth).Br(new(0)).End().End()
            .LocalGet(method.AggregateMessage);
    }
}
