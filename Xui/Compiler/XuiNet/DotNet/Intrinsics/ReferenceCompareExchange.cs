using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Recognizes framework reference CompareExchange calls, including their runtime-provided implementation.</summary>
public static class ReferenceCompareExchange
{
    /// <summary>Reads a closed reference signature without traversing the framework's native or generic wrapper body.</summary>
    public static MethodSignature? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        var declaration = token.Kind == HandleKind.MethodSpecification
            ? module.GetMethodSpecification((MethodSpecificationHandle)token).Method : token;
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (declaration.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)declaration);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (declaration.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)declaration);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        if (name is not ("CompareExchange" or "CompareExchangeObject") || !FrameworkTypes.Is(owner, "System.Threading", "Interlocked")) return null;
        var arguments = token.Kind == HandleKind.MethodSpecification
            ? module.GetMethodSpecification((MethodSpecificationHandle)token).Read(context, methodArguments) : null;
        var signature = SignatureReader.Method(module, blob, context, arguments);
        var value = signature.Result;
        if (signature.HasThis || signature.GenericParameterCount > 1 || value.IsByReference || value.ContainsGenericParameters ||
            signature.Parameters.Length != 3 || signature.Parameters[0] != value.ByReference() ||
            signature.Parameters[1] != value || signature.Parameters[2] != value) return null;
        if (value.Code == SignatureTypeCode.String)
            throw new NotSupportedException("Reference CompareExchange on strings requires managed string identity; native JavaScript strings cannot preserve it.");
        return value.Code is SignatureTypeCode.Object or SignatureTypeCode.SZArray || value.IsReferenceType && value.Identity is { IsValueType: false }
            ? signature : null;
    }
}
