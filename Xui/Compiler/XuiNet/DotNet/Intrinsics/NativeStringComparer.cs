using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Adapts CoreLib's optional dictionary string-hash wrapper to the native JS-string runtime.</summary>
/// <remarks>
/// Dictionary keeps its supplied comparer. Native string hashing does not use CoreLib's
/// NonRandomizedStringEqualityComparer representation, so its wrapper tests are always false.
/// Constructing those internal wrappers is unsupported, rather than mixing hash implementations.
/// </remarks>
public static class NativeStringComparer
{
    public static bool IsWrapper(TypeIdentity type) => FrameworkTypes.Is(type, "System.Collections.Generic", "NonRandomizedStringEqualityComparer");

    public static MethodSignature? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        if (name != "GetStringComparer" || !IsWrapper(owner)) return null;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 1 &&
            signature.Parameters[0].Code == SignatureTypeCode.Object && signature.Result.Identity is { } result &&
            DefaultEquality.IsInterface(result) && result.Arguments?.Values is [{ Code: SignatureTypeCode.String }]
            ? signature : null;
    }
}
