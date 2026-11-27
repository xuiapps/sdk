using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Compiler-owned, single-threaded shared array pools. Custom pools remain unsupported.</summary>
public sealed record SharedArrayPool(
    string Operation,
    TypeIdentity Owner,
    SignatureType Element,
    MethodSignature Signature)
{
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Buffers", "ArrayPool`1");

    public static SharedArrayPool? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments = null)
    {
        if (token.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind != HandleKind.TypeSpecification) return null;
        var owner = SignatureReader.Resolve(module, member.Parent, context, methodArguments).Identity;
        if (owner is not { } pool || !Is(pool) || pool.Arguments?.Values.Length != 1) return null;
        var signature = SignatureReader.Method(module, member.Signature, pool.Arguments);
        var element = pool.Arguments.Values[0];
        bool supported = member.Name switch
        {
            "get_Shared" => !signature.HasThis && signature.Parameters.Length == 0 && signature.Result.Identity == pool,
            "Rent" => signature.HasThis && signature.Parameters is [{ Code: SignatureTypeCode.Int32 }] && signature.Result == SignatureType.Vector(element),
            "Return" => signature.HasThis && signature.Result.Code == SignatureTypeCode.Void && signature.Parameters.Length == 2 &&
                signature.Parameters[0] == SignatureType.Vector(element) && signature.Parameters[1].Code == SignatureTypeCode.Boolean,
            _ => false
        };
        if (!supported) throw new NotSupportedException($"Shared array pool member is not implemented: {pool}.{member.Name}.");
        return new(member.Name, pool, element, signature);
    }
}
