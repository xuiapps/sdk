using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Target clock constants matching the host's nanosecond-unit timestamp representation.</summary>
public sealed record StopwatchConstant(
    string Name,
    SignatureTypeCode Code)
{
    public const long Frequency = 1_000_000_000;
    public static StopwatchConstant? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Field || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.FieldDefinition)
        {
            var field = module.GetField((FieldDefinitionHandle)token);
            if ((field.Attributes & (System.Reflection.FieldAttributes.Static | System.Reflection.FieldAttributes.InitOnly)) !=
                (System.Reflection.FieldAttributes.Static | System.Reflection.FieldAttributes.InitOnly)) return null;
            owner = new(module, field.DeclaringType); name = field.Name; blob = field.Signature;
        }
        else return null;
        if (!FrameworkTypes.Is(owner, "System.Diagnostics", "Stopwatch")) return null;
        var code = name switch {
            "Frequency" => SignatureTypeCode.Int64,
            "IsHighResolution" => SignatureTypeCode.Boolean,
            "s_tickFrequency" => SignatureTypeCode.Double,
            _ => SignatureTypeCode.Void
        };
        return code != SignatureTypeCode.Void && SignatureReader.Field(module, blob) == new SignatureType(code) ? new(name, code) : null;
    }
}
