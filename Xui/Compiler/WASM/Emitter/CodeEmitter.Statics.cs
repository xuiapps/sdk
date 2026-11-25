using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EnsureInitialized(TypeIdentity type)
    {
        if (method.Storage.ModuleInitializers.TryGetValue(type.Module, out var module) && module.Identity != type)
            writer.Call(module.Ensure);
        if (method.Storage.Statics.TryGetValue(type, out var storage) && storage.Constructor is not null)
            writer.Call(storage.Ensure);
    }

    private void InitializeMethodType()
    {
        if (TypeInitialization.AtMethodEntry(method.Identity))
        {
            EnsureInitialized(method.Identity.DeclaringType);
            return;
        }
        if (method.Storage.ModuleInitializers.TryGetValue(source, out var module) && module.Constructor != method.Identity)
            writer.Call(module.Ensure);
    }

    private void EmitStaticField(Instruction.Token instruction, int offset)
    {
        var field = MemberResolver.Field(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType, method.Identity.Arguments);
        var owner = method.Storage.Statics[field.DeclaringType];
        var range = owner.Fields.Fields[field];
        var location = new GcLocation(owner.Fields, range.First);
        var value = range.Storage;
        EnsureInitialized(field.DeclaringType);
        if (instruction.Code == OpCode.Stsfld)
        {
            PopAssignable(value, instruction.Code, offset);
            writer.LocalSet(GcScratch(value));
            writer.GlobalGet(owner.Holder).LocalSet(method.GcOwner);
            if (value.Identity is { IsValueType: true } identity) StoreInlineValue(identity, value.UsesGc, location);
            else
            {
                writer.LocalGet(method.GcOwner).RefCast(owner.Fields.Type).LocalGet(GcScratch(value));
                Narrow(value); writer.StructSet(owner.Fields.Type, new((uint)range.First));
            }
            return;
        }
        writer.GlobalGet(owner.Holder);
        if (instruction.Code == OpCode.Ldsflda)
        {
            writer.I32Const(method.Storage.ReferenceField(location, value));
            stack.Push(new(value.TypeCode, value.Identity, true, value.Array));
            return;
        }
        if (value.Identity is { IsValueType: true } structType) ReadInlineValue(structType, location, offset);
        else
        {
            writer.RefCast(owner.Fields.Type).StructGet(owner.Fields.Type, new((uint)range.First));
            if (value.TypeCode == System.Reflection.Metadata.SignatureTypeCode.Single) writer.F64PromoteF32();
        }
        stack.Push(new(value.StackType, value.Identity, array: value.Array));
    }
}
