using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework vector-clear entry points with optional 32-bit index and length.</summary>
public sealed record ArrayClear(bool HasIndices)
{
    /// <summary>Recognizes Array.Clear(Array) and Array.Clear(Array, int, int).</summary>
    public static ArrayClear? Read(Module module, EntityHandle token)
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
        if (name != "Clear" || !FrameworkTypes.Is(owner, "System", "Array")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.Void ||
            signature.Parameters.Length is not (1 or 3)) return null;
        bool indices = signature.Parameters.Length == 3;
        for (int i = 0; i < signature.Parameters.Length; i++)
        {
            var p = signature.Parameters[i];
            if (p.IsByReference) return null;
            if (i == 0)
            {
                if (p.Identity is not { } type || !FrameworkTypes.Is(type, "System", "Array")) return null;
            }
            else if (p.Code != SignatureTypeCode.Int32) return null;
        }
        return new(indices);
    }
}
