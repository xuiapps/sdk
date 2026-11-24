using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

public sealed partial record DefaultEquality
{
    private static DefaultEquality? ReadArraySearch(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
    {
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
        if (name != "IndexOf" || !FrameworkTypes.Is(owner, "System", "Array")) return null;
        var arguments = spec.Read(context, methodArguments);
        if (arguments.Values.Length != 1) return null;
        var element = arguments.Values[0];
        var signature = SignatureReader.Method(module, blob, context, arguments);
        if (signature.HasThis || signature.GenericParameterCount != 1 || signature.Result.Code != SignatureTypeCode.Int32 ||
            signature.Parameters.Length is < 2 or > 4 || signature.Parameters[0] != SignatureType.Vector(element) ||
            signature.Parameters[1] != element || signature.Parameters.Skip(2).Any(p => p != new SignatureType(SignatureTypeCode.Int32))) return null;
        return Create(name, owner, element, signature);
    }
}
