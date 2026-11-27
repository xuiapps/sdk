using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Default hashing for named managed classes, retaining virtual GetHashCode overrides.</summary>
public static class ObjectHash
{
    /// <summary>Recognizes the root Object.GetHashCode signature, not hidden slots or typed value methods.</summary>
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
        if (name != "GetHashCode" || !FrameworkTypes.Is(owner, "System", "Object")) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(SignatureTypeCode.Int32);
    }

    public static bool Supports(SignatureType element) => !element.IsByReference &&
        element.Identity is { Handle.Kind: HandleKind.TypeDefinition, IsValueType: false, IsInterface: false } type &&
        !FrameworkTypes.IsRuntimeBase(type) && !DelegateInfo.Is(type);

    public static MethodIdentity? Implementation(IReadOnlyList<VirtualSlot> slots)
    {
        foreach (var slot in slots)
        {
            var declaration = slot.Declaration;
            var method = declaration.DeclaringType.Module.GetMethod(declaration.Handle);
            if (method.Name != "GetHashCode" || (method.Attributes & MethodAttributes.NewSlot) != 0) continue;
            var signature = SignatureReader.Method(declaration.DeclaringType.Module, method.Signature, declaration.DeclaringType.Arguments);
            if (signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 && signature.Parameters.Length == 0)
                return slot.Implementation;
        }
        return null;
    }
}
