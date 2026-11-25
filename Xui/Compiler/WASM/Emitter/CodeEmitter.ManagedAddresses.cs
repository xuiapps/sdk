using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void LinearValueAddress() => writer.LocalSet(method.AddressSelector)
        .RefNull(ValueTypeCode.EqRef).LocalGet(method.AddressSelector);

    private void ValueAddress(LocalVariable value)
    {
        if (value.UsesGc) writer.I32Const(method.Storage.ReferenceField(new(method.Storage.Shape(value.Identity!.Value), 0), value));
        else LinearValueAddress();
    }

    // Only private span/interpolation helpers use raw linear addresses internally.
    private void LinearAddressValue()
    {
        SaveAddress();
        CheckSavedAddress(method.AddressOwner, method.AddressSelector);
        writer.LocalGet(method.AddressOwner).RefIsNull().I32Eqz().If().Unreachable().End();
        writer.LocalGet(method.AddressSelector);
    }

    private void SaveAddress() => writer.LocalSet(method.AddressSelector).LocalSet(method.AddressOwner);

    private void CheckSavedAddress(LocalIndex owner, LocalIndex selector)
    {
        writer.LocalGet(owner).RefIsNull().LocalGet(selector).I32Const(-1).I32Eq().I32And().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End();
    }

    private void ProjectAddress(LocalIndex owner, LocalIndex selector, int bytes, int leaves, LocalVariable value)
    {
        CheckSavedAddress(owner, selector);
        writer.LocalGet(owner).LocalGet(owner).RefIsNull().If(method.Storage.AddressIndexType);
        writer.LocalGet(selector).I32Const(bytes).I32Add();
        if (StorageModel.IsReference(value)) writer.I32Load();
        writer.Else().LocalGet(selector).I32Const(leaves).I32Add().End();
    }

    private bool TryManagedField(Instruction.Token instruction, int offset)
    {
        bool store = instruction.Code == OpCode.Stfld;
        var receiver = store ? stack.ElementAt(1) : stack.Peek();
        if (!receiver.IsByReference) return false;
        var field = MemberResolver.Field(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType, method.Identity.Arguments);
        if (receiver.Identity != field.DeclaringType || !field.DeclaringType.IsValueType)
            throw new InvalidProgramException("Field access requires the declaring struct's managed address.");
        if (StorageModel.IsExplicitValue(field.DeclaringType))
        {
            var declaration = field.DeclaringType.Module.GetField(field.Handle);
            var value = method.Storage.Bind(SignatureReader.Field(field.DeclaringType.Module, declaration.Signature, field.DeclaringType.Arguments));
            if (store) writer.LocalSet(GcScratch(value));
            LinearAddressValue();
            if (store) writer.LocalGet(GcScratch(value));
            return false; // The existing byte-layout path preserves overlapping fields.
        }
        var range = method.Storage.Shape(field.DeclaringType).Fields[field];
        if (store) { PopAssignable(range.Storage, instruction.Code, offset); writer.LocalSet(GcScratch(range.Storage)); }
        _ = Pop(instruction.Code, offset);
        SaveAddress();
        int bytes = method.Storage.StackLayout(field.DeclaringType).Fields[field.Handle].Offset;
        ProjectAddress(method.AddressOwner, method.AddressSelector, bytes, range.First, range.Storage);
        if (instruction.Code == OpCode.Ldflda)
            stack.Push(new(range.Storage.TypeCode, range.Storage.Identity, true, range.Storage.Array));
        else if (store)
        {
            if (range.Storage.Identity is { IsValueType: true } identity)
            { SaveAddress(); StoreAddressStruct(identity, range.Storage.UsesGc); }
            else { writer.LocalGet(GcScratch(range.Storage)); Narrow(range.Storage); writer.Call(method.Storage.ReferenceAccessor(range.Storage, true)); }
        }
        else LoadAddressValue(range.Storage, offset);
        return true;
    }

    private void LoadAddressValue(LocalVariable value, int offset)
    {
        if (value.Identity is { IsValueType: true } identity)
        {
            if (StorageModel.IsExplicitValue(identity))
            {
                LinearAddressValue(); CopyValue(identity, offset);
                stack.Push(new(value.StackType, identity));
                return;
            }
            SaveAddress();
            CheckSavedAddress(method.AddressOwner, method.AddressSelector);
            var shape = method.Storage.Shape(identity);
            foreach (var leaf in shape.Leaves.Select((value, index) => (value, index)))
            {
                if (!value.UsesGc) FrameAddress(Frame.Values[offset] + leaf.value.MemoryOffset);
                ProjectAddress(method.AddressOwner, method.AddressSelector, leaf.value.MemoryOffset, leaf.index, leaf.value.Storage);
                writer.Call(method.Storage.ReferenceAccessor(leaf.value.Storage, false));
                if (!value.UsesGc) StoreRaw(leaf.value.Storage);
            }
            if (value.UsesGc) writer.StructNew(shape.Type);
            else FrameAddress(Frame.Values[offset]);
        }
        else
        {
            writer.Call(method.Storage.ReferenceAccessor(value, false));
            if (value.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        }
        stack.Push(new(value.StackType, value.Identity, array: value.Array));
    }

    // The destination owner/selector and source value are already in scratch locals.
    private void StoreAddressStruct(TypeIdentity identity, bool gcSource)
    {
        if (StorageModel.IsExplicitValue(identity))
        {
            writer.LocalGet(method.AddressOwner).LocalGet(method.AddressSelector); LinearAddressValue();
            writer.LocalGet(method.GcI32); CopyStruct(identity); return;
        }
        var shape = method.Storage.Shape(identity);
        for (int i = 0; i < shape.Leaves.Count; i++)
        {
            var leaf = shape.Leaves[i];
            ProjectAddress(method.AddressOwner, method.AddressSelector, leaf.MemoryOffset, i, leaf.Storage);
            if (gcSource) writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new((uint)i));
            else { writer.LocalGet(method.GcI32).I32Const(leaf.MemoryOffset).I32Add(); LoadRaw(leaf.Storage); }
            writer.Call(method.Storage.ReferenceAccessor(leaf.Storage, true));
        }
    }

    private void EmitManagedStructObject(Instruction.Token instruction, int offset)
    {
        var type = method.Storage.Bind(SignatureReader.StorageType(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments));
        var identity = type.Identity ?? throw new InvalidProgramException("Expected a struct type.");
        var expected = type with { IsByReference = true };
        if (instruction.Code == OpCode.Ldobj)
        {
            PopAssignable(expected, instruction.Code, offset);
            LoadAddressValue(type, offset);
            return;
        }
        if (instruction.Code == OpCode.Cpobj)
        {
            PopAssignable(expected, instruction.Code, offset);
            writer.LocalSet(method.SourceSelector).LocalSet(method.SourceOwner);
        }
        else if (instruction.Code == OpCode.Stobj)
        {
            PopAssignable(type, instruction.Code, offset);
            writer.LocalSet(GcScratch(type));
        }
        PopAssignable(expected, instruction.Code, offset); SaveAddress();
        CheckSavedAddress(method.AddressOwner, method.AddressSelector);
        if (instruction.Code == OpCode.Stobj) { StoreAddressStruct(identity, type.UsesGc); return; }
        if (StorageModel.IsExplicitValue(identity))
        {
            writer.LocalGet(method.AddressOwner).LocalGet(method.AddressSelector); LinearAddressValue();
            if (instruction.Code == OpCode.Cpobj)
            {
                writer.LocalGet(method.SourceOwner).LocalGet(method.SourceSelector); LinearAddressValue(); CopyStruct(identity);
            }
            else ClearStruct(identity);
            return;
        }
        var shape = method.Storage.Shape(identity);
        if (instruction.Code == OpCode.Cpobj) CheckSavedAddress(method.SourceOwner, method.SourceSelector);
        for (int i = 0; i < shape.Leaves.Count; i++)
        {
            var leaf = shape.Leaves[i];
            ProjectAddress(method.AddressOwner, method.AddressSelector, leaf.MemoryOffset, i, leaf.Storage);
            if (instruction.Code == OpCode.Cpobj)
            {
                ProjectAddress(method.SourceOwner, method.SourceSelector, leaf.MemoryOffset, i, leaf.Storage);
                writer.Call(method.Storage.ReferenceAccessor(leaf.Storage, false));
            }
            else DefaultValue(leaf.Storage);
            writer.Call(method.Storage.ReferenceAccessor(leaf.Storage, true));
        }
    }
}
