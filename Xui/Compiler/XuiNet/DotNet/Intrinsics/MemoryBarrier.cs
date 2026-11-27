using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Memory fences in the single-agent runtime; shared-memory threading is not supported.</summary>
public static class MemoryBarrier
{
    public static bool Is(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
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
        if (name != "MemoryBarrier" || !(FrameworkTypes.Is(owner, "System.Threading", "Thread") ||
            FrameworkTypes.Is(owner, "System.Threading", "Interlocked"))) return false;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(SignatureTypeCode.Void);
    }
}
