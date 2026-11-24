using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The supported linear-memory Span/ReadOnlySpan surface used by inline-array lowering.</summary>
public sealed record SpanAccess(string Operation, TypeIdentity Owner, MethodSignature Signature)
{
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System", "Span`1") || FrameworkTypes.Is(type, "System", "ReadOnlySpan`1");

    public static SpanAccess? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind != HandleKind.TypeSpecification) return null;
        var owner = SignatureReader.Resolve(module, member.Parent, context, methodArguments).Identity;
        if (owner is not { } span || !Is(span) || span.Arguments?.Values.Length != 1) return null;
        var signature = SignatureReader.Method(module, member.Signature, span.Arguments);
        if (!signature.HasThis) return null;
        if (member.Name == "get_Length" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Int32)
            return new(member.Name, span, signature);
        if (member.Name == "get_Item" && signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.Int32 &&
            (signature.Result with { IsReadOnlyByReference = false }) == span.Arguments.Values[0].ByReference())
            return new(member.Name, span, signature);
        return null;
    }
}
