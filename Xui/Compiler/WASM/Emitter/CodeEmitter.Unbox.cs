using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitUnbox(Instruction.Token instruction, int offset) => EmitUnboxing(instruction, offset, address: true);

    private void EmitUnboxing(Instruction.Token instruction, int offset, bool address)
    {
        var box = BoxingType.Read(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (box is not { } value)
        {
            if (address) throw new NotSupportedException("unbox requires a value type.");
            var reference = XuiNet.DotNet.Signatures.SignatureReader.StorageType(source, MetadataTokens.EntityHandle(instruction.Value),
                method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
            if (reference.Code == SignatureTypeCode.Object)
            {
                if (!IsObjectReference(Pop(instruction.Code, offset))) throw new InvalidProgramException("unbox.any object requires an object reference.");
                stack.Push(new(SignatureTypeCode.Object));
                return;
            }
            EmitTypeTest(instruction, offset, throwing: true);
            return;
        }
        if (!IsObjectReference(Pop(instruction.Code, offset))) throw new InvalidProgramException("Unboxing requires an object reference.");
        CheckExceptionNull();
        EmitBoxTest(value, compatible: true);
        writer.LocalTee(method.GcOwner).RefIsNull().If();
        ThrowException(ExceptionKind.InvalidCastException);
        writer.End().LocalGet(method.GcOwner);
        var storage = method.Storage.Bind(value.Storage);
        var shape = method.Storage.Box(value);
        if (address)
        {
            writer.I32Const(method.Storage.ReferenceField(new(shape, 1), storage));
            stack.Push(new(storage.TypeCode, isByReference: true));
        }
        else
        {
            writer.RefCast(shape.Type).StructGet(shape.Type, new(1));
            if (storage.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
            stack.Push(new(storage.StackType));
        }
    }

    // A structural WASM type test alone cannot distinguish Int32 from UInt32 or an enum.
    private void EmitBoxTest(BoxingType target, bool compatible)
    {
        var storage = method.Storage;
        var shape = storage.Box(target);
        writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefTest(shape.Type)
            .If(storage.ExceptionPayloadBlock);
        writer.LocalGet(method.GcOwner).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0)).LocalSet(method.GcSource);
        writer.I32Const(0);
        foreach (var candidate in storage.Boxes.Keys)
            if (compatible ? target.CanUnbox(candidate) : target.Identity == candidate.Identity)
                writer.LocalGet(method.GcSource).GlobalGet(storage.Classes.Resolve(candidate.Identity).Descriptor).RefEq().I32Or();
        writer.If(storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else()
            .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End().Else()
            .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End();
    }
}
