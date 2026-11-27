using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Reads an enum's underlying integer storage without erasing its nominal type identity.</summary>
public static class EnumInfo
{
    /// <summary>Reads named enum values in declaration order without loading a runtime Type.</summary>
    public static IReadOnlyList<EnumConstant> Constants(TypeIdentity identity)
    {
        if (UnderlyingType(identity) is null) throw new ArgumentException("Expected an enum.", nameof(identity));
        var result = new List<EnumConstant>();
        var metadata = identity.Module.Metadata;
        foreach (var handle in metadata.GetTypeDefinition((TypeDefinitionHandle)identity.Handle).GetFields())
        {
            var field = metadata.GetFieldDefinition(handle);
            if ((field.Attributes & FieldAttributes.Literal) == 0) continue;
            var constant = metadata.GetConstant(field.GetDefaultValue());
            var value = metadata.GetBlobReader(constant.Value);
            ulong bits = constant.TypeCode switch
            {
                ConstantTypeCode.SByte or ConstantTypeCode.Byte => value.ReadByte(),
                ConstantTypeCode.Int16 or ConstantTypeCode.UInt16 => value.ReadUInt16(),
                ConstantTypeCode.Int32 or ConstantTypeCode.UInt32 => value.ReadUInt32(),
                ConstantTypeCode.Int64 or ConstantTypeCode.UInt64 => value.ReadUInt64(),
                _ => throw new BadImageFormatException("An enum constant must use integer storage.")
            };
            result.Add(new(metadata.GetString(field.Name), bits));
        }
        return result;
    }

    /// <summary>Whether the enum declares the framework FlagsAttribute.</summary>
    public static bool IsFlags(TypeIdentity identity)
    {
        if (UnderlyingType(identity) is null) return false;
        var module = identity.Module;
        foreach (var handle in module.Metadata.GetTypeDefinition((TypeDefinitionHandle)identity.Handle).GetCustomAttributes())
        {
            var attribute = module.Metadata.GetCustomAttribute(handle);
            EntityHandle owner = attribute.Constructor.Kind switch
            {
                HandleKind.MemberReference => module.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
                HandleKind.MethodDefinition => module.GetMethod((MethodDefinitionHandle)attribute.Constructor).DeclaringType,
                _ => default
            };
            if (!owner.IsNil && FrameworkTypes.Is(new(module, owner), "System", "FlagsAttribute")) return true;
        }
        return false;
    }

    /// <summary>Returns the value__ field's integer signature, or null when the type is not an enum.</summary>
    /// <remarks>Small integers retain their storage width and signedness here; stack analysis promotes them separately.</remarks>
    public static SignatureTypeCode? UnderlyingType(TypeIdentity identity)
    {
        if (identity.Handle.Kind != HandleKind.TypeDefinition) return null;
        var definition = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        if (definition.BaseType.IsNil || !FrameworkTypes.Is(new(identity.Module, definition.BaseType), "System", "Enum")) return null;
        SignatureTypeCode? result = null;
        foreach (var handle in definition.Fields)
        {
            var field = identity.Module.GetField(handle);
            if ((field.Attributes & FieldAttributes.Static) != 0) continue;
            var type = SignatureReader.Field(identity.Module, field.Signature);
            if (result is not null || field.Name != "value__" || type.IsByReference || type.Code is not
                (SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
                 SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64))
                throw new NotSupportedException($"Unsupported enum underlying storage: {identity}.{field.Name}: {type}.");
            result = type.Code;
        }
        return result ?? throw new BadImageFormatException($"Enum {identity} has no value__ field.");
    }

    /// <summary>Gets the scalar storage signature for an enum value; other signatures are unchanged.</summary>
    public static SignatureType Storage(SignatureType type) => type.Identity is { } identity && UnderlyingType(identity) is { } code
        ? new(code, IsByReference: type.IsByReference, IsReadOnlyByReference: type.IsReadOnlyByReference) : type;
}
