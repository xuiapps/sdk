using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>The nominal managed type and scalar payload of a supported boxed value.</summary>
/// <remarks>Payload normalization never erases the identity used by GetType and type tests.</remarks>
public readonly record struct BoxingType(
    TypeIdentity Identity,
    SignatureType Storage)
{
    /// <summary>Resolves primitive, enum, and NFloat boxes; reference operands return null.</summary>
    public static BoxingType? Read(Module module, EntityHandle token, GenericArguments? types = null, GenericArguments? methods = null)
    {
        var value = SignatureReader.StorageType(module, token, types, methods);
        if (value.IsReferenceType || value.Code is SignatureTypeCode.Object or SignatureTypeCode.String or SignatureTypeCode.SZArray) return null;
        var declared = SignatureReader.Resolve(module, token, types, methods);
        var identity = declared.Identity ?? PrimitiveIdentity(module, value.Code);
        var storage = EnumInfo.Storage(value);
        if (Intrinsics.NativeFloat.Is(identity)) storage = new(SignatureTypeCode.Single);
        if (storage.IsByReference || storage.Identity is not null || storage.Code is not
            (SignatureTypeCode.Boolean or SignatureTypeCode.Char or SignatureTypeCode.SByte or SignatureTypeCode.Byte or
             SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or
             SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or
             SignatureTypeCode.Single or SignatureTypeCode.Double))
            throw new NotSupportedException($"Boxing/unboxing is currently implemented for primitives, enums, and NFloat only: {declared}.");
        if (identity.Handle.Kind != HandleKind.TypeDefinition)
            throw new NotSupportedException($"Boxing requires a linked type definition: {identity}.");
        return new(identity, storage);
    }

    /// <summary>CLI unboxing accepts an enum and its exact underlying type, including another enum of that type.</summary>
    public bool CanUnbox(BoxingType source) => Identity == source.Identity || Storage == source.Storage &&
        (EnumInfo.UnderlyingType(Identity) is not null || EnumInfo.UnderlyingType(source.Identity) is not null);

    internal static TypeIdentity PrimitiveIdentity(Module module, SignatureTypeCode code)
    {
        var compilation = module.Compilation ?? throw new NotSupportedException("Primitive runtime metadata requires linked framework assemblies.");
        var core = compilation.Assemblies.FirstOrDefault(a => a.Identity.Name is "System.Private.CoreLib" or "mscorlib")
            ?? throw new NotSupportedException("Primitive runtime metadata requires a linked CoreLib.");
        return compilation.ResolveType(core.ManifestModule, "System", code.ToString());
    }
}
