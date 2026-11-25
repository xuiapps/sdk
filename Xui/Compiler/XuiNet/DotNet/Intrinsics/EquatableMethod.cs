using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Resolves an unboxed value type's own IEquatable implementation without reflection or boxing.</summary>
public static class EquatableMethod
{
    /// <summary>Finds explicit or public typed equality; arbitrary ValueType.Equals remains unsupported.</summary>
    public static MethodIdentity? Find(SignatureType element)
    {
        if (element.Identity is not { IsValueType: true } type) return null;
        var contract = type.Interfaces().FirstOrDefault(i => FrameworkTypes.Is(i, "System", "IEquatable`1") &&
            i.Arguments?.Values.Length == 1 && i.Arguments.Values[0] == element);
        if (contract.Module is null) return null;
        var definition = type.Module.GetType((TypeDefinitionHandle)type.Handle);
        foreach (var handle in definition.MethodImplementations)
        {
            var mapping = type.Module.GetMethodImplementation(handle);
            var declaration = MemberResolver.Method(type.Module, mapping.Declaration, type);
            if (declaration.DeclaringType == contract && declaration.DeclaringType.Module.GetMethod(declaration.Handle).Name == "Equals")
                return MemberResolver.Method(type.Module, mapping.Body, type);
        }
        foreach (var handle in definition.Methods)
        {
            var method = type.Module.GetMethod(handle);
            if (method.Name != "Equals" || method.GenericParameters.Count != 0 ||
                (method.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) continue;
            var signature = SignatureReader.Method(type.Module, method.Signature, type.Arguments);
            if (signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean &&
                signature.Parameters.Length == 1 && signature.Parameters[0] == element) return new(type, handle);
        }
        return null;
    }
}
