using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework vector-copy entry points with 32-bit indices and length.</summary>
public sealed record ArrayCopy(bool HasIndices)
{
    /// <summary>Recognizes Array.Copy(Array, Array, int) and its five-argument overload.</summary>
    public static ArrayCopy? Read(Module module, EntityHandle token)
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
        if (name != "Copy" || !FrameworkTypes.Is(owner, "System", "Array")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.Void ||
            signature.Parameters.Length is not (3 or 5)) return null;
        bool indices = signature.Parameters.Length == 5;
        for (int i = 0; i < signature.Parameters.Length; i++)
        {
            var p = signature.Parameters[i];
            if (p.IsByReference) return null;
            if (i == 0 || i == (indices ? 2 : 1))
            {
                if (p.Identity is not { } type || !FrameworkTypes.Is(type, "System", "Array")) return null;
            }
            else if (p.Code != SignatureTypeCode.Int32) return null;
        }
        return new(indices);
    }
}
