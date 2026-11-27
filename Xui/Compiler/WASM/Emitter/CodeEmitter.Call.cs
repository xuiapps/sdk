using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet;
using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static MethodIdentity ResolveCall(Xui.Compiler.XuiNet.DotNet.Module source, MethodPlan context, int token, int offset)
    {
        var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(token);
        var target = MemberResolver.Method(source, handle, context.Identity.DeclaringType, context.Identity.Arguments);
        return XuiNet.CodeFlow.BooleanCallSpecialization.Target(context.Body, context.Graph, offset, target);
    }

    private void EmitCall(Instruction.Token instruction, int offset)
    {
        if (TryBitConversion(instruction, offset)) return;
        if (TrySuppressFinalization(instruction, offset)) return;
        if (TryRectangularArray(instruction, offset)) return;
        if (TryStaticSpan(instruction, offset)) return;
        if (method.PrimitiveCalls.TryGetValue(offset, out var primitive))
        {
            EmitPrimitiveCall(primitive, instruction.Code, offset);
            return;
        }
        if (NativeStringComparer.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value)) is { } nativeComparer)
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.IsByReference || receiver.Identity is not { } comparer || !comparer.IsAssignableTo(nativeComparer.Result.Identity!.Value))
                throw new NotSupportedException("Native string comparer adaptation requires a statically typed IEqualityComparer<string>.");
            stack.Push(new(nativeComparer.Result.Code, nativeComparer.Result.Identity));
            return;
        }
        if (TryMonotonicClock(instruction)) return;
        if (TryExecutionStack(instruction)) return;
        // No other agent observes this module's memory; preceding writes remain ordered.
        if (MemoryBarrier.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value))) return;
        if (CurrentThread.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value)))
        {
            writer.I32Const(1);
            stack.Push(new(SignatureTypeCode.Int32));
            return;
        }
        if (TryManagedBuffer(instruction, offset)) return;
        if (TryManagedReference(instruction, offset)) return;
        if (TryResourceString(instruction)) return;
        if (TryStringAccess(instruction, offset)) return;
        if (TryStringSplit(instruction, offset)) return;
        if (TryTextElement(instruction, offset)) return;
        if (TryStringConcatenation(instruction, offset)) return;
        if (TryManagedType(instruction, offset)) return;
        if (TryCompositeFormatting(instruction, offset)) return;
        if (TryInt32Formatting(instruction, offset)) return;
        if (TryConstructorFactory(instruction, offset)) return;
        if (TryGcArrayAllocation(instruction, offset)) return;
        if (TryArrayCopy(instruction, offset)) return;
        if (TryArrayClear(instruction, offset)) return;
        if (TryMath(instruction, offset)) return;
        if (TryStringEquality(instruction, offset)) return;
        if (TrySharedArrayPool(instruction, offset)) return;
        if (TryDefaultEquality(instruction, offset)) return;
        if (TryNullGuard(instruction, offset)) return;
        if (TryInterpolation(instruction, offset)) return;
        if (TryTimeSpanFactory(instruction, offset)) return;
        if (TryReferenceInterlocked(instruction, offset)) return;
        if (TryMonitor(instruction, offset)) return;
        if (TryRuntimeImport(instruction, offset)) return;
        if (TryTaskScheduler(instruction, offset)) return;
        if (TryDelegateOperation(instruction, offset)) return;
        if (TryInlineArrayAccess(instruction, offset)) return;
        if (TrySpanAccess(instruction, offset)) return;
        if (TryStringSpan(instruction, offset)) return;
        if (TryNativeFloat(instruction, offset)) return;
        if (TryExceptionMember(instruction, offset)) return;
        if (TryObjectText(instruction, offset)) return;
        if (TryObjectHash(instruction, offset)) return;
        if (Xui.Compiler.XuiNet.DotNet.WellKnownMethods.IsObjectConstructor(source,
            System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value)))
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.Identity is not { } id || !method.Storage.IsClass(id)) throw new InvalidProgramException("Object constructor requires a class receiver.");
            writer.Drop();
            return;
        }
        var target = method.ConstrainedCalls.TryGetValue(offset, out var concrete) ? concrete.Target : ResolveCall(source, method, instruction.Value, offset);
        var callee = methods[target];
        if (callee.Signature.HasEntryPointArguments)
            throw new NotSupportedException("Calls with string[] entry-point arguments are not implemented.");
        bool delegateInvoke = DelegateInfo.Is(target.DeclaringType);
        var arguments = callee.Signature.Parameters;
        method.CallTemporaries.TryGetValue(target, out var temporaries);
        for (int i = arguments.Length - 1; i >= 0; i--)
        {
            if (i == 0 && method.ConstrainedCalls.TryGetValue(offset, out var receiverCall) && receiverCall.ReferenceReceiver is not null)
                EmitIndirect(OpCode.Ldind_ref, offset, store: false);
            PopAssignable(arguments[i], instruction.Code, offset);
            if (temporaries is not null)
            {
                if (arguments[i].IsByReference) writer.LocalSet(method.CallReferenceTemporaries[target][i]);
                Narrow(arguments[i]);
                writer.LocalSet(temporaries[i]);
            }
        }
        var defaultComparer = instruction.Code == OpCode.Callvirt
            ? XuiNet.DotNet.Intrinsics.DefaultEquality.ForInterface(target) : null;
        if (defaultComparer is not null)
        {
            writer.LocalGet(temporaries![0]).GlobalGet(method.Storage.DefaultComparers[defaultComparer.Owner])
                .RefEq().If(method.Storage.ComparerResultType);
            EmitDefaultInterfaceResult(defaultComparer, temporaries);
            writer.Else();
        }
        int arrayArms = instruction.Code == OpCode.Callvirt ? EmitArrayInterfaceArms(target, temporaries!, offset) : 0;
        if (temporaries is not null)
            for (int i = 0; i < temporaries.Length; i++)
            {
                writer.LocalGet(temporaries[i]);
                if (i == 0 && (delegateInvoke || instruction.Code == OpCode.Callvirt) && arguments[0].UsesGc)
                {
                    CheckExceptionNull();
                    var receiverType = method.Storage.Shape(arguments[0].Identity!.Value).Type;
                    writer.RefCast(receiverType);
                }
            }
        for (int i = 0; i < arguments.Length; i++)
            if (arguments[i].IsByReference) writer.LocalGet(method.CallReferenceTemporaries[target][i]);
        if (callee.Signature.HasResultAddress) FrameAddress(Frame.Values[offset]);
        if (delegateInvoke)
        {
            writer.Call(callee.Function);
        }
        else if (instruction.Code == OpCode.Callvirt && (!method.ConstrainedCalls.TryGetValue(offset, out var constrainedCall) || constrainedCall.ReferenceReceiver is not null) &&
            (target.DeclaringType.Module.GetMethod(target.Handle).Attributes & System.Reflection.MethodAttributes.Virtual) != 0)
        {
            var storage = method.Storage;
            writer.LocalGet(temporaries![0]).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                .RefCast(storage.DescriptorType).StructGet(storage.DescriptorType, new((uint)Xui.Compiler.WASM.Runtime.DescriptorField.TableStart))
                .I32Const((int)storage.Classes.Slot(target)).I32Add().CallIndirect(callee.Type);
        }
        else writer.Call(callee.Function);
        for (int i = 0; i < arrayArms; i++) writer.End();
        if (defaultComparer is not null) writer.End();
        if (callee.Signature.ReturnType != SignatureTypeCode.Void)
        {
            var result = callee.Signature.Result;
            if (result.TypeCode == SignatureTypeCode.Single && !result.IsByReference) writer.F64PromoteF32();
            stack.Push(new MsilStackEntry(result.IsByReference ? result.TypeCode : result.StackType, result.Identity, result.IsByReference, result.Array));
        }
    }
}
