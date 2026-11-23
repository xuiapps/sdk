using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A compiled class or interface's nominal identity, inheritance, dispatch slots, and WASM locations.</summary>
/// <remarks>This compiler-side descriptor is the source for runtime metadata. It is not System.Type.
/// IDs and indices are local to one emitted module. Keep the source assembly alive when reading Identity.</remarks>
public sealed class Type
{
    internal Type(uint id, TypeIdentity identity, Type? baseType, bool isAbstract, IReadOnlyList<VirtualSlot> slots)
    {
        Id = id; Identity = identity; Name = identity.ToString(); BaseType = baseType;
        IsAbstract = isAbstract; VirtualSlots = slots; IsInterface = identity.IsInterface;
    }

    /// <summary>The nominal identifier, independent of WASM's structural type equivalence.</summary>
    public uint Id { get; }
    /// <summary>The owning module and source declaration.</summary>
    public TypeIdentity Identity { get; }
    /// <summary>The source generic declaration, without its constructed arguments.</summary>
    public TypeIdentity GenericDefinition => Identity.GenericDefinition;
    /// <summary>The concrete arguments used for this class specialization; empty for nongeneric types.</summary>
    public IReadOnlyList<SignatureType> GenericArguments => Identity.Arguments is { } arguments ? arguments.Values : Array.Empty<SignatureType>();
    /// <summary>The namespace-qualified source name.</summary>
    public string Name { get; }
    /// <summary>The base class; null denotes an interface or the System.Object boundary.</summary>
    public Type? BaseType { get; }
    /// <summary>Whether allocation of this declaration is forbidden.</summary>
    public bool IsAbstract { get; }
    /// <summary>Whether this descriptor denotes an interface rather than an allocatable class.</summary>
    public bool IsInterface { get; }
    /// <summary>All implemented or inherited interfaces, including constructed generic identities.</summary>
    public IReadOnlyList<Type> Interfaces { get; internal set; } = [];
    /// <summary>Reachable interface declarations and their selected concrete implementations.</summary>
    public IReadOnlyList<InterfaceSlot> InterfaceSlots { get; internal set; } = [];
    /// <summary>Inherited slots with overrides substituted; newslot declarations append entries.</summary>
    public IReadOnlyList<VirtualSlot> VirtualSlots { get; }
    /// <summary>Inherited and declared instance fields mapped to flattened GC field ranges.</summary>
    public IReadOnlyList<Field> Fields { get; internal set; } = [];
    /// <summary>The GC field count, including the type-descriptor header at field zero.</summary>
    public uint FieldCount { get; internal set; }
    /// <summary>The concrete GC struct type, or the shared class-reference root for an interface.</summary>
    public TypeIndex WasmType { get; internal set; }
    /// <summary>The immutable global containing this class's runtime descriptor.</summary>
    public GlobalIndex Descriptor { get; internal set; }
    /// <summary>The first function-table entry for this class; interface slots precede class virtual slots.</summary>
    public uint TableStart { get; internal set; }
}
