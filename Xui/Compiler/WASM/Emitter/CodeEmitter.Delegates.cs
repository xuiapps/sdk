using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryDelegateConstructor(Instruction.Token instruction, int offset)
    {
        if (DelegateInfo.Member(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity) is not { } constructor) return false;
        if (constructor.DeclaringType.Module.GetMethod(constructor.Handle).Name != ".ctor")
            throw new InvalidProgramException("Delegate newobj requires its constructor.");
        var pointer = Pop(instruction.Code, offset);
        if (pointer.Method is not { } target) throw new InvalidProgramException("Delegate construction requires ldftn or ldvirtftn.");
        var callee = methods[target];
        var invoke = methods[DelegateInfo.Invoke(constructor.DeclaringType)];
        ValidateDelegateBinding(invoke.Signature, callee.Signature);
        writer.LocalSet(method.GcI32);
        if (callee.Signature.HasThis)
        {
            PopAssignable(callee.Signature.Parameters[0], instruction.Code, offset);
            writer.LocalTee(method.GcSource).RefIsNull().If();
            ThrowException(XuiNet.DotNet.Exceptions.ExceptionKind.ArgumentException);
            writer.End();
        }
        else
        {
            if (!Pop(instruction.Code, offset).IsNull)
                throw new NotSupportedException("Closed static delegates with a bound first argument are not implemented.");
            writer.LocalSet(method.GcSource);
        }
        var shape = method.Storage.Shape(constructor.DeclaringType);
        writer.GlobalGet(method.Storage.Classes.Resolve(constructor.DeclaringType).Descriptor)
            .LocalGet(method.GcSource).LocalGet(method.GcI32)
            .GlobalGet(method.Storage.DelegateMethodIds).RefCast(method.Storage.DelegateMethodsType).LocalGet(method.GcI32).ArrayGet(method.Storage.DelegateMethodsType)
            .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).StructNew(shape.Type);
        stack.Push(new(SignatureTypeCode.TypeHandle, constructor.DeclaringType));
        return true;
    }

    private static void ValidateDelegateBinding(MethodSignature invoke, MethodSignature target)
    {
        int receiver = target.HasThis ? 1 : 0;
        if (invoke.Parameters.Length - 1 != target.Parameters.Length - receiver ||
            !Assignable(target.Result, invoke.Result) || invoke.HasResultAddress != target.HasResultAddress)
            throw new NotSupportedException("Delegate binding requires compatible parameter/result representations.");
        for (int i = 1; i < invoke.Parameters.Length; i++)
            if (!Assignable(invoke.Parameters[i], target.Parameters[i - 1 + receiver]))
                throw new NotSupportedException("Delegate binding requires compatible parameter/result representations.");

        static bool Assignable(LocalVariable value, LocalVariable destination)
        {
            if (value == destination) return true;
            if (value.TypeCode == SignatureTypeCode.Void || destination.TypeCode == SignatureTypeCode.Void) return false;
            if (value.IsByReference || destination.IsByReference || value.WasmType != destination.WasmType) return false;
            if (value.Identity is { IsValueType: false } source)
                return destination.TypeCode == SignatureTypeCode.Object || destination.Identity is { } target && source.IsAssignableTo(target);
            return false;
        }
    }
}
