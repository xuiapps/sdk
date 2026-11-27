using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Runtime-supplied rectangular array members and dimension queries on an exact array receiver.</summary>
public sealed record RectangularArray(string Operation, SignatureType? ArrayType, MethodSignature Signature)
{
    public static RectangularArray? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
    {
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method) return null;
        SignatureType? arrayType = null;
        if (member.Parent.Kind == HandleKind.TypeSpecification)
        {
            var type = SignatureReader.Resolve(module, member.Parent, context, methodArguments);
            if (type.Array is not { IsVector: false }) return null;
            arrayType = type;
        }
        else if (member.Parent.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference) ||
            !FrameworkTypes.Is(new(module, member.Parent), "System", "Array")) return null;
        // Array pseudo-method signatures use the caller's generic context, not their own method arity.
        var blob = member.Signature;
        var header = blob.ReadSignatureHeader();
        if (!header.IsInstance || header.IsGeneric || header.Kind != SignatureKind.Method) return null;
        int count = blob.ReadCompressedInteger();
        var result = SignatureReader.Type(module, ref blob, context, methodArguments);
        var parameters = System.Collections.Immutable.ImmutableArray.CreateBuilder<SignatureType>(count);
        for (int i = 0; i < count; i++) parameters.Add(SignatureReader.Type(module, ref blob, context, methodArguments));
        if (blob.RemainingBytes != 0) throw new BadImageFormatException("Trailing array member signature bytes.");
        var signature = new MethodSignature(parameters.MoveToImmutable(), result, true);
        if (arrayType is { Array: { } array } declared)
        {
            bool indices = signature.Parameters.Take(array.Rank).All(p => p == new SignatureType(SignatureTypeCode.Int32));
            bool supported = indices && member.Name switch {
                ".ctor" => signature.Parameters.Length == array.Rank && signature.Result.Code == SignatureTypeCode.Void,
                "Get" => signature.Parameters.Length == array.Rank && signature.Result == array.Element,
                "Set" => signature.Parameters.Length == array.Rank + 1 && signature.Parameters[^1] == array.Element && signature.Result.Code == SignatureTypeCode.Void,
                "Address" => signature.Parameters.Length == array.Rank && signature.Result == array.Element.ByReference(),
                _ => false
            };
            if (!supported) throw new NotSupportedException($"Rectangular array member is not implemented: {declared}.{member.Name}.");
            return new(member.Name, declared, signature);
        }
        if (signature.Result.Code != SignatureTypeCode.Int32) return null;
        if (member.Name is "get_Length" or "get_Rank" && signature.Parameters.Length == 0 ||
            member.Name is "GetLength" or "GetLowerBound" or "GetUpperBound" && signature.Parameters.SequenceEqual([new SignatureType(SignatureTypeCode.Int32)]))
            return new(member.Name, null, signature);
        return null;
    }
}
