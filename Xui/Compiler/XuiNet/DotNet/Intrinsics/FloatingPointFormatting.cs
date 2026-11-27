using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Floating-point ToString(format, provider), accepted only with a proven invariant-provider pair.</summary>
public static class FloatingPointFormatting
{
    public static SignatureType? Read(Module module, EntityHandle token)
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
        if (name != "ToString") return null;
        var code = FrameworkTypes.Is(owner, "System", "Double") ? SignatureTypeCode.Double :
            FrameworkTypes.Is(owner, "System", "Single") || NativeFloat.Is(owner) ? SignatureTypeCode.Single : SignatureTypeCode.Void;
        if (code == SignatureTypeCode.Void) return null;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.String &&
            signature.Parameters.Length == 2 && signature.Parameters[0].Code == SignatureTypeCode.String &&
            signature.Parameters[1].Identity is { } provider && FrameworkTypes.Is(provider, "System", "IFormatProvider") &&
            !signature.Parameters[0].IsByReference && !signature.Parameters[1].IsByReference
                ? NativeFloat.Is(owner) ? new(SignatureTypeCode.TypeHandle, module.ResolveType(owner.Handle)) : new(code)
                : null;
    }
}
