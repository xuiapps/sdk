using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>The supported operations on compiler-owned runtime type descriptors.</summary>
public enum ManagedTypeOperation
{
    GetType,
    FromHandle,
    Name,
    Equal,
    NotEqual
}

/// <summary>A narrow System.Type facade; arbitrary reflection is not a runtime dependency.</summary>
public sealed record ManagedType(
    ManagedTypeOperation Operation,
    MethodSignature Signature)
{
    /// <summary>Identifies the opaque managed reference backed by a module's immutable type descriptor.</summary>
    public static bool Is(TypeIdentity identity) => FrameworkTypes.Is(identity, "System", "Type");

    /// <summary>Resolves a closed class/interface token for the supported typeof path.</summary>
    public static TypeIdentity Token(Module module, EntityHandle token, GenericArguments? typeArguments = null, GenericArguments? methodArguments = null)
    {
        if (token.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference or HandleKind.TypeSpecification))
            throw new NotSupportedException("Runtime field and method handles are not implemented.");
        var signature = SignatureReader.Resolve(module, token, typeArguments, methodArguments);
        if (signature.Identity is not { Handle.Kind: HandleKind.TypeDefinition, IsValueType: false } identity ||
            FrameworkTypes.IsRuntimeBase(identity) || Is(identity) || DefaultEquality.Is(identity) ||
            Exceptions.ExceptionTypes.Kind(identity) is not null)
            throw new NotSupportedException($"typeof currently requires a linked class or interface descriptor: {signature}.");
        return identity;
    }

    /// <summary>Recognizes GetType, GetTypeFromHandle, the inherited Name property, and typed equality operators.</summary>
    public static ManagedType? Read(Module module, EntityHandle token)
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
        if (!Is(owner) && !FrameworkTypes.Is(owner, "System", "Object") && !FrameworkTypes.Is(owner, "System.Reflection", "MemberInfo")) return null;
        var signature = SignatureReader.Method(module, blob);
        if (signature.GenericParameterCount != 0) return null;
        if (Is(owner) && name == "GetTypeFromHandle" && !signature.HasThis && signature.Parameters.Length == 1 &&
            !signature.Parameters[0].IsByReference && signature.Parameters[0].Identity is { } handleType && FrameworkTypes.Is(handleType, "System", "RuntimeTypeHandle") &&
            signature.Result.Identity is { } returnedType && Is(returnedType))
            return new(ManagedTypeOperation.FromHandle, signature);
        if (signature.HasThis && signature.Parameters.Length == 0)
        {
            if (name == "GetType" && FrameworkTypes.Is(owner, "System", "Object") && signature.Result.Identity is { } result && Is(result))
                return new(ManagedTypeOperation.GetType, signature);
            if (name == "get_Name" && (Is(owner) || FrameworkTypes.Is(owner, "System.Reflection", "MemberInfo")) && signature.Result.Code == SignatureTypeCode.String)
                return new(ManagedTypeOperation.Name, signature);
        }
        if (Is(owner) && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean && signature.Parameters.Length == 2 &&
            signature.Parameters.All(p => !p.IsByReference && p.Identity is { } type && Is(type)))
        {
            if (name == "op_Equality") return new(ManagedTypeOperation.Equal, signature);
            if (name == "op_Inequality") return new(ManagedTypeOperation.NotEqual, signature);
        }
        return null;
    }
}
