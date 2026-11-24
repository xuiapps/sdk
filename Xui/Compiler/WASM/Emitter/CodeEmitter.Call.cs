using Xui.Compiler.XuiNet.DotNet;
using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static MethodIdentity ResolveCall(Xui.Compiler.XuiNet.DotNet.Module source, MethodIdentity context, int token, int offset)
    {
        var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(token);
        return MemberResolver.Method(source, handle, context.DeclaringType, context.Arguments);
    }

    private void EmitCall(Instruction.Token instruction, int offset)
    {
        if (TryStringConcatenation(instruction, offset)) return;
        if (TryManagedType(instruction, offset)) return;
        if (TryInt32Formatting(instruction, offset)) return;
        if (TryConstructorFactory(instruction, offset)) return;
        if (TryArrayCopy(instruction, offset)) return;
        if (TryMath(instruction, offset)) return;
        if (TryStringEquality(instruction, offset)) return;
        if (TryDefaultEquality(instruction, offset)) return;
        if (TryNullGuard(instruction, offset)) return;
        if (TryInterpolation(instruction, offset)) return;
        if (TryTimeSpanFactory(instruction, offset)) return;
        if (TryReferenceCompareExchange(instruction, offset)) return;
        if (TryDelegateOperation(instruction, offset)) return;
        if (TryInlineArrayAccess(instruction, offset)) return;
        if (TrySpanAccess(instruction, offset)) return;
        if (TryNativeFloat(instruction, offset)) return;
        if (TryExceptionMember(instruction, offset)) return;
        if (Xui.Compiler.XuiNet.DotNet.WellKnownMethods.IsObjectConstructor(source,
            System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value)))
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.Identity is not { } id || !method.Storage.IsClass(id)) throw new InvalidProgramException("Object constructor requires a class receiver.");
            writer.Drop();
            return;
        }
        var target = ResolveCall(source, method.Identity, instruction.Value, offset);
        var callee = methods[target];
        if (callee.Signature.HasEntryPointArguments)
            throw new NotSupportedException("Calls with string[] entry-point arguments are not implemented.");
        bool delegateInvoke = DelegateInfo.Is(target.DeclaringType);
        var arguments = callee.Signature.Parameters;
        method.CallTemporaries.TryGetValue(target, out var temporaries);
        for (int i = arguments.Length - 1; i >= 0; i--)
        {
            if (arguments[i].IsByReference && stack.Peek().Location is { } location &&
                (!arguments[i].UsesGc || location.First != 0 || location.Owner.Identity != arguments[i].Identity))
                throw new NotSupportedException("Calls through inline value field addresses are not implemented.");
            PopAssignable(arguments[i], instruction.Code, offset);
            if (temporaries is not null)
            {
                if (arguments[i].IsReferenceAddress) writer.LocalSet(method.CallReferenceTemporaries[target][i]);
                Narrow(arguments[i]);
                writer.LocalSet(temporaries[i]);
            }
        }
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
            if (arguments[i].IsReferenceAddress) writer.LocalGet(method.CallReferenceTemporaries[target][i]);
        if (callee.Signature.HasResultAddress) FrameAddress(Frame.Values[offset]);
        if (delegateInvoke)
        {
            writer.Call(callee.Function);
        }
        else if (instruction.Code == OpCode.Callvirt &&
            (target.DeclaringType.Module.GetMethod(target.Handle).Attributes & System.Reflection.MethodAttributes.Virtual) != 0)
        {
            var storage = method.Storage;
            writer.LocalGet(temporaries![0]).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                .RefCast(storage.DescriptorType).StructGet(storage.DescriptorType, new((uint)Xui.Compiler.WASM.Runtime.DescriptorField.TableStart))
                .I32Const((int)storage.Classes.Slot(target)).I32Add().CallIndirect(callee.Type);
        }
        else writer.Call(callee.Function);
        if (callee.Signature.ReturnType != SignatureTypeCode.Void)
        {
            var result = callee.Signature.Result;
            if (result.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
            stack.Push(new MsilStackEntry(result.StackType, result.Identity, array: result.Array));
        }
    }
}
