using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>UTF-16 extended grapheme boundaries supplied by the host Unicode implementation.</summary>
public sealed record TextElement(MethodSignature Signature)
{
    public static TextElement? Read(Module module, EntityHandle token)
    {
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || member.Name != "GetNextTextElementLength" ||
            member.Parent.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference) ||
            !FrameworkTypes.Is(new(module, member.Parent), "System.Globalization", "StringInfo")) return null;
        var signature = SignatureReader.Method(module, member.Signature);
        return !signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters is [{ IsByReference: false, Identity: { } span }] &&
            FrameworkTypes.Is(span, "System", "ReadOnlySpan`1") && span.Arguments?.Values is [{ Code: SignatureTypeCode.Char }]
            ? new(signature) : null;
    }
}
