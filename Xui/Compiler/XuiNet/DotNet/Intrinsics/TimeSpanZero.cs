using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The framework's immutable, all-zero TimeSpan constant; other static fields retain normal initialization requirements.</summary>
public static class TimeSpanZero
{
    public static TypeIdentity? Read(Module module, EntityHandle token, TypeIdentity context)
    {
        string name;
        TypeIdentity owner;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Field || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            name = member.Name; owner = module.ResolveType(member.Parent);
        }
        else if (token.Kind == HandleKind.FieldDefinition)
        {
            var field = module.GetField((FieldDefinitionHandle)token);
            name = field.Name; owner = new(module, field.DeclaringType);
        }
        else return null;
        if (name != "Zero" || !FrameworkTypes.Is(owner, "System", "TimeSpan")) return null;
        var resolved = MemberResolver.Field(module, token, context);
        var definition = resolved.DeclaringType.Module.GetField(resolved.Handle);
        var signature = Signatures.SignatureReader.Field(resolved.DeclaringType.Module, definition.Signature);
        if ((definition.Attributes & (FieldAttributes.Static | FieldAttributes.InitOnly)) != (FieldAttributes.Static | FieldAttributes.InitOnly) ||
            signature.Identity != owner || signature.IsByReference || signature.IsReferenceType)
            throw new NotSupportedException("Unsupported framework TimeSpan.Zero declaration.");
        return owner;
    }
}
