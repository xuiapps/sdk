using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>UTF-16 indexing, slicing and ordinal searching over native JavaScript strings.</summary>
public sealed record StringAccess(
    MethodSignature Signature,
    string Operation,
    StringSearchKind? Search = null,
    bool HasComparison = false)
{
    /// <summary>Recognizes supported accessors and searches; comparison overloads enforce Ordinal during execution.</summary>
    public static StringAccess? Read(Module module, EntityHandle token)
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
        if (name is not ("get_Length" or "get_Chars" or "Substring" or "IndexOf" or "Contains" or "StartsWith" or "EndsWith" or "ToLowerInvariant" or "Replace") || !FrameworkTypes.Is(owner, "System", "String")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (!signature.HasThis || signature.GenericParameterCount != 0) return null;
        if (name is "StartsWith" or "EndsWith" or "IndexOf" or "Contains" &&
            signature.Parameters.Length is 1 or 2 && signature.Parameters.All(p => !p.IsByReference) &&
            signature.Result.Code == (name == "IndexOf" ? SignatureTypeCode.Int32 : SignatureTypeCode.Boolean))
        {
            bool comparison = signature.Parameters.Length == 2 && signature.Parameters[1].Identity is { } mode &&
                FrameworkTypes.Is(mode, "System", "StringComparison");
            bool character = signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.Char && name is "StartsWith" or "EndsWith";
            bool text = signature.Parameters[0].Code == SignatureTypeCode.String && (comparison || name == "Contains" && signature.Parameters.Length == 1);
            if (character || text)
                return new(signature, name, name == "StartsWith" ? StringSearchKind.Prefix : name == "EndsWith" ? StringSearchKind.Suffix : StringSearchKind.Index, comparison);
        }
        bool integers = signature.Parameters.All(p => p.Code == SignatureTypeCode.Int32 && !p.IsByReference);
        return name switch
        {
            "Replace" when signature.Result == new SignatureType(SignatureTypeCode.String) && signature.Parameters.Length == 2 &&
                signature.Parameters[0] == signature.Parameters[1] &&
                (signature.Parameters[0] == new SignatureType(SignatureTypeCode.String) || signature.Parameters[0] == new SignatureType(SignatureTypeCode.Char)) => new(signature, name),
            "ToLowerInvariant" when signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String => new(signature, name),
            "Contains" when signature.Parameters.Length == 1 && signature.Parameters[0] == new SignatureType(SignatureTypeCode.Char) &&
                signature.Result == new SignatureType(SignatureTypeCode.Boolean) => new(signature, name),
            "IndexOf" when signature.Parameters.Length is >= 1 and <= 3 && signature.Parameters[0] == new SignatureType(SignatureTypeCode.Char) &&
                signature.Parameters.Skip(1).All(p => p == new SignatureType(SignatureTypeCode.Int32)) && signature.Result == new SignatureType(SignatureTypeCode.Int32) => new(signature, name),
            "get_Length" when signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Int32 => new(signature, name),
            "get_Chars" when integers && signature.Parameters.Length == 1 && signature.Result.Code == SignatureTypeCode.Char => new(signature, name),
            "Substring" when integers && signature.Parameters.Length is 1 or 2 && signature.Result.Code == SignatureTypeCode.String => new(signature, name),
            _ => null
        };
    }
}
