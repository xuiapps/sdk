using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework interpolation surface backed by a compiler-owned UTF-16 buffer.</summary>
/// <remarks>Formatting uses the invariant browser runtime. Numeric values accept its supported
/// numeric formats and the invariant singleton provider; custom providers, alignment and arbitrary object formatting remain unsupported.</remarks>
public sealed record InterpolatedStringHandler(string Operation, TypeIdentity Owner, MethodSignature Signature)
{
    /// <summary>Whether this append formats a floating-point primitive or NFloat.</summary>
    public bool IsFloatingPoint => Signature.Parameters.Length > 0 && Floating(Signature.Parameters[0]);
    /// <summary>Whether floating-point formatting uses single-precision decimal semantics.</summary>
    public bool IsSingle => IsFloatingPoint && Signature.Parameters[0].Code != SignatureTypeCode.Double;
    /// <summary>Whether this append retains the closed enum's name and flags metadata.</summary>
    public bool IsEnum => Signature.Parameters.Length == 1 && Enum(Signature.Parameters[0]);

    /// <summary>Whether an integer append supplies an explicit numeric format.</summary>
    public bool IsFormattedInteger => Operation == "AppendFormatted" && Signature.Parameters.Length == 2 && Integer(Signature.Parameters[0]);
    private static bool Integer(SignatureType type) => !type.IsByReference && type.Code is
        SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
        SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64;

    /// <summary>Whether construction includes an explicit formatting provider.</summary>
    public bool HasProvider => Operation == ".ctor" && Signature.Parameters.Length == 3;
    private static bool IsProvider(SignatureType type) => !type.IsByReference && type.Identity is { } id && FrameworkTypes.Is(id, "System", "IFormatProvider");

    private static bool Enum(SignatureType type) => !type.IsByReference && type.Identity is { } id && EnumInfo.UnderlyingType(id) is not null;

    private static bool Floating(SignatureType type) => !type.IsByReference &&
        (type.Code is SignatureTypeCode.Single or SignatureTypeCode.Double || type.Identity is { } id && NativeFloat.Is(id));
    /// <summary>Recognizes the trusted framework handler, never an application type with the same name.</summary>
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Runtime.CompilerServices", "DefaultInterpolatedStringHandler");

    /// <summary>Decodes a supported member or rejects unsupported members before traversing their BCL bodies.</summary>
    public static InterpolatedStringHandler? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        var declaration = token.Kind == HandleKind.MethodSpecification
            ? module.GetMethodSpecification((MethodSpecificationHandle)token).Method : token;
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (declaration.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)declaration);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (declaration.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)declaration);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        if (FrameworkTypes.Is(owner, "System", "String") && name == "Create")
        {
            var create = SignatureReader.Method(module, blob);
            if (!create.HasThis && create.GenericParameterCount == 0 && create.Result.Code == SignatureTypeCode.String &&
                create.Parameters.Length == 2 && IsProvider(create.Parameters[0]) &&
                create.Parameters[1] is { IsByReference: true, Identity: { } handler } && Is(handler))
                return new("Create", handler, create);
            return null;
        }
        if (!Is(owner)) return null;
        var arguments = token.Kind == HandleKind.MethodSpecification
            ? module.GetMethodSpecification((MethodSpecificationHandle)token).Read(context, methodArguments) : null;
        var signature = SignatureReader.Method(module, blob, context, arguments);
        var parameters = signature.Parameters;
        bool supported = signature.HasThis && signature.GenericParameterCount <= 1 && name switch
        {
            ".ctor" => parameters.Length is 2 or 3 && parameters.Take(2).All(p => p.Code == SignatureTypeCode.Int32 && !p.IsByReference) && (parameters.Length == 2 || IsProvider(parameters[2])) && signature.Result.Code == SignatureTypeCode.Void,
            "ToString" or "ToStringAndClear" => parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String,
            "AppendLiteral" => parameters.Length == 1 && parameters[0].Code == SignatureTypeCode.String && !parameters[0].IsByReference && signature.Result.Code == SignatureTypeCode.Void,
            "AppendFormatted" => signature.Result.Code == SignatureTypeCode.Void &&
                (parameters.Length == 2 && Integer(parameters[0]) && parameters[1] == new SignatureType(SignatureTypeCode.String) ||
                parameters.Length == 1 && Enum(parameters[0]) || parameters.Length is 1 or 2 && Floating(parameters[0]) &&
                    (parameters.Length == 1 || parameters[1] == new SignatureType(SignatureTypeCode.String)) ||
                parameters.Length == 1 && !parameters[0].IsByReference && parameters[0].Code is
                SignatureTypeCode.String or SignatureTypeCode.Char or SignatureTypeCode.Boolean or
                SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
                SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64),
            _ => false
        };
        if (!supported) throw new NotSupportedException($"Interpolation member is not implemented: {name}({string.Join(", ", parameters)}). Supported values are strings, characters, booleans, integers and floating-point values with supported invariant formats; alignment and custom providers are not implemented.");
        return new(name, owner.Module.ResolveType(owner.Handle), signature);
    }
}
