using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Object.ToString and its class overrides, independent of hidden new virtual slots.</summary>
public static class ObjectText
{
    public static bool Is(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
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
        if (name != "ToString" || !FrameworkTypes.Is(owner, "System", "Object")) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(SignatureTypeCode.String);
    }

    public static MethodIdentity? Implementation(IReadOnlyList<VirtualSlot> slots)
    {
        foreach (var slot in slots)
        {
            var declaration = slot.Declaration;
            var method = declaration.DeclaringType.Module.GetMethod(declaration.Handle);
            if (method.Name != "ToString" || (method.Attributes & MethodAttributes.NewSlot) != 0) continue;
            var signature = SignatureReader.Method(declaration.DeclaringType.Module, method.Signature, declaration.DeclaringType.Arguments);
            if (signature.HasThis && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String)
                return slot.Implementation;
        }
        return null;
    }

    /// <summary>The default runtime spelling, including nested declarations and closed generic arguments.</summary>
    public static string TypeName(TypeIdentity type)
    {
        string name = DeclarationName(type);
        return type.Arguments is null ? name : name + "[" + string.Join(",", type.Arguments.Values.Select(TypeName)) + "]";
    }

    public static string TypeName(SignatureType type) => type.Array is { } array ? TypeName(array.Element) +
        (array.IsVector ? "[]" : "[" + (array.Rank == 1 ? "*" : new string(',', array.Rank - 1)) + "]") :
        type.Identity is { } identity ? TypeName(identity) : "System." + type.Code;

    private static string DeclarationName(TypeIdentity type)
    {
        var definition = type.Module.GetType((TypeDefinitionHandle)type.Handle);
        if (!definition.DeclaringType.IsNil) return DeclarationName(new(type.Module, definition.DeclaringType)) + "+" + definition.Name;
        return definition.Namespace.Length == 0 ? definition.Name : definition.Namespace + "." + definition.Name;
    }
}
