using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Layout;

/// <summary>Calculates and caches target-specific layouts of unmanaged structs and enums from PE metadata.</summary>
/// <remarks>Keep the source assembly alive. Closed generic fields are specialized before layout. Reference fields, open generics,
/// unresolved external values are rejected. Auto layouts require an explicit storage policy. Framework
/// ValueTuple auto layouts use declaration-order target storage, not a host CLR interop layout.</remarks>
public sealed partial class LayoutResolver
{
    private readonly Model.Module module;
    private readonly Dictionary<Model.Module, LayoutResolver> modules;
    /// <summary>Creates a layout resolver sharing target rules across linked implementation assemblies.</summary>
    public LayoutResolver(Model.Module module, TargetLayout target, AutoLayoutPolicy autoLayout = AutoLayoutPolicy.Reject) : this(module, target, autoLayout, new()) { }
    private LayoutResolver(Model.Module module, TargetLayout target, AutoLayoutPolicy autoLayout, Dictionary<Model.Module, LayoutResolver> modules)
    { this.module = module; Target = target; AutoLayout = autoLayout; this.modules = modules; modules.Add(module, this); }
    private LayoutResolver For(Model.Module owner) => modules.TryGetValue(owner, out var resolver)
        ? resolver : new(owner, Target, AutoLayout, modules);

    private readonly Dictionary<TypeIdentity, StructLayout> cache = new();
    private readonly HashSet<TypeIdentity> resolving = [];

    /// <summary>The explicit target used for every layout in this resolver.</summary>
    public TargetLayout Target { get; }
    /// <summary>Whether automatic layouts may use compiler-owned declaration-order bytes.</summary>
    public AutoLayoutPolicy AutoLayout { get; }

    /// <summary>Resolves the instance layout of a struct or enum defined in the source module.</summary>
    public StructLayout Resolve(TypeDefinitionHandle handle) => Resolve(new TypeIdentity(module, handle));

    /// <summary>Resolves a closed value-type specialization using its defining module and arguments.</summary>
    public StructLayout Resolve(TypeIdentity identity)
    {
        if (identity.Module != module) return For(identity.Module).Resolve(identity);
        if (cache.TryGetValue(identity, out var found)) return found;
        if (!resolving.Add(identity)) throw new BadImageFormatException("Recursive value-type field layout.");
        try
        {
            var layout = Calculate(identity);
            cache.Add(identity, layout);
            return layout;
        }
        finally { resolving.Remove(identity); }
    }

    private StructLayout Calculate(TypeIdentity identity)
    {
        var handle = (TypeDefinitionHandle)identity.Handle;
        var type = module.GetType(handle);
        if (DotNet.Intrinsics.NativeFloat.Is(identity))
        {
            var value = type.Fields.Single(h => (module.GetField(h).Attributes & FieldAttributes.Static) == 0);
            return new(Target.NativeFloatSize, Target.NativeFloatSize, new() { [value] = new(0, Target.NativeFloatSize) });
        }
        bool isEnum = IsCoreType(type.BaseType, "System", "Enum");
        if (!isEnum && !IsCoreType(type.BaseType, "System", "ValueType"))
            throw new NotSupportedException($"Only unboxed value-type layout is supported: {type.Name}.");
        var definition = module.Metadata.GetTypeDefinition(handle);
        if (definition.GetGenericParameters().Count != (identity.Arguments?.Values.Length ?? 0) || identity.Arguments?.ContainsGenericParameters == true)
            throw new NotSupportedException($"Generic layout requires a closed specialization: {type.Name}.");
        var declared = type.DeclaredLayout;
        int pack = declared.PackingSize == 0 ? Target.DefaultPackingSize : declared.PackingSize;
        if (pack is not (1 or 2 or 4 or 8 or 16 or 32 or 64 or 128))
            throw new BadImageFormatException($"Invalid packing size {pack} for {type.Name}.");
        if (InlineArrayInfo.Read(identity) is { } inline)
        {
            var element = StorageOf(inline.Element);
            return new(checked(element.Size * inline.Length), Math.Min(pack, element.Alignment),
                new() { [inline.Field.Handle] = new(0, element.Size) });
        }
        var kind = type.Attributes & TypeAttributes.LayoutMask;
        if (!isEnum && kind is not (TypeAttributes.SequentialLayout or TypeAttributes.ExplicitLayout) && !FrameworkTypes.IsValueTuple(identity) && AutoLayout != AutoLayoutPolicy.DeclarationOrder)
            throw new NotSupportedException($"Auto layout is not supported: {type.Name}.");
        var fields = new Dictionary<FieldDefinitionHandle, FieldLayout>();
        int end = 0, alignment = 1;
        foreach (var fieldHandle in type.Fields)
        {
            var field = module.GetField(fieldHandle);
            if ((field.Attributes & FieldAttributes.Static) != 0) continue;
            var signature = field.Signature;
            if (signature.ReadSignatureHeader().Kind != SignatureKind.Field)
                throw new BadImageFormatException($"Invalid field signature: {type.Name}.{field.Name}.");
            Storage storage;
            try { storage = ReadStorage(ref signature, identity.Arguments); }
            catch (NotSupportedException error)
            {
                throw new NotSupportedException($"Cannot lay out {type.Name}.{field.Name}: {error.Message}", error);
            }
            if (signature.RemainingBytes != 0) throw new BadImageFormatException("Unexpected field signature bytes.");
            int fieldAlignment = Math.Min(pack, storage.Alignment);
            alignment = Math.Max(alignment, fieldAlignment);
            int offset = kind == TypeAttributes.ExplicitLayout ? field.DeclaredOffset : Align(end, fieldAlignment);
            if (offset < 0) throw new BadImageFormatException($"Missing explicit offset for {type.Name}.{field.Name}.");
            fields.Add(fieldHandle, new(offset, storage.Size));
            end = Math.Max(end, checked(offset + storage.Size));
        }
        // A nonzero metadata size is a minimum extent; it suppresses implicit tail padding.
        int size = declared.Size == 0 ? Align(end, alignment) : Math.Max(end, declared.Size);
        return new(Math.Max(1, size), alignment, fields);
    }

