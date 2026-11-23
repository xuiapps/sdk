using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Resolves members across the supplied assemblies while retaining constructed declaring-type identity.</summary>
public static class MemberResolver
{
    /// <summary>Resolves a MethodDef or MemberRef using the containing type's generic context.</summary>
    public static MethodIdentity Method(Module module, EntityHandle handle, TypeIdentity context)
    {
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var definition = module.GetMethod((MethodDefinitionHandle)handle);
            return new(Owner(module, definition.DeclaringType, context), (MethodDefinitionHandle)handle);
        }
        if (handle.Kind != HandleKind.MemberReference) throw new NotSupportedException($"Method token {handle.Kind} is not implemented.");
        var reference = module.GetMemberReference((MemberReferenceHandle)handle);
        if (reference.Kind != MemberReferenceKind.Method) throw new BadImageFormatException("Expected a method reference.");
        var owner = Parent(module, reference.Parent, context);
        // Match templates before substitution: M(T) and M(int) remain distinct when T is int.
        foreach (var candidate in owner.Module.GetType((TypeDefinitionHandle)owner.Handle).Methods)
        {
            var method = owner.Module.GetMethod(candidate);
            if (method.Name == reference.Name && Equal(SignatureReader.Method(owner.Module, method.Signature), SignatureReader.Method(module, reference.Signature))) return new(owner, candidate);
        }
        throw new NotSupportedException($"Cannot resolve method reference {owner}.{reference.Name}.");
    }

    /// <summary>Resolves a FieldDef or MemberRef using the containing type's generic context.</summary>
    public static FieldIdentity Field(Module module, EntityHandle handle, TypeIdentity context)
    {
        if (handle.Kind == HandleKind.FieldDefinition)
        {
            var definition = module.GetField((FieldDefinitionHandle)handle);
            return new(Owner(module, definition.DeclaringType, context), (FieldDefinitionHandle)handle);
        }
        if (handle.Kind != HandleKind.MemberReference) throw new NotSupportedException($"Field token {handle.Kind} is not implemented.");
        var reference = module.GetMemberReference((MemberReferenceHandle)handle);
        if (reference.Kind != MemberReferenceKind.Field) throw new BadImageFormatException("Expected a field reference.");
        var owner = Parent(module, reference.Parent, context);
        foreach (var candidate in owner.Module.GetType((TypeDefinitionHandle)owner.Handle).Fields)
        {
            var field = owner.Module.GetField(candidate);
            if (field.Name == reference.Name && SignatureReader.Field(owner.Module, field.Signature) == SignatureReader.Field(module, reference.Signature)) return new(owner, candidate);
        }
        throw new NotSupportedException($"Cannot resolve field reference {owner}.{reference.Name}.");
    }

    private static TypeIdentity Parent(Module module, EntityHandle parent, TypeIdentity context)
    {
        var type = SignatureReader.Resolve(module, parent, context.Arguments);
        if (type.Identity is not { Handle.Kind: HandleKind.TypeDefinition } identity)
            throw new NotSupportedException($"Cannot resolve member owner {type.Identity}; supply its implementation assembly to the compilation.");
        return identity;
    }

    private static TypeIdentity Owner(Module module, TypeDefinitionHandle declaringType, TypeIdentity context) =>
        context.Module == module && context.Handle == declaringType ? context : new(module, declaringType);

    private static bool Equal(MethodSignature a, MethodSignature b) =>
        a.HasThis == b.HasThis && a.Result == b.Result && a.Parameters.SequenceEqual(b.Parameters);
}
