using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>String splitting over native JavaScript UTF-16 strings, returning a managed string vector.</summary>
public sealed record StringSplit(
    MethodSignature Signature,
    bool HasCount,
    bool HasOptions)
{
    /// <summary>Recognizes char, string, char[] and string[] separators with optional count and StringSplitOptions.</summary>
    public static StringSplit? Read(Module module, EntityHandle token)
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
        if (!FrameworkTypes.Is(owner, "System", "String")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (name != "Split" || !signature.HasThis || signature.GenericParameterCount != 0 ||
            signature.Result.Array?.Element.Code != SignatureTypeCode.String || signature.Parameters.Length is < 1 or > 3)
            return null;
        var separator = signature.Parameters[0];
        if (separator.IsByReference || !(separator.Code is SignatureTypeCode.Char or SignatureTypeCode.String ||
            separator.Code == SignatureTypeCode.SZArray && separator.Array?.Element.Code is SignatureTypeCode.Char or SignatureTypeCode.String)) return null;
        bool count = false, options = false;
        for (int i = 1; i < signature.Parameters.Length; i++)
        {
            var parameter = signature.Parameters[i];
            if (parameter.IsByReference) return null;
            if (parameter.Code == SignatureTypeCode.Int32 && !count && !options) count = true;
            else if (parameter.Identity is { } type && FrameworkTypes.Is(type, "System", "StringSplitOptions") && !options) options = true;
            else return null;
        }
        return new(signature, count, options);
    }
}
