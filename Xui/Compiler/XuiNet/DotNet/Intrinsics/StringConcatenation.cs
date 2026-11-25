using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Fixed-arity string concatenation over native JavaScript UTF-16 strings.</summary>
public sealed record StringConcatenation(
    MethodSignature Signature)
{
    /// <summary>Recognizes the two-, three-, and four-string Concat overloads; object and collection overloads are excluded.</summary>
    public static StringConcatenation? Read(Module module, EntityHandle token)
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
        return name == "Concat" && !signature.HasThis && signature.GenericParameterCount == 0 &&
            signature.Result.Code == SignatureTypeCode.String && signature.Parameters.Length is >= 2 and <= 4 &&
            signature.Parameters.All(p => p.Code == SignatureTypeCode.String && !p.IsByReference)
            ? new(signature) : null;
    }
}
