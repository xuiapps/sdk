using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Limits default field hashing to structs composed entirely of primitive value fields.</summary>
/// <remarks>Nested hash overrides, reference fields, pointers and inline arrays require their own semantics.</remarks>
public static class PrimitiveStructHash
{
    public static bool Supports(SignatureType element)
    {
        var path = new HashSet<TypeIdentity>();
        return Visit(element);

        bool Visit(SignatureType value)
        {
            if (value.IsByReference || value.IsReferenceType || value.Code == SignatureTypeCode.String) return false;
            if (PrimitiveValue.Supports(value)) return true;
            if (value.Identity is not { Handle.Kind: HandleKind.TypeDefinition, IsValueType: true } type ||
                InlineArrayInfo.Read(type) is not null || ValueHash.Find(value) is not null || !path.Add(type)) return false;
            try
            {
                foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Fields)
                {
                    var field = type.Module.GetField(handle);
                    if ((field.Attributes & FieldAttributes.Static) != 0) continue;
                    if (!Visit(SignatureReader.Field(type.Module, field.Signature, type.Arguments))) return false;
                }
                return true;
            }
            finally { path.Remove(type); }
        }
    }
}
