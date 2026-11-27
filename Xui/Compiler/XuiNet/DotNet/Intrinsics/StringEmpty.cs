using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The runtime-initialized String.Empty field has no managed initializer body.</summary>
public static class StringEmpty
{
    public static bool Is(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Field || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return false;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.FieldDefinition)
        {
            var field = module.GetField((FieldDefinitionHandle)token);
            if ((field.Attributes & (System.Reflection.FieldAttributes.Static | System.Reflection.FieldAttributes.InitOnly)) !=
                (System.Reflection.FieldAttributes.Static | System.Reflection.FieldAttributes.InitOnly)) return false;
            owner = new(module, field.DeclaringType); name = field.Name; blob = field.Signature;
        }
        else return false;
        return FrameworkTypes.Is(owner, "System", "String") && name == "Empty" &&
            SignatureReader.Field(module, blob) == new SignatureType(SignatureTypeCode.String);
    }
}
