using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework object null guard; pointer overloads retain their unsupported status.</summary>
public static class NullGuard
{
    /// <summary>Recognizes ArgumentNullException.ThrowIfNull(object, string) in references and implementation PEs.</summary>
    public static bool Is(Module module, EntityHandle token)
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
        if (name != "ThrowIfNull" || !FrameworkTypes.Is(owner, "System", "ArgumentNullException")) return false;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.Void &&
            signature.Parameters.Length == 2 && signature.Parameters[0].Code == SignatureTypeCode.Object &&
            signature.Parameters[1].Code == SignatureTypeCode.String && signature.Parameters.All(p => !p.IsByReference);
    }
}
