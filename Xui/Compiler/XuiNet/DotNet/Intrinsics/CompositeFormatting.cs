using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Composite formatting through the deterministic native-JavaScript-string runtime.</summary>
public sealed record CompositeFormatting(
    MethodSignature Signature,
    bool HasProvider,
    bool HasArray)
{
    /// <summary>Fixed object arguments or object[]; SR's fixed-arity overloads use the neutral-resource profile.</summary>
    public static CompositeFormatting? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
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
        bool resource = FrameworkTypes.Is(owner, "System", "SR");
        if (name != "Format" || !resource && !FrameworkTypes.Is(owner, "System", "String")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.String ||
            signature.Parameters.Any(p => p.IsByReference) || signature.Parameters.Length < 2) return null;
        bool provider = signature.Parameters[0].Identity is { } identity && FrameworkTypes.Is(identity, "System", "IFormatProvider");
        int first = provider ? 1 : 0;
        if (signature.Parameters[first].Code != SignatureTypeCode.String) return null;
        var arguments = signature.Parameters.Skip(first + 1).ToArray();
        bool array = arguments.Length == 1 && arguments[0].Array is { Element.Code: SignatureTypeCode.Object };
        // SR's array overload treats null specially; leave it to a later extension.
        if (array && resource) return null;
        return array || arguments.Length is >= 1 and <= 3 && arguments.All(p => p.Code == SignatureTypeCode.Object)
            ? new(signature, provider, array) : null;
    }
}
