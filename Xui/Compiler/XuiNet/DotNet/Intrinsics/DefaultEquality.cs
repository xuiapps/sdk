using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Closed framework default equality comparers with an explicitly supported value representation.</summary>
public sealed partial record DefaultEquality(
    string Operation,
    TypeIdentity Owner,
    SignatureType Element,
    MethodSignature Signature,
    MethodIdentity? Implementation = null,
    bool UsesObjectEquality = false)
{
    /// <summary>Recognizes the trusted framework comparer definition.</summary>
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Collections.Generic", "EqualityComparer`1");

    /// <summary>Reads default equality and vector search operations without retaining BCL factory bodies.</summary>
    public static DefaultEquality? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        if (token.Kind == HandleKind.MethodSpecification) return ReadArraySearch(module, token, context, methodArguments);
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind != HandleKind.TypeSpecification) return null;
        var owner = SignatureReader.Resolve(module, member.Parent, context, methodArguments).Identity;
        if (owner is not { } comparer || !Is(comparer) || comparer.Arguments?.Values.Length != 1) return null;
        if (member.Name == ".ctor") throw new NotSupportedException("Custom EqualityComparer subclasses are not implemented.");
        var signature = SignatureReader.Method(module, member.Signature, comparer.Arguments);
        var element = comparer.Arguments.Values[0];
        if (member.Name == "GetHashCode" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.Length == 1 && signature.Parameters[0] == element)
        {
            return Create(member.Name, comparer, element, signature);
        }
        if (member.Name == "get_Default" && !signature.HasThis && signature.Parameters.Length == 0 && signature.Result.Identity == comparer ||
            member.Name == "Equals" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean &&
            signature.Parameters.Length == 2 && signature.Parameters.All(p => p == element) ||
            member.Name == "IndexOf" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.Length == 4 && signature.Parameters[0] == SignatureType.Vector(element) &&
            signature.Parameters[1] == element && signature.Parameters[2] == new SignatureType(SignatureTypeCode.Int32) &&
            signature.Parameters[3] == new SignatureType(SignatureTypeCode.Int32))
            return Create(member.Name, comparer, element, signature);
        throw new NotSupportedException($"Default comparer member is not implemented: {comparer}.{member.Name}.");
    }
    private static DefaultEquality Create(string operation, TypeIdentity owner, SignatureType element, MethodSignature signature)
    {
        if (operation == "GetHashCode")
        {
            if (!PrimitiveValue.Supports(element))
                throw new NotSupportedException($"Default hashing is not implemented for {element}.");
            return new(operation, owner, element, signature);
        }
        bool supported = PrimitiveValue.Supports(element);
        var implementation = supported ? null : EquatableMethod.Find(element);
        bool objectEquality = !supported && implementation is null && ObjectEquality.Supports(element);
        if (!supported && implementation is null && !objectEquality) throw new NotSupportedException($"Default equality is not implemented for {element}.");
        return new(operation, owner, element, signature, implementation, objectEquality);
    }
}
