using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Framework array copies with element indices or primitive byte offsets.</summary>
public sealed record ArrayCopy(bool HasIndices, bool ByteOffsets = false)
{
    /// <summary>Recognizes Array.Copy's 32-bit overloads and Buffer.BlockCopy.</summary>
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
        bool bytes = name == "BlockCopy" && FrameworkTypes.Is(owner, "System", "Buffer");
        if (!bytes && (name != "Copy" || !FrameworkTypes.Is(owner, "System", "Array"))) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.Void ||
            signature.Parameters.Length is not (3 or 5)) return null;
        bool indices = signature.Parameters.Length == 5;
        if (bytes && !indices) return null;
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
        return new(indices, bytes);
    }
}
