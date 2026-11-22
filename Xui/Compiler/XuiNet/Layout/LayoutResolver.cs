using System.Reflection;
using System.Reflection.Metadata;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Layout;

/// <summary>Calculates and caches target-specific layouts of unmanaged structs and enums from PE metadata.</summary>
/// <remarks>Keep the source assembly alive. Reference fields, generic and auto layouts, inline arrays,
/// and unresolved external value types are rejected; no host-runtime layout is substituted.</remarks>
public sealed partial class LayoutResolver(Model.Module module, TargetLayout target)
{
    private readonly Dictionary<TypeDefinitionHandle, StructLayout> cache = new();
    private readonly HashSet<TypeDefinitionHandle> resolving = [];

    /// <summary>The explicit target used for every layout in this resolver.</summary>
    public TargetLayout Target { get; } = target;

    /// <summary>Resolves the instance layout of a struct or enum defined in the source module.</summary>
    public StructLayout Resolve(TypeDefinitionHandle handle)
    {
        if (cache.TryGetValue(handle, out var found)) return found;
        if (!resolving.Add(handle)) throw new BadImageFormatException("Recursive value-type field layout.");
        try
        {
            var layout = Calculate(handle);
            cache.Add(handle, layout);
            return layout;
        }
        finally { resolving.Remove(handle); }
    }

    private StructLayout Calculate(TypeDefinitionHandle handle)
    {
        var type = module.GetType(handle);
        bool isEnum = IsCoreType(type.BaseType, "System", "Enum");
        if (!isEnum && !IsCoreType(type.BaseType, "System", "ValueType"))
            throw new NotSupportedException($"Only unboxed value-type layout is supported: {type.Name}.");
        var definition = module.Metadata.GetTypeDefinition(handle);
        if (definition.GetGenericParameters().Count != 0)
            throw new NotSupportedException($"Generic layout is not supported: {type.Name}.");
        foreach (var attributeHandle in definition.GetCustomAttributes())
        {
            var attribute = module.Metadata.GetCustomAttribute(attributeHandle);
            if (attribute.Constructor.Kind == HandleKind.MemberReference &&
                IsCoreType(module.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
                    "System.Runtime.CompilerServices", "InlineArrayAttribute"))
                throw new NotSupportedException($"Inline-array layout is not supported: {type.Name}.");
        }
        var kind = type.Attributes & TypeAttributes.LayoutMask;
        if (!isEnum && kind is not (TypeAttributes.SequentialLayout or TypeAttributes.ExplicitLayout))
            throw new NotSupportedException($"Auto layout is not supported: {type.Name}.");
        var declared = type.DeclaredLayout;
        int pack = declared.PackingSize == 0 ? Target.DefaultPackingSize : declared.PackingSize;
        if (pack is not (1 or 2 or 4 or 8 or 16 or 32 or 64 or 128))
            throw new BadImageFormatException($"Invalid packing size {pack} for {type.Name}.");
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
            try { storage = ReadStorage(ref signature); }
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

    private Storage ReadStorage(ref BlobReader reader)
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
                return ReadStorage(ref reader);
            case SignatureTypeCode.TypeHandle:
                if (raw.ReadByte() != (byte)SignatureTypeKind.ValueType)
                    throw new NotSupportedException("Reference-field storage is not supported.");
                var handle = reader.ReadTypeHandle();
                if (handle.Kind == HandleKind.TypeDefinition)
                {
                    var layout = Resolve((TypeDefinitionHandle)handle);
                    return new(layout.Size, layout.Alignment);
                }
                if (IsCoreType(handle, "System.Runtime.InteropServices", "NFloat"))
                    return new(Target.NativeFloatSize, Target.NativeFloatSize);
                throw new NotSupportedException("External value-type layout is not resolved.");
            default:
                throw new NotSupportedException($"Field storage type {code} is not supported.");
        }
    }

    private bool IsCoreType(EntityHandle handle, string ns, string name)
    {
        if (handle.Kind != HandleKind.TypeReference) return false;
        var type = module.GetTypeReference((TypeReferenceHandle)handle);
        if (type.Namespace != ns || type.Name != name || type.ResolutionScope.Kind != HandleKind.AssemblyReference) return false;
        var assembly = module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope);
        return assembly.Name is "System.Private.CoreLib" or "System.Runtime" or "mscorlib";
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);

    private readonly record struct Storage(
        int Size,
        int Alignment);
}
