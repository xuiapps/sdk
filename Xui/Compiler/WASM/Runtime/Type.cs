using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A compiled class's nominal identity, inheritance, dispatch slots, and WASM locations.</summary>
/// <remarks>This compiler-side descriptor is the source for runtime metadata. It is not System.Type.
/// IDs and indices are local to one emitted module. Keep the source assembly alive when reading Identity.</remarks>
public sealed class Type
{
    internal Type(uint id, TypeIdentity identity, Type? baseType, bool isAbstract, IReadOnlyList<VirtualSlot> slots)
    {
        Id = id; Identity = identity; Name = identity.ToString(); BaseType = baseType;
        IsAbstract = isAbstract; VirtualSlots = slots;
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
    /// <summary>The same-module base class; null currently denotes the System.Object boundary.</summary>
    public Type? BaseType { get; }
    /// <summary>Whether allocation of this declaration is forbidden.</summary>
    public bool IsAbstract { get; }
    /// <summary>Inherited slots with overrides substituted; newslot declarations append entries.</summary>
    public IReadOnlyList<VirtualSlot> VirtualSlots { get; }
    /// <summary>Inherited and declared instance fields mapped to flattened GC field ranges.</summary>
    public IReadOnlyList<Field> Fields { get; internal set; } = [];
    /// <summary>The GC field count, including the type-descriptor header at field zero.</summary>
    public uint FieldCount { get; internal set; }
    /// <summary>The concrete GC struct type, including inherited fields and the descriptor header.</summary>
    public TypeIndex WasmType { get; internal set; }
    /// <summary>The immutable global containing this class's runtime descriptor.</summary>
    public GlobalIndex Descriptor { get; internal set; }
    /// <summary>The first function-table entry for this class's virtual slots.</summary>
    public uint TableStart { get; internal set; }
}
