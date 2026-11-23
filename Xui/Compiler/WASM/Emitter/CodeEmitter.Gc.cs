using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool IsGc(MsilStackEntry value) => value.TypeCode == SignatureTypeCode.Object || value.Location is not null ||
        (value.Identity is { } identity && method.Storage.IsGc(identity));

    private GcLocation Location(MsilStackEntry value) => value.Location ?? new(method.Storage.Shape(value.Identity!.Value), 0);

    private void InitializeGcLocals()
    {
        for (int i = 0; i < locals.Length; i++)
            if (locals[i].UsesGc && locals[i].Identity is { } identity && !method.Storage.IsClass(identity))
                writer.StructNewDefault(method.Storage.Shape(locals[i].Identity!.Value).Type)
                    .LocalSet(new LocalIndex((uint)(parameters.Length + i)));
    }

    private void CloneGc(TypeIdentity identity)
    {
        var shape = method.Storage.Shape(identity);
        writer.RefCast(shape.Type).LocalSet(method.GcSource);
        for (int i = 0; i < shape.Leaves.Count; i++)
            writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new((uint)i));
        writer.StructNew(shape.Type);
    }

    private LocalIndex GcScratch(LocalVariable type) => type.WasmType switch
    {
        ValueTypeCode.EqRef => method.GcSource,
        ValueTypeCode.ExternRef => method.GcExtern,
        ValueTypeCode.I64 => method.GcI64,
        ValueTypeCode.F32 or ValueTypeCode.F64 => method.GcF64,
        _ => method.GcI32
    };

    private void StoreGcLocal(LocalIndex local, TypeIdentity identity)
    {
        writer.LocalSet(method.GcSource).LocalGet(local).LocalSet(method.GcOwner);
        var shape = method.Storage.Shape(identity);
        CopyGcLeaves(shape, new(shape, 0), new(shape, 0));
    }

    private void CopyGcLeaves(GcShape value, GcLocation sourceLocation, GcLocation destination)
    {
        for (int i = 0; i < value.Leaves.Count; i++)
        {
            writer.LocalGet(method.GcOwner).RefCast(destination.Owner.Type)
                .LocalGet(method.GcSource).RefCast(sourceLocation.Owner.Type)
                .StructGet(sourceLocation.Owner.Type, new((uint)(sourceLocation.First + i)))
                .StructSet(destination.Owner.Type, new((uint)(destination.First + i)));
        }
    }

    private bool TryGcField(Instruction.Token instruction, int offset)
    {
        bool store = instruction.Code == OpCode.Stfld;
        var receiver = store ? stack.ElementAt(1) : stack.Peek();
        var handle = MetadataTokens.EntityHandle(instruction.Value);
        var resolvedField = MemberResolver.Field(source, handle, method.Identity.DeclaringType);
        var field = resolvedField.DeclaringType.Module.GetField(resolvedField.Handle);
        var declaring = resolvedField.DeclaringType;
        if (receiver.IsNull && method.Storage.IsClass(declaring)) receiver = new(SignatureTypeCode.TypeHandle, declaring);
        if (!IsGc(receiver)) return false;
        var range = method.Storage.Shape(declaring).Fields[resolvedField.Handle];
        if ((receiver.Identity is not { } receiverType || !receiverType.IsAssignableTo(declaring)) || (declaring.IsValueType && instruction.Code != OpCode.Ldfld && !receiver.IsByReference))
            throw new InvalidProgramException($"Invalid GC field receiver at IL_{offset:x4}.");
        if (store) PopAssignable(range.Storage, instruction.Code, offset);
        _ = Pop(instruction.Code, offset);
        var location = Location(receiver);
        location = location with { First = location.First + range.First };
        if (instruction.Code == OpCode.Ldflda)
        {
            writer.RefCast(location.Owner.Type); // Null must trap even for an empty inline field.
            stack.Push(new(range.Storage.TypeCode, range.Storage.Identity, true) { Location = location });
            return true;
        }
        if (store)
        {
            writer.LocalSet(GcScratch(range.Storage)).RefCast(location.Owner.Type).LocalSet(method.GcOwner);
            if (range.Storage.Identity is { } identity && !method.Storage.IsClass(identity))
                StoreInlineValue(identity, range.Storage.UsesGc, location);
            else
            {
                writer.LocalGet(method.GcOwner).RefCast(location.Owner.Type).LocalGet(GcScratch(range.Storage));
                Narrow(range.Storage);
                writer.StructSet(location.Owner.Type, new((uint)location.First));
            }
        }
        else if (range.Storage.Identity is { } identity && !method.Storage.IsClass(identity))
            ReadInlineValue(identity, location, offset);
        else
        {
            writer.RefCast(location.Owner.Type).StructGet(location.Owner.Type, new((uint)location.First));
            if (range.Storage.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        }
        if (!store) stack.Push(new(range.Storage.StackType, range.Storage.Identity));
        return true;
    }

    private void ReadInlineValue(TypeIdentity identity, GcLocation location, int offset)
    {
        var shape = method.Storage.Shape(identity);
        writer.RefCast(location.Owner.Type).LocalSet(method.GcOwner);
        if (method.Storage.IsGc(identity))
        {
            for (int i = 0; i < shape.Leaves.Count; i++)
                writer.LocalGet(method.GcOwner).RefCast(location.Owner.Type).StructGet(location.Owner.Type, new((uint)(location.First + i)));
            writer.StructNew(shape.Type);
        }
        else
        {
            for (int i = 0; i < shape.Leaves.Count; i++)
            {
                var leaf = shape.Leaves[i];
                FrameAddress(Frame.Values[offset] + leaf.MemoryOffset);
                writer.LocalGet(method.GcOwner).RefCast(location.Owner.Type).StructGet(location.Owner.Type, new((uint)(location.First + i)));
                StoreRaw(leaf.Storage);
            }
            FrameAddress(Frame.Values[offset]);
        }
    }

    private void StoreInlineValue(TypeIdentity identity, bool gcSource, GcLocation destination)
    {
        var shape = method.Storage.Shape(identity);
        if (gcSource) CopyGcLeaves(shape, new(shape, 0), destination);
        else for (int i = 0; i < shape.Leaves.Count; i++)
        {
            writer.LocalGet(method.GcOwner).RefCast(destination.Owner.Type)
                .LocalGet(method.GcI32).I32Const(shape.Leaves[i].MemoryOffset).I32Add();
            LoadRaw(shape.Leaves[i].Storage);
            writer.StructSet(destination.Owner.Type, new((uint)(destination.First + i)));
        }
    }

    private void DefaultValue(LocalVariable type)
    {
        switch (type.WasmType)
        {
            case ValueTypeCode.EqRef: writer.RefNull(ValueTypeCode.EqRef); break;
            case ValueTypeCode.ExternRef: writer.RefNull(ValueTypeCode.ExternRef); break;
            case ValueTypeCode.F32: writer.F32Const(0); break;
            case ValueTypeCode.F64: writer.F64Const(0); break;
            case ValueTypeCode.I64: writer.I64Const(0); break;
            default: writer.I32Const(0); break;
        }
    }

    private bool TryGcObject(Instruction.Token instruction, int offset)
    {
        bool copying = instruction.Code is OpCode.Stobj or OpCode.Cpobj;
        var sourceValue = stack.Peek();
        var destination = copying ? stack.ElementAt(1) : sourceValue;
        if (!IsGc(destination) && (!copying || !IsGc(sourceValue))) return false;
        var identity = Xui.Compiler.XuiNet.DotNet.Signatures.SignatureReader.Resolve(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments).Identity
            ?? throw new NotSupportedException("Primitive generic object-copy instructions are not implemented.");
        var shape = method.Storage.Shape(identity);
        if (!identity.IsValueType) throw new NotSupportedException("Object copy instructions require value types.");
        if (copying)
        {
            _ = Pop(instruction.Code, offset);
            _ = Pop(instruction.Code, offset);
            if (sourceValue.Identity != identity || destination.Identity != identity || !destination.IsByReference ||
                (instruction.Code == OpCode.Cpobj && !sourceValue.IsByReference))
                throw new InvalidProgramException("Mismatched managed struct copy.");
            bool sourceGc = IsGc(sourceValue), destinationGc = IsGc(destination);
            writer.LocalSet(sourceGc ? method.GcSource : method.GcI32)
                .LocalSet(destinationGc ? method.GcOwner : Frame.AddressScratch);
            var srcLocation = sourceGc ? Location(sourceValue) : default;
            var dstLocation = destinationGc ? Location(destination) : default;
            for (int i = 0; i < shape.Leaves.Count; i++)
            {
                var leaf = shape.Leaves[i];
                if (destinationGc) writer.LocalGet(method.GcOwner).RefCast(dstLocation.Owner.Type);
                else writer.LocalGet(Frame.AddressScratch).I32Const(leaf.MemoryOffset).I32Add();
                if (sourceGc) writer.LocalGet(method.GcSource).RefCast(srcLocation.Owner.Type).StructGet(srcLocation.Owner.Type, new((uint)(srcLocation.First + i)));
                else { writer.LocalGet(method.GcI32).I32Const(leaf.MemoryOffset).I32Add(); LoadRaw(leaf.Storage); }
                if (destinationGc) writer.StructSet(dstLocation.Owner.Type, new((uint)(dstLocation.First + i)));
                else StoreRaw(leaf.Storage);
            }
            return true;
        }
        _ = Pop(instruction.Code, offset);
        if (!destination.IsByReference || destination.Identity != identity) throw new InvalidProgramException("Expected matching GC struct address.");
        var location = Location(destination);
        if (instruction.Code == OpCode.Ldobj)
        {
            ReadInlineValue(identity, location, offset);
            stack.Push(new(SignatureTypeCode.TypeHandle, identity));
        }
        else
        {
            writer.RefCast(location.Owner.Type).LocalSet(method.GcOwner);
            for (int i = 0; i < shape.Leaves.Count; i++)
            {
                writer.LocalGet(method.GcOwner).RefCast(location.Owner.Type);
                DefaultValue(shape.Leaves[i].Storage);
                writer.StructSet(location.Owner.Type, new((uint)(location.First + i)));
            }
        }
        return true;
    }
}
