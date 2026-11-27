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
        if (member.Name=="ToString" && signature.HasThis && signature.Parameters.Length==0 &&
            signature.Result==new SignatureType(SignatureTypeCode.String) && span.Arguments.Values[0]==new SignatureType(SignatureTypeCode.Char))
            return new("ToString",span,signature);
        bool SameElement(SignatureType type, string name) => !type.IsByReference && type.Identity is { } id &&
            FrameworkTypes.Is(id, "System", name) && id.Arguments?.Values.Length == 1 && id.Arguments.Values[0] == span.Arguments.Values[0];
        var array = SignatureType.Vector(span.Arguments.Values[0]);
        if (member.Name == ".ctor" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Void &&
            signature.Parameters.Length == 2 && signature.Parameters[0].Code == SignatureTypeCode.Pointer &&
            signature.Parameters[0].Pointer?.Element == new SignatureType(SignatureTypeCode.Void) &&
            signature.Parameters[1] == new SignatureType(SignatureTypeCode.Int32))
            return new("FromPointer", span, signature);
        if (member.Name == ".ctor" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Void &&
            signature.Parameters.Length == 1 &&
            (signature.Parameters[0] with { IsReadOnlyByReference = false }) == span.Arguments.Values[0].ByReference())
            return new("FromReference", span, signature);
        if ((member.Name == ".ctor" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Void ||
             member.Name == "op_Implicit" && !signature.HasThis && signature.Result.Identity == span) &&
            signature.Parameters.Length is 1 or 3 && signature.Parameters[0] == array &&
            signature.Parameters.Skip(1).All(p => p == new SignatureType(SignatureTypeCode.Int32)))
            return new("FromArray", span, signature);
        if (!signature.HasThis)
            return member.Name == "op_Implicit" && FrameworkTypes.Is(span, "System", "Span`1") &&
                signature.Parameters.Length == 1 && signature.Parameters[0].Identity == span &&
                SameElement(signature.Result, "ReadOnlySpan`1") ? new(member.Name, span, signature) : null;
        if (member.Name == "ToArray" && signature.Parameters.Length == 0 && signature.Result == array)
            return new(member.Name, span, signature);
        if (FrameworkTypes.Is(span, "System", "Span`1") && signature.Result.Code == SignatureTypeCode.Void &&
            (member.Name == "Fill" && signature.Parameters.Length == 1 && signature.Parameters[0] == span.Arguments.Values[0] ||
             member.Name == "Clear" && signature.Parameters.Length == 0))
            return new(member.Name, span, signature);
        if (member.Name == "get_IsEmpty" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Boolean)
            return new(member.Name, span, signature);
        if (member.Name == "Slice" && signature.Parameters.Length is 1 or 2 &&
            signature.Parameters.All(p => p == new SignatureType(SignatureTypeCode.Int32)) && signature.Result.Identity == span && !signature.Result.IsByReference)
            return new(member.Name, span, signature);
        if (member.Name is "CopyTo" or "TryCopyTo" && signature.Parameters.Length == 1 && SameElement(signature.Parameters[0], "Span`1") &&
            signature.Result == new SignatureType(member.Name == "CopyTo" ? SignatureTypeCode.Void : SignatureTypeCode.Boolean))
            return new(member.Name, span, signature);
        if (member.Name == "get_Length" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Int32)
            return new(member.Name, span, signature);
        if (member.Name == "get_Item" && signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.Int32 &&
            (signature.Result with { IsReadOnlyByReference = false }) == span.Arguments.Values[0].ByReference())
            return new(member.Name, span, signature);
        return null;
    }
}
