using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Execution-stack queries implemented with the target runtime call-depth budget.</summary>
public static class ExecutionStack
{
    public static string? Read(Module module, EntityHandle token)
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
        if (name is not ("EnsureSufficientExecutionStack" or "TryEnsureSufficientExecutionStack") ||
            !FrameworkTypes.Is(owner, "System.Runtime.CompilerServices", "RuntimeHelpers")) return null;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(name == "EnsureSufficientExecutionStack" ? SignatureTypeCode.Void : SignatureTypeCode.Boolean)
            ? name : null;
    }
}
