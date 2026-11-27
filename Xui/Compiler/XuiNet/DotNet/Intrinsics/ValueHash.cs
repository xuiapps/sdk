using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A value type's declared GetHashCode override, called on an unboxed value copy.</summary>
public static class ValueHash
{
    public static MethodIdentity? Find(SignatureType element)
    {
        if (element.IsByReference || element.Identity is not { Handle.Kind: HandleKind.TypeDefinition, IsValueType: true } type) return null;
        foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Methods)
        {
            var method = type.Module.GetMethod(handle);
            if (method.Name != "GetHashCode" || method.GenericParameters.Count != 0 ||
                (method.Attributes & (MethodAttributes.Virtual | MethodAttributes.NewSlot)) != MethodAttributes.Virtual) continue;
            var signature = SignatureReader.Method(type.Module, method.Signature, type.Arguments);
            if (signature.HasThis && signature.Parameters.Length == 0 && signature.Result == new SignatureType(SignatureTypeCode.Int32))
                return new(type, handle);
        }
        return null;
    }
}
