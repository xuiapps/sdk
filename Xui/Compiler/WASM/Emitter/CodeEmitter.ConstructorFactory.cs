using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryConstructorFactory(Instruction.Token instruction, int offset)
    {
        var target = ConstructorFactory.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (target is not { } constructor) return false;
        var storage = method.Storage;
        var shape = storage.Shape(constructor.DeclaringType);
        var instance = method.CallTemporaries[constructor][0];
        writer.StructNewDefault(shape.Type).LocalTee(instance).RefCast(shape.Type)
            .GlobalGet(storage.Classes.Resolve(constructor.DeclaringType).Descriptor).StructSet(shape.Type, new(0));

        // Activator/new T() wraps managed constructor failures, unlike a direct newobj.
        int done = OpenExceptionLabel(storage.EmptyBlock);
        OpenExceptionLabel(storage.ExceptionPayloadBlock);
        writer.TryCatch(storage.ExceptionTag, new(0)); labels.Push((null, default));
        writer.LocalGet(instance).Call(methods[constructor].Function);
        CloseExceptionLabel();
        BranchExceptionLabel(done);
        CloseExceptionLabel();
        writer.LocalSet(method.GcSource).I32Const((int)ExceptionKind.TargetInvocationException);
        EmitString(ExceptionTypes.Message(ExceptionKind.TargetInvocationException));
        if (storage.ArgumentExceptionsEnabled) writer.RefNull(ValueTypeCode.ExternRef);
        writer.LocalGet(method.GcSource).StructNew(storage.ExceptionType).Throw(storage.ExceptionTag);
        CloseExceptionLabel();
        writer.LocalGet(instance);
        stack.Push(new(SignatureTypeCode.TypeHandle, constructor.DeclaringType));
        return true;
    }
}
