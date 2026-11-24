using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Closed framework default equality comparers with an explicitly supported value representation.</summary>
public sealed record DefaultEquality(
    string Operation,
    TypeIdentity Owner,
    SignatureType Element,
    MethodSignature Signature,
    MethodIdentity? Implementation = null)
{
    /// <summary>Recognizes the trusted framework comparer definition.</summary>
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Collections.Generic", "EqualityComparer`1");

    /// <summary>Reads the supported Default/Equals surface without retaining BCL factory bodies.</summary>
    public static DefaultEquality? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind != HandleKind.TypeSpecification) return null;
        var owner = SignatureReader.Resolve(module, member.Parent, context, methodArguments).Identity;
        if (owner is not { } comparer || !Is(comparer) || comparer.Arguments?.Values.Length != 1) return null;
        if (member.Name == ".ctor") throw new NotSupportedException("Custom EqualityComparer subclasses are not implemented.");
        var signature = SignatureReader.Method(module, member.Signature, comparer.Arguments);
        var element = comparer.Arguments.Values[0];
        var storage = EnumInfo.Storage(element);
        bool supported = !element.IsByReference && (storage.Code is SignatureTypeCode.String or SignatureTypeCode.Boolean or SignatureTypeCode.Char or
            SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
            SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or
            SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Single or SignatureTypeCode.Double ||
            element.Identity is { } native && NativeFloat.Is(native));
        var implementation = supported ? null : EquatableMethod.Find(element);
        if (!supported && implementation is null) throw new NotSupportedException($"Default equality is not implemented for {element}.");
        if (member.Name == "get_Default" && !signature.HasThis && signature.Parameters.Length == 0 && signature.Result.Identity == comparer ||
            member.Name == "Equals" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean &&
            signature.Parameters.Length == 2 && signature.Parameters.All(p => p == element))
            return new(member.Name, comparer, element, signature, implementation);
        throw new NotSupportedException($"Default comparer member is not implemented: {comparer}.{member.Name}.");
    }
}
