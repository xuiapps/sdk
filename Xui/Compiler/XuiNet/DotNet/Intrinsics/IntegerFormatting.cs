using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Integral general formatting, accepted only with a proven invariant provider and, if present, null format.</summary>
public static class IntegerFormatting
{
    public static SignatureType? Read(Module module, EntityHandle token, bool withFormat = false)
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
        if (name != "ToString") return null;
        SignatureTypeCode code = SignatureTypeCode.Void;
        foreach (var candidate in new[] { SignatureTypeCode.SByte, SignatureTypeCode.Byte, SignatureTypeCode.Int16,
            SignatureTypeCode.UInt16, SignatureTypeCode.Int32, SignatureTypeCode.UInt32, SignatureTypeCode.Int64, SignatureTypeCode.UInt64 })
            if (FrameworkTypes.Is(owner, "System", candidate.ToString())) { code = candidate; break; }
        if (code == SignatureTypeCode.Void) return null;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.String &&
            signature.Parameters.Length == (withFormat ? 2 : 1) &&
            (!withFormat || signature.Parameters[0] == new SignatureType(SignatureTypeCode.String)) &&
            !signature.Parameters[^1].IsByReference && signature.Parameters[^1].Identity is { } provider &&
            FrameworkTypes.Is(provider, "System", "IFormatProvider")
                ? new(code) : null;
    }
}
