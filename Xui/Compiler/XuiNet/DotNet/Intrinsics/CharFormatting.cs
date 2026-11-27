using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>One UTF-16 code unit converted to a native JavaScript string, including isolated surrogates.</summary>
public sealed record CharFormatting(bool HasThis)
{
    public static CharFormatting? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
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
        if (name != "ToString" || !FrameworkTypes.Is(owner, "System", "Char")) return null;
        var signature = SignatureReader.Method(module, blob);
        return signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.String &&
            (signature.HasThis ? signature.Parameters.Length == 0 : signature.Parameters.Length == 1 &&
                signature.Parameters[0] == new SignatureType(SignatureTypeCode.Char)) ? new(signature.HasThis) : null;
    }
}
