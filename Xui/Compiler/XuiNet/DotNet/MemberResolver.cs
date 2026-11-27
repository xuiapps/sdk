using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Resolves members across the supplied assemblies while retaining constructed declaring-type identity.</summary>
public static class MemberResolver
{
    /// <summary>Resolves MethodDef, MemberRef, or MethodSpec in the caller's type and method contexts.</summary>
    public static MethodIdentity Method(Module module, EntityHandle handle, TypeIdentity context, GenericArguments? methodArguments = null)
    {
        if (handle.Kind == HandleKind.MethodSpecification)
        {
            var spec = module.GetMethodSpecification((MethodSpecificationHandle)handle);
            var template = Method(module, spec.Method, context, methodArguments);
            var arguments = spec.Read(context.Arguments, methodArguments);
            if (template.DeclaringType.Module.GetMethod(template.Handle).GenericParameters.Count != arguments.Values.Length)
                throw new BadImageFormatException("Method argument count differs from its declaration.");
            return template with { Arguments = arguments };
        }
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var definition = module.GetMethod((MethodDefinitionHandle)handle);
            var declaredOwner = Owner(module, definition.DeclaringType, context);
            if (declaredOwner.Module == module && declaredOwner.Handle == definition.DeclaringType)
                return new(declaredOwner, (MethodDefinitionHandle)handle);
            return FindMethod(declaredOwner, definition.Name, SignatureReader.Method(module, definition.Signature));
        }
        if (handle.Kind != HandleKind.MemberReference) throw new NotSupportedException($"Method token {handle.Kind} is not implemented.");
        var reference = module.GetMemberReference((MemberReferenceHandle)handle);
        if (reference.Kind != MemberReferenceKind.Method) throw new BadImageFormatException("Expected a method reference.");
        var owner = Parent(module, reference.Parent, context, methodArguments);
        // Match templates before substitution: M(T) and M(int) remain distinct when T is int.
        return FindMethod(owner, reference.Name, SignatureReader.Method(module, reference.Signature));
    }

    private static MethodIdentity FindMethod(TypeIdentity owner, string name, MethodSignature signature)
    {
        foreach (var candidate in owner.Module.GetType((TypeDefinitionHandle)owner.Handle).Methods)
        {
            var method = owner.Module.GetMethod(candidate);
            if (method.Name == name && Equal(SignatureReader.Method(owner.Module, method.Signature), signature)) return new(owner, candidate);
        }
        throw new NotSupportedException($"Cannot resolve method reference {owner}.{name}.");
    }

    /// <summary>Resolves a FieldDef or MemberRef using the containing type's generic context.</summary>
    public static FieldIdentity Field(Module module, EntityHandle handle, TypeIdentity context, GenericArguments? methodArguments = null)
    {
        if (handle.Kind == HandleKind.FieldDefinition)
        {
            var definition = module.GetField((FieldDefinitionHandle)handle);
            var declaredOwner = Owner(module, definition.DeclaringType, context);
            if (declaredOwner.Module == module && declaredOwner.Handle == definition.DeclaringType)
                return new(declaredOwner, (FieldDefinitionHandle)handle);
            return FindField(declaredOwner, definition.Name, SignatureReader.Field(module, definition.Signature));
        }
        if (handle.Kind != HandleKind.MemberReference) throw new NotSupportedException($"Field token {handle.Kind} is not implemented.");
        var reference = module.GetMemberReference((MemberReferenceHandle)handle);
        if (reference.Kind != MemberReferenceKind.Field) throw new BadImageFormatException("Expected a field reference.");
        var owner = Parent(module, reference.Parent, context, methodArguments);
        return FindField(owner, reference.Name, SignatureReader.Field(module, reference.Signature));
    }

    private static FieldIdentity FindField(TypeIdentity owner, string name, SignatureType signature)
    {
        foreach (var candidate in owner.Module.GetType((TypeDefinitionHandle)owner.Handle).Fields)
        {
            var field = owner.Module.GetField(candidate);
            if (field.Name == name && Equal(SignatureReader.Field(owner.Module, field.Signature), signature)) return new(owner, candidate);
        }
        throw new NotSupportedException($"Cannot resolve field reference {owner}.{name}.");
    }

    private static TypeIdentity Parent(Module module, EntityHandle parent, TypeIdentity context, GenericArguments? methodArguments = null)
    {
        var type = SignatureReader.Resolve(module, parent, context.Arguments, methodArguments);
        if (type.Identity is not { Handle.Kind: HandleKind.TypeDefinition } identity)
            throw new NotSupportedException($"Cannot resolve member owner {type.Identity}; supply its implementation assembly to the compilation.");
        return identity;
    }

    private static TypeIdentity Owner(Module module, TypeDefinitionHandle declaringType, TypeIdentity context)
    {
        var definition = module.ResolveType(declaringType);
        return definition == context.GenericDefinition ? context : definition;
    }

    private static bool Equal(MethodSignature a, MethodSignature b) =>
        a.GenericParameterCount == b.GenericParameterCount && a.HasThis == b.HasThis && Equal(a.Result, b.Result) && a.Parameters.Length == b.Parameters.Length &&
        a.Parameters.Zip(b.Parameters).All(pair => Equal(pair.First, pair.Second));

    // The supported framework NFloat facades name the same intrinsic type across otherwise-unresolved assemblies.
    private static bool Equal(SignatureType a, SignatureType b) => a == b ||
        a.Code == b.Code && a.IsByReference == b.IsByReference && a.IsReferenceType == b.IsReferenceType &&
        a.IsReadOnlyByReference == b.IsReadOnlyByReference &&
        a.Identity is { } left && b.Identity is { } right && Intrinsics.NativeFloat.Is(left) && Intrinsics.NativeFloat.Is(right);
}
