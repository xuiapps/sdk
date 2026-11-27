using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Default decimal Int32 formatting for the invariant JavaScript-string runtime.</summary>
public static class Int32Formatting
{
    /// <summary>Recognizes Int32.ToString. Provider overloads require a proven adjacent invariant-provider pair; a format argument additionally requires a proven null.</summary>
    public static bool Is(Module module, EntityHandle token, bool withProvider = false, bool withFormat = false)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return false;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return false;
        if (name != "ToString" || !FrameworkTypes.Is(owner, "System", "Int32")) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.String &&
            (withProvider ? signature.Parameters.Length == (withFormat ? 2 : 1) &&
                (!withFormat || signature.Parameters[0] == new SignatureType(SignatureTypeCode.String)) && signature.Parameters[^1].Identity is { } provider &&
                FrameworkTypes.Is(provider, "System", "IFormatProvider") && !signature.Parameters[^1].IsByReference
                : signature.Parameters.Length == 0);
    }

    /// <summary>The trusted CultureInfo or NumberFormatInfo invariant singleton getter.</summary>
    public static SignatureType? InvariantProvider(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        string providerName;
        if (name == "get_InvariantCulture" && FrameworkTypes.Is(owner, "System.Globalization", "CultureInfo")) providerName = "CultureInfo";
        else if (name == "get_InvariantInfo" && FrameworkTypes.Is(owner, "System.Globalization", "NumberFormatInfo")) providerName = "NumberFormatInfo";
        else return null;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result.Identity is { } type && FrameworkTypes.Is(type, "System.Globalization", providerName) ? signature.Result : null;
    }
}
