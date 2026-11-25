using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Framework elapsed clocks supplied by the host's monotonic clock.</summary>
public sealed record MonotonicClock(string Operation)
{
    /// <summary>Recognizes exact static Environment.TickCount64 and Stopwatch.GetTimestamp signatures.</summary>
    public static MonotonicClock? Read(Module module, EntityHandle token)
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
        if (!(name == "get_TickCount64" && FrameworkTypes.Is(owner, "System", "Environment") ||
            name == "GetTimestamp" && FrameworkTypes.Is(owner, "System.Diagnostics", "Stopwatch"))) return null;
        var signature = SignatureReader.Method(module, blob);
        return !signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(SignatureTypeCode.Int64) ? new(name) : null;
    }
}