    private Storage ReadStorage(ref BlobReader reader, GenericArguments? arguments = null)
    {
        var raw = reader;
        var code = reader.ReadSignatureTypeCode();
        switch (code)
        {
            case SignatureTypeCode.Boolean:
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.SByte: return new(1, 1);
            case SignatureTypeCode.Char:
            case SignatureTypeCode.Int16:
            case SignatureTypeCode.UInt16: return new(2, 2);
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32:
            case SignatureTypeCode.Single: return new(4, 4);
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64:
            case SignatureTypeCode.Double: return new(8, 8);
            case SignatureTypeCode.IntPtr:
            case SignatureTypeCode.UIntPtr: return new(Target.PointerSize, Target.PointerSize);
            case SignatureTypeCode.OptionalModifier:
            case SignatureTypeCode.RequiredModifier:
                _ = reader.ReadTypeHandle();
                return ReadStorage(ref reader, arguments);
            case SignatureTypeCode.GenericTypeParameter:
            case SignatureTypeCode.GenericTypeInstance:
            case SignatureTypeCode.TypeHandle:
                reader = raw;
                return StorageOf(SignatureReader.Type(module, ref reader, arguments));
            default:
                throw new NotSupportedException($"Field storage type {code} is not supported.");
        }
    }

    private Storage StorageOf(SignatureType type)
    {
        if (type.IsReferenceType || type.IsByReference) throw new NotSupportedException("Reference-field storage is not supported.");
        if (type.Identity is { } identity)
        {
            if (DotNet.Intrinsics.NativeFloat.Is(identity)) return new(Target.NativeFloatSize, Target.NativeFloatSize);
            if (identity.Handle.Kind != HandleKind.TypeDefinition) throw new NotSupportedException("External value-type layout is not resolved.");
            var layout = Resolve(identity);
            return new(layout.Size, layout.Alignment);
        }
        return type.Code switch
        {
            SignatureTypeCode.Boolean or SignatureTypeCode.Byte or SignatureTypeCode.SByte => new(1, 1),
            SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => new(2, 2),
            SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Single => new(4, 4),
            SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.Double => new(8, 8),
            SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr => new(Target.PointerSize, Target.PointerSize),
            _ => throw new NotSupportedException($"Field storage type {type.Code} is not supported.")
        };
    }

    private bool IsCoreType(EntityHandle handle, string ns, string name) => FrameworkTypes.Is(new(module, handle), ns, name);

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);

    private readonly record struct Storage(
        int Size,
        int Alignment);
}
