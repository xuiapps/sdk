using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Runtime;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryManagedType(Instruction.Token instruction, int offset)
    {
        var access = ManagedType.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (access is null) return false;
        var storage = method.Storage;
        switch (access.Operation)
        {
            case ManagedTypeOperation.FromHandle:
                if (Pop(instruction.Code, offset).TypeToken is null)
                    throw new NotSupportedException("GetTypeFromHandle requires a compiler-known type token.");
                // The transient handle already is the canonical descriptor reference.
                stack.Push(new(SignatureTypeCode.TypeHandle, access.Signature.Result.Identity));
                break;
            case ManagedTypeOperation.GetType:
                var receiver = Pop(instruction.Code, offset);
                if (!receiver.IsNull && (receiver.IsByReference || receiver.Identity is not { } identity ||
                    !storage.IsClass(identity) || ManagedType.Is(identity) || DefaultEquality.Is(identity) || DefaultEquality.IsInterface(identity) ||
                    ExceptionTypes.Kind(identity) is not null || DelegateInfo.IsRuntimeBase(identity)))
                    throw new NotSupportedException("GetType currently requires a named class or interface receiver with a runtime descriptor.");
                CheckExceptionNull();
                writer.RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0));
                stack.Push(new(SignatureTypeCode.TypeHandle, access.Signature.Result.Identity));
                break;
            case ManagedTypeOperation.Name:
                var type = Pop(instruction.Code, offset);
                if (!type.IsNull && (type.IsByReference || type.Identity is not { } typeIdentity || !ManagedType.Is(typeIdentity)))
                    throw new NotSupportedException("MemberInfo.Name currently requires a System.Type descriptor.");
                CheckExceptionNull();
                writer.RefCast(storage.DescriptorType).StructGet(storage.DescriptorType, new((uint)DescriptorField.SimpleNameUtf16))
                    .LocalTee(method.GcSource).RefCast(Runtime.Utf16ArrayType).I32Const(0)
                    .LocalGet(method.GcSource).RefCast(Runtime.Utf16ArrayType).ArrayLen()
                    .Call(Runtime.FromCharCodeArrayFunction);
                stack.Push(new(SignatureTypeCode.String));
                break;
            case ManagedTypeOperation.Equal:
            case ManagedTypeOperation.NotEqual:
                PopAssignable(storage.Bind(access.Signature.Parameters[1]), instruction.Code, offset);
                PopAssignable(storage.Bind(access.Signature.Parameters[0]), instruction.Code, offset);
                writer.RefEq();
                if (access.Operation == ManagedTypeOperation.NotEqual) writer.I32Eqz();
                stack.Push(new(SignatureTypeCode.Int32));
                break;
            default: throw new NotSupportedException($"Unsupported runtime type operation: {access.Operation}.");
        }
        return true;
    }
}
