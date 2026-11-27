using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The browser runtime's deferred managed-action boundary.</summary>
public sealed record HostScheduler(MethodSignature Signature, MethodIdentity Callback)
{
    public static HostScheduler? Read(Module module, EntityHandle token)
    {
        EntityHandle parent;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            parent = method.DeclaringType; name = method.Name; blob = method.Signature;
        }
        else if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method) return null;
            parent = member.Parent; name = member.Name; blob = member.Signature;
        }
        else return null;
        if (name != "Post" || parent.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference)) return null;
        var owner = module.ResolveType(parent);
        var type = owner.Module.GetType((TypeDefinitionHandle)owner.Handle);
        if (owner.Module.Metadata.GetString(owner.Module.Metadata.GetAssemblyDefinition().Name) != "Xui.Compiler.Runtime" || type.Namespace != "Xui.Compiler.Runtime.Tasks" || type.Name != "NativeScheduler") return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.Parameters.Length != 1 || signature.Result.Code != SignatureTypeCode.Void ||
            signature.Parameters[0].Identity is not { } action || !FrameworkTypes.Is(action, "System", "Action")) return null;
        var callback = type.Methods.Single(h => owner.Module.GetMethod(h).Name == "Invoke");
        return new(signature, new(owner, callback));
    }
}
