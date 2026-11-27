using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>String constructors that copy a managed character array or a slice of it.</summary>
public static class StringCharacters
{
    public static MethodSignature? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
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
        if (name != ".ctor" || !FrameworkTypes.Is(owner, "System", "String")) return null;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Result.Code == SignatureTypeCode.Void &&
            signature.Parameters.Length is 1 or 3 && signature.Parameters[0] == SignatureType.Vector(new(SignatureTypeCode.Char)) &&
            signature.Parameters.Skip(1).All(p => p == new SignatureType(SignatureTypeCode.Int32)) ? signature : null;
    }
}
