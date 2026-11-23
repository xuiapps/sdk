using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.Reachability;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Assigns runtime class identities to layouts already retained by source reachability.</summary>
internal sealed class TypeRegistry(Model.Module module, DependencyGraph dependencies)
{
    private readonly Dictionary<TypeIdentity, Type> types = new();
    public IReadOnlyCollection<Type> Types => types.Values;

    public Type Resolve(TypeIdentity identity)
    {
        if (types.TryGetValue(identity, out var found)) return found;
        if (identity.Module != module || identity.Handle.Kind != HandleKind.TypeDefinition || identity.IsValueType)
            throw new NotSupportedException($"Runtime class metadata requires a same-module class: {identity}.");
        var definition = module.GetType((TypeDefinitionHandle)identity.Handle);
        Type? parent = identity.BaseType is { Handle.Kind: HandleKind.TypeDefinition } baseIdentity ? Resolve(baseIdentity) : null;
        var slots = dependencies.VirtualSlots(identity)
            .Select(slot => new VirtualSlot(slot.Index, slot.Declaration, slot.Implementation)).ToArray();
        var type = new Type((uint)types.Count + 1, identity, parent,
            (definition.Attributes & TypeAttributes.Abstract) != 0, Array.AsReadOnly(slots));
        types.Add(identity, type);
        return type;
    }

    public uint Slot(MethodIdentity handle)
    {
        foreach (var slot in Resolve(handle.DeclaringType).VirtualSlots)
            if (slot.Implementation == handle || slot.Declaration == handle) return slot.Index;
        throw new NotSupportedException($"No virtual slot for {handle}.");
    }
}
