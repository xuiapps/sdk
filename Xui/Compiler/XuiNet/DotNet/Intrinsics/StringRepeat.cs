using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Construction of a native string from a repeated UTF-16 code unit.</summary>
public static class StringRepeat
{
    /// <summary>Recognizes the trusted String(char, int) constructor.</summary>
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
        if (name != ".ctor" || !FrameworkTypes.Is(owner, "System", "String")) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.Void &&
            signature.Parameters.Length == 2 && signature.Parameters[0] == new SignatureType(SignatureTypeCode.Char) &&
            signature.Parameters[1] == new SignatureType(SignatureTypeCode.Int32);
    }
}
