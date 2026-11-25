using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A framework null managed-reference operation, independent of native pointer representation.</summary>
public sealed record ManagedReference(
    string Operation,
    SignatureType Element,
    MethodSignature Signature)
{
    /// <summary>Recognizes closed Unsafe.NullRef and Unsafe.IsNullRef calls in references and implementation assemblies.</summary>
    public static ManagedReference? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
    {
        if (token.Kind != HandleKind.MethodSpecification) return null;
        var spec = module.GetMethodSpecification((MethodSpecificationHandle)token);
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (spec.Method.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)spec.Method);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (spec.Method.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)spec.Method);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        if (name is not ("NullRef" or "IsNullRef") || !FrameworkTypes.Is(owner, "System.Runtime.CompilerServices", "Unsafe")) return null;
        var arguments = spec.Read(context, methodArguments);
        if (arguments.Values.Length != 1 || arguments.Values[0].ContainsGenericParameters) return null;
        var element = arguments.Values[0];
        var signature = SignatureReader.Method(module, blob, context, arguments);
        if (signature.HasThis || signature.GenericParameterCount != 1) return null;
        bool valid = name == "NullRef"
            ? signature.Parameters.Length == 0 && signature.Result == element.ByReference()
            : signature.Parameters.Length == 1 && signature.Parameters[0] == element.ByReference() && signature.Result == new SignatureType(SignatureTypeCode.Boolean);
        return valid ? new(name, element, signature) : null;
    }
}
