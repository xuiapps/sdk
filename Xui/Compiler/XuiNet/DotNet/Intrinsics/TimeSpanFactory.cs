using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A framework duration factory whose scalar input is converted to signed 100ns ticks.</summary>
public sealed record TimeSpanFactory(
    string Name,
    TypeIdentity Owner,
    MethodSignature Signature,
    long TicksPerUnit)
{
    /// <summary>The invariant English message for NaN input.</summary>
    public const string NaNMessage = "TimeSpan does not accept floating point Not-a-Number values.";
    /// <summary>The invariant English message for an unrepresentable duration.</summary>
    public const string OverflowMessage = "TimeSpan overflowed because the duration is too long.";

    /// <summary>Recognizes single-value double/integral overloads; composite overloads retain normal compilation.</summary>
    public static TimeSpanFactory? Read(Module module, EntityHandle token)
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
        if (!FrameworkTypes.Is(owner, "System", "TimeSpan")) return null;
        long scale = name switch
        {
            "FromDays" => 864_000_000_000,
            "FromHours" => 36_000_000_000,
            "FromMinutes" => 600_000_000,
            "FromSeconds" => 10_000_000,
            "FromMilliseconds" => 10_000,
            "FromMicroseconds" => 10,
            _ => 0
        };
        if (scale == 0) return null;
        var signature = SignatureReader.Method(module, blob);
        owner = owner.Module.ResolveType(owner.Handle);
        return !signature.HasThis && signature.GenericParameterCount == 0 &&
            signature.Parameters.Length == 1 && signature.Parameters[0].Code is SignatureTypeCode.Double or SignatureTypeCode.Int32 or SignatureTypeCode.Int64 && !signature.Parameters[0].IsByReference &&
            signature.Result.Identity == owner && !signature.Result.IsByReference && !signature.Result.IsReferenceType
            ? new(name, owner, signature, scale) : null;
    }
}
