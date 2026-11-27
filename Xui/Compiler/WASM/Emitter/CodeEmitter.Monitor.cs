using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryMonitor(Instruction.Token instruction, int offset)
    {
        if (MonitorMethod.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } monitor) return false;
        var storage = method.Storage;
        var boolean = new LocalVariable(SignatureTypeCode.Boolean);
        if (monitor.HasLockTaken)
        {
            PopAssignable(boolean with { IsByReference = true }, instruction.Code, offset);
            writer.LocalSet(method.GcI32).LocalSet(method.GcOwner);
        }
        if (stack.Peek().TypeCode == SignatureTypeCode.String)
            throw new NotSupportedException("Monitors on native strings require managed string identity.");
        PopAssignable(new(SignatureTypeCode.Object, UsesGc: true), instruction.Code, offset);
        writer.LocalSet(method.GcSource);
        if (monitor.HasLockTaken)
        {
            // Like CLR Monitor.Enter, a pre-set flag fails before acquiring anything.
            writer.LocalGet(method.GcOwner).LocalGet(method.GcI32).Call(storage.ReferenceAccessor(boolean, store: false)).If();
            ThrowArgumentException(ExceptionKind.ArgumentException, "lockTaken", MonitorRuntime.InvalidLockTakenMessage);
            writer.End();
        }
        writer.LocalGet(method.GcSource).RefIsNull().If();
        if (monitor.Operation == "IsEntered") ThrowArgumentException(ExceptionKind.ArgumentNullException, "obj");
        else ThrowException(ExceptionKind.ArgumentNullException);
        writer.End();
        // Object-typed native strings are canonicalized carriers, not CLR string identities.
        if (storage.StringObjects.Shape is { } strings)
        {
            writer.LocalGet(method.GcSource).RefTest(strings.Type).If();
            ThrowException(ExceptionKind.NotSupportedException);
            writer.End();
        }
        writer.LocalGet(method.GcSource).I32Const(monitor.Operation switch { "Enter" => 0, "Exit" => 1, "IsEntered" => 2, _ => throw new InvalidOperationException() })
            .Call(storage.Monitors.Change);
        if (monitor.Operation == "IsEntered") stack.Push(new(SignatureTypeCode.Int32));
        else if (monitor.Operation == "Exit")
        {
            writer.I32Eqz().If();
            ThrowException(ExceptionKind.SynchronizationLockException);
            writer.End();
        }
        else
        {
            writer.Drop();
            if (monitor.HasLockTaken)
                writer.LocalGet(method.GcOwner).LocalGet(method.GcI32).I32Const(1).Call(storage.ReferenceAccessor(boolean, store: true));
        }
        return true;
    }
}
