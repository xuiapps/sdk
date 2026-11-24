using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>Reads declared signatures and substitutes constructed class arguments without erasing type identity.</summary>
public static class SignatureReader
{
    /// <summary>Reads one supported declared type from a borrowed metadata cursor.</summary>
    public static SignatureType Type(Module module, ref BlobReader reader, GenericArguments? arguments = null)
    {
        var raw = reader;
        var code = reader.ReadSignatureTypeCode();
        switch (code)
        {
            case SignatureTypeCode.TypeHandle:
                byte kind = raw.ReadByte();
                if (kind is not ((byte)SignatureTypeKind.Class) and not ((byte)SignatureTypeKind.ValueType))
                    throw new NotSupportedException("Invalid named type signature.");
                bool isReference = kind == (byte)SignatureTypeKind.Class;
                var handle = reader.ReadTypeHandle();
                if (handle.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference))
                    throw new NotSupportedException($"Constructed type signatures are not implemented: {handle.Kind}.");
                return new(code, module.ResolveType(handle), IsReferenceType: isReference);
            case SignatureTypeCode.GenericTypeInstance:
                var definition = Type(module, ref reader, arguments);
                if (definition.Code != SignatureTypeCode.TypeHandle || definition.Identity is not { } identity || identity.Arguments is not null)
                    throw new BadImageFormatException("Expected a generic type declaration.");
                int arity = reader.ReadCompressedInteger();
                if (arity <= 0) throw new BadImageFormatException("Generic instantiation requires type arguments.");
                var values = new SignatureType[arity];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = NonVoid(module, ref reader, arguments);
                    if (values[i].IsByReference) throw new NotSupportedException("Byref generic arguments are not implemented.");
                }
                if (identity.Handle.Kind == HandleKind.TypeDefinition && identity.Module.GetType((TypeDefinitionHandle)identity.Handle).GenericParameterCount != arity)
                    throw new BadImageFormatException("Generic argument count differs from the type declaration.");
                return definition with { Identity = identity with { Arguments = new GenericArguments(values) } };
            case SignatureTypeCode.GenericTypeParameter:
            case SignatureTypeCode.GenericMethodParameter:
                int index = reader.ReadCompressedInteger();
                if (index < 0) throw new BadImageFormatException("Invalid generic parameter index.");
                if (code == SignatureTypeCode.GenericTypeParameter && arguments is not null)
                    return index < arguments.Values.Length ? arguments.Values[index] : throw new BadImageFormatException("Generic parameter index exceeds the supplied context.");
                return new(code, GenericParameterIndex: index);
            case SignatureTypeCode.RequiredModifier:
                var modifier = module.ResolveType(reader.ReadTypeHandle());
                if (IsFrameworkInModifier(modifier))
                {
                    var qualified = Type(module, ref reader, arguments);
                    if (!qualified.IsByReference) throw new BadImageFormatException("InAttribute requires a managed-byref signature.");
                    return qualified with { IsReadOnlyByReference = true };
                }
                throw new NotSupportedException($"Required signature modifier is not implemented: {modifier}.");
            case SignatureTypeCode.ByReference:
                return Type(module, ref reader, arguments).ByReference();
            case SignatureTypeCode.SZArray:
                return SignatureType.Vector(NonVoid(module, ref reader, arguments));
            case SignatureTypeCode.Object:
                return new(code, IsReferenceType: true);
            case SignatureTypeCode.Void:
            case SignatureTypeCode.Boolean:
            case SignatureTypeCode.Char:
            case SignatureTypeCode.SByte:
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Int16:
            case SignatureTypeCode.UInt16:
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32:
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64:
            case SignatureTypeCode.Single:
            case SignatureTypeCode.Double:
            case SignatureTypeCode.IntPtr:
            case SignatureTypeCode.UIntPtr:
            case SignatureTypeCode.String:
                return new(code);
            default:
                throw new NotSupportedException($"Unsupported signature type: {code}.");
        }
    }

    /// <summary>Reads a non-generic method signature in an optional generic declaring-type context.</summary>
    public static MethodSignature Method(Module module, BlobReader reader, GenericArguments? arguments = null)
    {
        var header = reader.ReadSignatureHeader();
        if ((header.RawValue & ~(byte)SignatureAttributes.Instance) != (byte)SignatureKind.Method)
            throw new NotSupportedException("Only non-generic methods with the default calling convention are supported.");
        int count = reader.ReadCompressedInteger();
        if (count < 0) throw new BadImageFormatException("Invalid parameter count.");
        var result = Type(module, ref reader, arguments);
        var parameters = ImmutableArray.CreateBuilder<SignatureType>(count);
        for (int i = 0; i < count; i++) parameters.Add(NonVoid(module, ref reader, arguments));
        Complete(reader);
        return new(parameters.MoveToImmutable(), result, header.IsInstance);
    }

    /// <summary>Reads locals, preserving small integer storage types and concrete value-type identities.</summary>
    public static ImmutableArray<SignatureType> Locals(Module module, BlobReader reader, GenericArguments? arguments = null)
    {
        if (reader.Length == 0) return [];
        if (reader.ReadSignatureHeader().Kind != SignatureKind.LocalVariables)
            throw new BadImageFormatException("Expected a local variable signature.");
        int count = reader.ReadCompressedInteger();
        if (count < 0 || count > ushort.MaxValue) throw new BadImageFormatException("Invalid local variable count.");
        var locals = ImmutableArray.CreateBuilder<SignatureType>(count);
        for (int i = 0; i < count; i++) locals.Add(NonVoid(module, ref reader, arguments));
        Complete(reader);
        return locals.MoveToImmutable();
    }

    /// <summary>Reads a field's declared storage type.</summary>
    public static SignatureType Field(Module module, BlobReader reader, GenericArguments? arguments = null)
    {
        if (reader.ReadSignatureHeader().Kind != SignatureKind.Field) throw new BadImageFormatException("Expected a field signature.");
        var type = NonVoid(module, ref reader, arguments);
        Complete(reader);
        return type;
    }

    /// <summary>Resolves a type token, substituting TypeSpec signatures in the supplied context.</summary>
    public static SignatureType Resolve(Module module, EntityHandle handle, GenericArguments? arguments = null)
    {
        if (handle.Kind == HandleKind.TypeSpecification) return module.GetTypeSpecification((TypeSpecificationHandle)handle).Read(arguments);
        if (handle.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference))
            throw new NotSupportedException($"Unsupported type token: {handle.Kind}.");
        var identity = module.ResolveType(handle);
        return new(SignatureTypeCode.TypeHandle, identity, IsReferenceType: !identity.IsValueType);
    }

    /// <summary>Resolves a storage operand, normalizing named framework primitives to signature codes.</summary>
    public static SignatureType StorageType(Module module, EntityHandle handle, GenericArguments? arguments = null)
    {
        var type = Resolve(module, handle, arguments);
        if (type.Identity is not { } identity) return type;
        foreach (var code in new[] { SignatureTypeCode.Boolean, SignatureTypeCode.Char, SignatureTypeCode.SByte,
            SignatureTypeCode.Byte, SignatureTypeCode.Int16, SignatureTypeCode.UInt16, SignatureTypeCode.Int32,
            SignatureTypeCode.UInt32, SignatureTypeCode.Int64, SignatureTypeCode.UInt64, SignatureTypeCode.Single,
            SignatureTypeCode.Double, SignatureTypeCode.IntPtr, SignatureTypeCode.UIntPtr, SignatureTypeCode.String, SignatureTypeCode.Object })
            if (FrameworkTypes.Is(identity, "System", code.ToString()))
                return new(code, IsReferenceType: code == SignatureTypeCode.Object);
        return type;
    }

    // C# emits this required modifier for readonly byrefs on virtual/interface methods.
    // It changes the source contract, but uses the same managed-address calling convention.
    private static bool IsFrameworkInModifier(TypeIdentity identity) => FrameworkTypes.Is(identity, "System.Runtime.InteropServices", "InAttribute");

    private static SignatureType NonVoid(Module module, ref BlobReader reader, GenericArguments? arguments)
    {
        var type = Type(module, ref reader, arguments);
        return type.Code == SignatureTypeCode.Void ? throw new BadImageFormatException("Void is not a storage type.") : type;
    }

    private static void Complete(BlobReader reader)
    {
        if (reader.RemainingBytes != 0) throw new BadImageFormatException("Unexpected trailing signature bytes.");
    }
}
