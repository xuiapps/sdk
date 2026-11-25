using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Recognizes the runtime reference-content query whose body is a CLR intrinsic stub.</summary>
public static class ReferenceContent
{
    /// <summary>Returns the closed type queried by RuntimeHelpers.IsReferenceOrContainsReferences, with its signature checked.</summary>
    public static SignatureType? Read(Module module, EntityHandle token, GenericArguments? types, GenericArguments? methods)
    {
        if (token.Kind != HandleKind.MethodSpecification) return null;
        var spec = module.GetMethodSpecification((MethodSpecificationHandle)token);
        TypeIdentity owner; string name; BlobReader blob;
        if (spec.Method.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)spec.Method);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else if (spec.Method.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)spec.Method);
            if (member.Kind != MemberReferenceKind.Method) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else return null;
        if (name != "IsReferenceOrContainsReferences" || !FrameworkTypes.Is(owner, "System.Runtime.CompilerServices", "RuntimeHelpers")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 1 || signature.Parameters.Length != 0 ||
            signature.Result.Code != SignatureTypeCode.Boolean || signature.Result.IsByReference) return null;
        var arguments = spec.Read(types, methods);
        return arguments.Values.Length == 1 ? arguments.Values[0] : null;
    }
}
