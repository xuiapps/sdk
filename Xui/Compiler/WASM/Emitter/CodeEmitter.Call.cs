using Xui.Compiler.XuiNet.DotNet;
using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static MethodIdentity ResolveCall(Xui.Compiler.XuiNet.DotNet.Module source, TypeIdentity context, int token, int offset)
    {
        var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(token);
        return MemberResolver.Method(source, handle, context);
    }

    private void EmitCall(Instruction.Token instruction, int offset)
    {
        if (Xui.Compiler.XuiNet.DotNet.WellKnownMethods.IsObjectConstructor(source,
            System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.Value)))
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.Identity is not { } id || !method.Storage.IsClass(id)) throw new InvalidProgramException("Object constructor requires a class receiver.");
            writer.Drop();
            return;
        }
        var target = ResolveCall(source, method.Identity.DeclaringType, instruction.Value, offset);
        var callee = methods[target];
        if (callee.Signature.HasEntryPointArguments)
            throw new NotSupportedException("Calls with string[] entry-point arguments are not implemented.");
        var arguments = callee.Signature.Parameters;
        method.CallTemporaries.TryGetValue(target, out var temporaries);
        for (int i = arguments.Length - 1; i >= 0; i--)
        {
            if (arguments[i].IsByReference && stack.Peek().Location is { } location &&
                (location.First != 0 || location.Owner.Identity != arguments[i].Identity))
                throw new NotSupportedException("Instance calls through inline struct addresses are not implemented.");
            PopAssignable(arguments[i], instruction.Code, offset);
            if (temporaries is not null)
            {
                Narrow(arguments[i]);
                writer.LocalSet(temporaries[i]);
            }
        }
        if (temporaries is not null)
            for (int i = 0; i < temporaries.Length; i++)
            {
                writer.LocalGet(temporaries[i]);
                if (i == 0 && instruction.Code == OpCode.Callvirt)
                    writer.RefCast(method.Storage.Shape(arguments[0].Identity!.Value).Type);
            }
        if (instruction.Code == OpCode.Callvirt &&
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
            stack.Push(new MsilStackEntry(result.StackType, result.Identity));
        }
    }
}
