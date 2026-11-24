using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.Reachability;

internal sealed partial class DispatchTable
{
    public Dictionary<(TypeIdentity Type, MethodIdentity Method), MethodIdentity> InterfaceImplementations { get; } = new();

    public MethodIdentity InterfaceImplementation(TypeIdentity type, MethodIdentity declaration)
    {
        if (InterfaceImplementations.TryGetValue((type, declaration), out var cached)) return cached;
        if (!type.IsAssignableTo(declaration.DeclaringType))
            throw new InvalidProgramException($"{type} does not implement {declaration.DeclaringType}.");
        var definition = type.Module.GetType((TypeDefinitionHandle)type.Handle);
        TypeIdentity? templateContract = null;
        foreach (var handle in definition.Interfaces)
        {
            var token = type.Module.GetInterfaceType(handle);
            var closed = SignatureReader.Resolve(type.Module, token, type.Arguments).Identity!.Value;
            var open = SignatureReader.Resolve(type.Module, token).Identity!.Value;
            templateContract ??= FindContract(closed, open, declaration.DeclaringType);
        }
        MethodIdentity? target = null;
        if (templateContract is { } contract)
        {
            foreach (var handle in definition.MethodImplementations)
            {
                var mapping = type.Module.GetMethodImplementation(handle);
                if (MemberResolver.Method(type.Module, mapping.Declaration, type) == declaration)
                    target = MemberResolver.Method(type.Module, mapping.Body, type);
            }
            TypeIdentity? current = type, template = type.GenericDefinition;
            while (target is null && current is { Handle.Kind: HandleKind.TypeDefinition } owner && template is { } openOwner)
            {
                foreach (var handle in owner.Module.GetType((TypeDefinitionHandle)owner.Handle).Methods)
                {
                    var method = owner.Module.GetMethod(handle);
                    if ((method.Attributes & (MethodAttributes.MemberAccessMask | MethodAttributes.Static)) != MethodAttributes.Public) continue;
                    if (InterfaceSignatureMatches(new(openOwner, handle), new(contract, declaration.Handle)))
                    {
                        target = new(owner, handle);
                        break;
                    }
                }
                current = owner.BaseType;
                template = openOwner.BaseType;
            }
        }
        if (target is null && type.BaseType is { } parent && parent.IsAssignableTo(declaration.DeclaringType))
            target = InterfaceImplementation(parent, declaration);
        if (target is not { } implementation)
            throw new NotSupportedException($"No class implementation of {declaration} on {type}; default interface methods are not implemented.");
        if ((implementation.DeclaringType.Module.GetMethod(implementation.Handle).Attributes & MethodAttributes.Virtual) != 0)
            implementation = Resolve(type)[(int)Slot(implementation)].Implementation;
        InterfaceImplementations.Add((type, declaration), implementation);
        return implementation;
    }

    // Follow the same metadata path in open and substituted contexts. Matching only the
    // closed signature would collapse F(T) and F(int) when T is specialized to int.
    private static TypeIdentity? FindContract(TypeIdentity closed, TypeIdentity open, TypeIdentity target)
    {
        if (closed == target) return open;
        if (closed.Handle.Kind != HandleKind.TypeDefinition) return null;
        foreach (var handle in closed.Module.GetType((TypeDefinitionHandle)closed.Handle).Interfaces)
        {
            var token = closed.Module.GetInterfaceType(handle);
            var result = FindContract(SignatureReader.Resolve(closed.Module, token, closed.Arguments).Identity!.Value,
                SignatureReader.Resolve(open.Module, token, open.Arguments).Identity!.Value, target);
            if (result is not null) return result;
        }
        return null;
    }

    private static bool InterfaceSignatureMatches(MethodIdentity candidate, MethodIdentity declaration)
    {
        var left = candidate.DeclaringType.Module.GetMethod(candidate.Handle);
        var right = declaration.DeclaringType.Module.GetMethod(declaration.Handle);
        if (left.Name != right.Name || left.GenericParameters.Count != right.GenericParameters.Count) return false;
        var x = SignatureReader.Method(candidate.DeclaringType.Module, left.Signature, candidate.DeclaringType.Arguments);
        var y = SignatureReader.Method(declaration.DeclaringType.Module, right.Signature, declaration.DeclaringType.Arguments);
        return x.HasThis == y.HasThis && x.Result == y.Result && x.Parameters.SequenceEqual(y.Parameters);
    }

    private static void ValidateInterface(TypeIdentity identity)
    {
        var definition = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        foreach (var parameter in definition.GenericParameters)
            if ((identity.Module.GetGenericParameter(parameter).Attributes & GenericParameterAttributes.VarianceMask) != 0)
                throw new NotSupportedException("Variant interface dispatch is not implemented.");
        foreach (var handle in definition.Methods)
        {
            var method = identity.Module.GetMethod(handle);
            if ((method.Attributes & MethodAttributes.Static) != 0 || (method.Attributes & MethodAttributes.Abstract) == 0)
                throw new NotSupportedException($"Static and default interface methods are not implemented: {identity}.{method.Name}.");
            if (method.GenericParameters.Count != 0)
                throw new NotSupportedException("Generic interface methods are not implemented.");
        }
    }
}
