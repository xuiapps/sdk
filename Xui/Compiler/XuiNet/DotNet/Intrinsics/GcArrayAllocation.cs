using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Closed GC array factories backed by ordinary zero-initialized WASM GC vectors.</summary>
public sealed record GcArrayAllocation(string Operation, SignatureType Element, MethodSignature Signature)
{
    public static GcArrayAllocation? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
    {
        if (token.Kind != HandleKind.MethodSpecification) return null;
        var spec = module.GetMethodSpecification((MethodSpecificationHandle)token);
        TypeIdentity owner; string name; BlobReader blob;
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
        if (name is not ("AllocateArray" or "AllocateUninitializedArray") || !FrameworkTypes.Is(owner, "System", "GC")) return null;
        var arguments = spec.Read(context, methodArguments);
        var signature = SignatureReader.Method(module, blob, context, arguments);
        if (arguments.Values.Length != 1 || signature.HasThis || signature.GenericParameterCount != 1 ||
            signature.Parameters.Length != 2 || signature.Parameters[0] != new SignatureType(SignatureTypeCode.Int32) ||
            signature.Parameters[1] != new SignatureType(SignatureTypeCode.Boolean) || signature.Result != SignatureType.Vector(arguments.Values[0])) return null;
        if (arguments.ContainsGenericParameters) throw new NotSupportedException("GC array allocation requires a closed element type.");
        return new(name, arguments.Values[0], signature);
    }
}
