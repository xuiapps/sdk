using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static void RequireDefaultHash(MethodPlan method, DefaultEquality access)
    {
        RequirePrimitiveOperation(method, access.Element, access.Operation);
        if (access.Implementation is { } implementation && !method.Calls.Contains(implementation)) method.Calls.Add(implementation);
        if (access.UsesPrimitiveStructHash)
        {
            foreach (var leaf in method.Storage.Shape(access.Element.Identity!.Value).Leaves)
                method.EqualityTemporaries.TryAdd(leaf.Storage, default);
            return;
        }
        if (access.UsesArrayIdentity) { method.Storage.Primitives.UsesObjectHash = true; return; }
        if (!access.UsesObjectHash) return;
        method.Storage.Primitives.UsesObjectHash = true;
        foreach (var target in method.Storage.HashTargets(access.Element.Identity!.Value))
            if (!method.Calls.Contains(target.Method)) method.Calls.Add(target.Method);
    }

    private void EmitDefaultHash(DefaultEquality access, LocalIndex value)
    {
        if (access.UsesPrimitiveStructHash)
        {
            // Hash declared scalar fields, never padding bytes. Primitive hashing
            // normalizes NaNs and signed zeros consistently with value equality.
            writer.I32Const(17);
            foreach (var leaf in method.Storage.Shape(access.Element.Identity!.Value).Leaves)
            {
                writer.I32Const(31).I32Mul();
                writer.LocalGet(value).I32Const(leaf.MemoryOffset).I32Add();
                LoadRaw(leaf.Storage);
                if (leaf.Storage.TypeCode == System.Reflection.Metadata.SignatureTypeCode.Single) writer.F64PromoteF32();
                var temporary = method.EqualityTemporaries[leaf.Storage].Left;
                writer.LocalSet(temporary);
                EmitPrimitiveHash(leaf.Storage, temporary);
                writer.I32Add();
            }
            return;
        }
        if (access.UsesArrayIdentity)
        {
            writer.LocalGet(value).RefIsNull().If().I32Const(0).LocalSet(method.GcI32).Else();
            writer.LocalGet(value).ExternConvertAny().Call(method.Storage.Primitives.ObjectHash).LocalSet(method.GcI32);
            writer.End().LocalGet(method.GcI32);
            return;
        }
        if (access.Implementation is { } valueHash)
        {
            var element = method.Storage.Bind(access.Element);
            if (element.UsesGc)
                writer.LocalGet(value).I32Const(method.Storage.ReferenceField(new(method.Storage.Shape(valueHash.DeclaringType), 0), element));
            else writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).LocalGet(value);
            writer.Call(methods[valueHash].Function);
            return;
        }
        if (!access.UsesObjectHash) { EmitPrimitiveHash(method.Storage.Bind(access.Element), value); return; }
        var storage = method.Storage;
        var targets = storage.HashTargets(access.Element.Identity!.Value);
        writer.LocalGet(value).RefIsNull().If().I32Const(0).LocalSet(method.GcI32).Else();
        foreach (var (receiver, implementation) in targets)
        {
            writer.LocalGet(value).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                .GlobalGet(storage.Classes.Resolve(receiver).Descriptor).RefEq().If();
            writer.LocalGet(value).Call(methods[implementation].Function).LocalSet(method.GcI32).Else();
        }
        writer.LocalGet(value).ExternConvertAny().Call(storage.Primitives.ObjectHash).LocalSet(method.GcI32);
        foreach (var _ in targets) writer.End();
        writer.End().LocalGet(method.GcI32);
    }
}
