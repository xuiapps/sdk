using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Ordinal string equality over the JavaScript UTF-16 string representation.</summary>
public sealed record StringEquality(
    string Operation,
    MethodSignature Signature,
    bool HasComparison = false)
{
    /// <summary>Recognizes string operators and typed Equals; comparison modes are validated at execution.</summary>
    public static StringEquality? Read(Module module, EntityHandle token)
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
        if (!FrameworkTypes.Is(owner, "System", "String")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.Boolean) return null;
        if (name == "Equals" && signature.Parameters.Length == (signature.HasThis ? 2 : 3) &&
            signature.Parameters.Take(signature.Parameters.Length - 1).All(p => p == new SignatureType(SignatureTypeCode.String)) &&
            !signature.Parameters[^1].IsByReference && signature.Parameters[^1].Identity is { } comparison &&
            FrameworkTypes.Is(comparison, "System", "StringComparison")) return new(name, signature, true);
        if (signature.Parameters.Any(p => p.Code != SignatureTypeCode.String || p.IsByReference)) return null;
        if (name == "Equals" && signature.Parameters.Length == (signature.HasThis ? 1 : 2) ||
            name is "op_Equality" or "op_Inequality" && !signature.HasThis && signature.Parameters.Length == 2)
            return new(name, signature);
        return null;
    }
}
