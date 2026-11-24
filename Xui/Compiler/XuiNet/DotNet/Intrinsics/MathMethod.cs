using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A scalar framework math operation supplied by the small host runtime.</summary>
public sealed record MathMethod(
    string Operation,
    bool IsSingle)
{
    /// <summary>Recognizes checked Pow signatures, including the target-width NFloat operation.</summary>
    public static MathMethod? Read(Module module, EntityHandle token)
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
        if (name != "Pow") return null;
        bool native = NativeFloat.Is(owner);
        bool single = native || FrameworkTypes.Is(owner, "System", "MathF") || FrameworkTypes.Is(owner, "System", "Single");
        if (!single && !FrameworkTypes.Is(owner, "System", "Math") && !FrameworkTypes.Is(owner, "System", "Double")) return null;
        var signature = SignatureReader.Method(module, blob);
        bool Scalar(SignatureType type) => !type.IsByReference && (native
            ? type.Identity is { } id && NativeFloat.Is(id)
            : type.Code == (single ? SignatureTypeCode.Single : SignatureTypeCode.Double));
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 2 &&
            Scalar(signature.Result) && signature.Parameters.All(Scalar) ? new(name, single) : null;
    }
}
