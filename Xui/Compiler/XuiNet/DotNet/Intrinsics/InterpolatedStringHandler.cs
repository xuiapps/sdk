using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework interpolation surface backed by a compiler-owned UTF-16 buffer.</summary>
/// <remarks>Default integer formatting uses invariant decimal digits. Providers, formats, alignment,
/// floating-point values, and arbitrary object formatting are deliberately unsupported.</remarks>
public sealed record InterpolatedStringHandler(string Operation, TypeIdentity Owner, MethodSignature Signature)
{
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
        if (!Is(owner)) return null;
        var arguments = token.Kind == HandleKind.MethodSpecification
            ? module.GetMethodSpecification((MethodSpecificationHandle)token).Read(context, methodArguments) : null;
        var signature = SignatureReader.Method(module, blob, context, arguments);
        var parameters = signature.Parameters;
        bool supported = signature.HasThis && signature.GenericParameterCount <= 1 && name switch
        {
            ".ctor" => parameters.Length == 2 && parameters.All(p => p.Code == SignatureTypeCode.Int32 && !p.IsByReference) && signature.Result.Code == SignatureTypeCode.Void,
            "ToString" or "ToStringAndClear" => parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String,
            "AppendLiteral" => parameters.Length == 1 && parameters[0].Code == SignatureTypeCode.String && !parameters[0].IsByReference && signature.Result.Code == SignatureTypeCode.Void,
            "AppendFormatted" => parameters.Length == 1 && !parameters[0].IsByReference && parameters[0].Code is
                SignatureTypeCode.String or SignatureTypeCode.Char or SignatureTypeCode.Boolean or
                SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
                SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 && signature.Result.Code == SignatureTypeCode.Void,
            _ => false
        };
        if (!supported) throw new NotSupportedException($"Interpolation member is not implemented: {name}({string.Join(", ", parameters)}). Supported values are strings, characters, booleans, and integers without formats, alignment, or providers.");
        return new(name, owner.Module.ResolveType(owner.Handle), signature);
    }
}
