using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Monitor operations that have useful semantics in the single-agent execution model.</summary>
public sealed record MonitorMethod(
    string Operation,
    MethodSignature Signature)
{
    public bool HasLockTaken => Signature.Parameters.Length == 2;

    public static MonitorMethod? Read(Module module, EntityHandle token)
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
        if (!FrameworkTypes.Is(owner, "System.Threading", "Monitor") || name is not ("Enter" or "Exit" or "IsEntered")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Parameters.Length is < 1 or > 2 ||
            signature.Parameters[0] != new SignatureType(SignatureTypeCode.Object, IsReferenceType: true)) return null;
        if (signature.Parameters.Length == 2 && (name != "Enter" || signature.Parameters[1] != new SignatureType(SignatureTypeCode.Boolean, IsByReference: true))) return null;
        if (signature.Result != new SignatureType(name == "IsEntered" ? SignatureTypeCode.Boolean : SignatureTypeCode.Void)) return null;
        return new(name, signature);
    }
}
