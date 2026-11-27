using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Static numeric Parse signatures, accepted only with a proven invariant-provider pair.</summary>
public sealed record NumericParsing(SignatureType Input, SignatureType Result, bool HasStyle)
{
    public bool IsSpan => Input.Identity is not null;
    public int DefaultStyle => (int)(Result.Code == SignatureTypeCode.Double
        ? System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands : System.Globalization.NumberStyles.Integer);

    public static NumericParsing? Read(Module module, EntityHandle token)
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
        if (name != "Parse") return null;
        var code = FrameworkTypes.Is(owner, "System", "Double") ? SignatureTypeCode.Double :
            FrameworkTypes.Is(owner, "System", "UInt32") ? SignatureTypeCode.UInt32 : SignatureTypeCode.Void;
        if (code == SignatureTypeCode.Void) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result != new SignatureType(code) ||
            signature.Parameters.Length is not (2 or 3) || signature.Parameters.Any(p => p.IsByReference) ||
            signature.Parameters[^1].Identity is not { } provider || !FrameworkTypes.Is(provider, "System", "IFormatProvider")) return null;
        var input = signature.Parameters[0];
        if (input.Code != SignatureTypeCode.String && !(input.Identity is { } span && FrameworkTypes.Is(span, "System", "ReadOnlySpan`1") &&
            span.Arguments?.Values.Length == 1 && span.Arguments.Values[0] == new SignatureType(SignatureTypeCode.Char))) return null;
        bool style = signature.Parameters.Length == 3;
        if (style && (signature.Parameters[1].Identity is not { } styles || !FrameworkTypes.Is(styles, "System.Globalization", "NumberStyles"))) return null;
        return new(input, signature.Result, style);
    }
}
