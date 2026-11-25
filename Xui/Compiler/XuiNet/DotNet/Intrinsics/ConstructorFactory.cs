using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A closed generic Activator factory with a statically selected public parameterless class constructor.</summary>
public static class ConstructorFactory
{
    /// <summary>Resolves new T() without runtime reflection; value types and missing public constructors remain explicit gaps.</summary>
    public static MethodIdentity? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
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
        if (name != "CreateInstance" || !FrameworkTypes.Is(owner, "System", "Activator")) return null;
        var arguments = spec.Read(context, methodArguments);
        var signature = SignatureReader.Method(module, blob, context, arguments);
        if (arguments.Values.Length != 1 || signature.HasThis || signature.GenericParameterCount != 1 ||
            signature.Parameters.Length != 0 || signature.Result != arguments.Values[0]) return null;
        if (signature.Result.Identity is not { Handle.Kind: HandleKind.TypeDefinition, IsValueType: false, IsInterface: false } type ||
            (type.Module.GetType((TypeDefinitionHandle)type.Handle).Attributes & TypeAttributes.Abstract) != 0)
            throw new NotSupportedException("Generic construction currently requires a concrete named class.");
        foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Methods)
        {
            var constructor = type.Module.GetMethod(handle);
            if (constructor.Name != ".ctor" || (constructor.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) continue;
            var declared = SignatureReader.Method(type.Module, constructor.Signature, type.Arguments);
            if (declared.HasThis && declared.Parameters.Length == 0 && declared.Result.Code == SignatureTypeCode.Void) return new(type, handle);
        }
        throw new NotSupportedException($"Generic construction requires a public parameterless constructor: {type}.");
    }
}
