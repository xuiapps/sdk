using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Default decimal Int32 formatting for the invariant JavaScript-string runtime.</summary>
public static class Int32Formatting
{
    /// <summary>Recognizes Int32.ToString. The provider overload is accepted only when the caller proves an adjacent invariant-provider pair.</summary>
    public static bool Is(Module module, EntityHandle token, bool withProvider = false)
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
            (withProvider ? signature.Parameters.Length == 1 && signature.Parameters[0].Identity is { } provider &&
                FrameworkTypes.Is(provider, "System", "IFormatProvider") && !signature.Parameters[0].IsByReference
                : signature.Parameters.Length == 0);
    }

    /// <summary>The exact invariant singleton getter, used only inside a proven adjacent formatting pair.</summary>
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
        if (name != "get_InvariantCulture" || !FrameworkTypes.Is(owner, "System.Globalization", "CultureInfo")) return null;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result.Identity is { } type && FrameworkTypes.Is(type, "System.Globalization", "CultureInfo") ? signature.Result : null;
    }
}
