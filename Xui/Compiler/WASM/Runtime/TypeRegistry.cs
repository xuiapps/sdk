using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.Reachability;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Assigns runtime class/interface identities and dispatch locations retained by source reachability.</summary>
internal sealed class TypeRegistry(DependencyGraph dependencies)
{
    private readonly Dictionary<TypeIdentity, Type> types = new();
    public IReadOnlyCollection<Type> Types => types.Values;
    public IReadOnlyList<MethodIdentity> DispatchMethods { get; } = dependencies.Nodes
        .OfType<Dependency.VirtualCall>().Select(node => node.Identity)
        .Where(method => method.DeclaringType.IsInterface || method.Arguments is not null).Distinct().ToArray();

    public Type Resolve(TypeIdentity identity)
    {
        if (types.TryGetValue(identity, out var found)) return found;
        if (identity.Handle.Kind != HandleKind.TypeDefinition)
            throw new NotSupportedException($"Runtime metadata requires a linked type definition: {identity}.");
        var definition = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        if (identity.IsValueType || FrameworkTypes.IsRuntimeBase(identity) || FrameworkTypes.Is(identity, "System", "String"))
        {
            var valueParent = identity.BaseType is { } baseValue && !FrameworkTypes.Is(baseValue, "System", "Object") ? Resolve(baseValue) : null;
            var value = new Type((uint)types.Count + 1, identity, valueParent, (definition.Attributes & TypeAttributes.Abstract) != 0, []);
            types.Add(identity, value);
            return value;
        }
        Type? parent = !DelegateInfo.Is(identity) && identity.BaseType is { Handle.Kind: HandleKind.TypeDefinition } baseIdentity && !FrameworkTypes.Is(baseIdentity, "System", "Object") ? Resolve(baseIdentity) : null;
        var slots = dependencies.VirtualSlots(identity)
            .Select(slot => new VirtualSlot(slot.Index, slot.Declaration, slot.Implementation)).ToArray();
        var type = new Type((uint)types.Count + 1, identity, parent,
            (definition.Attributes & TypeAttributes.Abstract) != 0, Array.AsReadOnly(slots));
        types.Add(identity, type);
        type.Interfaces = Array.AsReadOnly((DelegateInfo.Is(identity) ? [] : identity.Interfaces().Select(Resolve).ToArray()));
        if (!type.IsInterface)
        {
            var specialized = DispatchMethods.Select((method, index) =>
                (Method: method, Index: (uint)index, Implementation: method.DeclaringType.IsInterface
                    ? dependencies.InterfaceImplementation(identity, method) : dependencies.GenericVirtualImplementation(identity, method)))
                .Where(slot => slot.Implementation is not null).ToArray();
            type.InterfaceSlots = Array.AsReadOnly(specialized.Where(slot => slot.Method.DeclaringType.IsInterface)
                .Select(slot => new InterfaceSlot(slot.Index, slot.Method, slot.Implementation!.Value)).ToArray());
            type.GenericVirtualSlots = Array.AsReadOnly(specialized.Where(slot => !slot.Method.DeclaringType.IsInterface)
                .Select(slot => new GenericVirtualSlot(slot.Index, slot.Method, slot.Implementation!.Value)).ToArray());
        }
        return type;
    }

    public uint Slot(MethodIdentity handle)
    {
        if (handle.DeclaringType.IsInterface || handle.Arguments is not null)
        {
            for (int i = 0; i < DispatchMethods.Count; i++) if (DispatchMethods[i] == handle) return (uint)i;
        }
        else foreach (var slot in Resolve(handle.DeclaringType).VirtualSlots)
            if (slot.Implementation == handle || slot.Declaration == handle) return (uint)DispatchMethods.Count + slot.Index;
        throw new NotSupportedException($"No virtual slot for {handle}.");
    }
}
