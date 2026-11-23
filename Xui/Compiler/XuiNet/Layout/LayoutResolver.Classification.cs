using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.Layout;

public sealed partial class LayoutResolver
{
    private readonly Dictionary<TypeDefinitionHandle, StorageKind> classifications = new();
    private readonly HashSet<TypeDefinitionHandle> classifying = [];

    /// <summary>Recursively classifies instance fields independently of the target's byte layout.</summary>
    /// <remarks>Static fields are ignored. Generic and unresolved external value types throw rather than being
    /// assumed reference-free. A managed byref also requires tracking, even when it points to stack storage.</remarks>
    public StorageKind Classify(TypeDefinitionHandle handle)
    {
        if (classifications.TryGetValue(handle, out var found)) return found;
        if (!classifying.Add(handle)) throw new BadImageFormatException("Recursive value-type field classification.");
        try
        {
            var type = module.GetType(handle);
            if (!IsCoreType(type.BaseType, "System", "ValueType") && !IsCoreType(type.BaseType, "System", "Enum"))
                throw new NotSupportedException($"Only value types can be classified: {type.Name}.");
            if (module.Metadata.GetTypeDefinition(handle).GetGenericParameters().Count != 0)
                throw new NotSupportedException($"Generic classification is not supported: {type.Name}.");
            var result = StorageKind.LinearMemory;
            foreach (var fieldHandle in type.Fields)
            {
                var field = module.GetField(fieldHandle);
                if ((field.Attributes & FieldAttributes.Static) != 0) continue;
                var signature = field.Signature;
                if (signature.ReadSignatureHeader().Kind != SignatureKind.Field)
                    throw new BadImageFormatException($"Invalid field signature: {type.Name}.{field.Name}.");
                try
                {
                    if (ClassifyField(ref signature) == StorageKind.ContainsManagedReferences)
                        result = StorageKind.ContainsManagedReferences;
                }
                catch (NotSupportedException error)
                {
                    throw new NotSupportedException($"Cannot classify {type.Name}.{field.Name}: {error.Message}", error);
                }
            }
            classifications.Add(handle, result);
            return result;
        }
        finally { classifying.Remove(handle); }
    }

    // The outer field encoding is sufficient for arrays, references and unmanaged pointers.
    // Their element/pointee is not embedded storage and must not be traversed as a nested struct.
    private StorageKind ClassifyField(ref BlobReader reader)
    {
        var raw = reader;
        var code = reader.ReadSignatureTypeCode();
        switch (code)
        {
            case SignatureTypeCode.String:
            case SignatureTypeCode.Object:
            case SignatureTypeCode.SZArray:
            case SignatureTypeCode.Array:
            case SignatureTypeCode.ByReference:
                return StorageKind.ContainsManagedReferences;
            case SignatureTypeCode.Boolean:
            case SignatureTypeCode.Char:
            case SignatureTypeCode.SByte:
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Int16:
            case SignatureTypeCode.UInt16:
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32:
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64:
            case SignatureTypeCode.Single:
            case SignatureTypeCode.Double:
            case SignatureTypeCode.IntPtr:
            case SignatureTypeCode.UIntPtr:
            case SignatureTypeCode.Pointer:
            case SignatureTypeCode.FunctionPointer:
                return StorageKind.LinearMemory;
            case SignatureTypeCode.OptionalModifier:
            case SignatureTypeCode.RequiredModifier:
                _ = reader.ReadTypeHandle();
                return ClassifyField(ref reader);
            case SignatureTypeCode.TypeHandle:
                if (raw.ReadByte() == (byte)SignatureTypeKind.Class)
                    return StorageKind.ContainsManagedReferences;
                var handle = reader.ReadTypeHandle();
                if (handle.Kind == HandleKind.TypeDefinition) return Classify((TypeDefinitionHandle)handle);
                if (IsCoreType(handle, "System.Runtime.InteropServices", "NFloat")) return StorageKind.LinearMemory;
                throw new NotSupportedException("External value-type classification is not resolved.");
            default:
                throw new NotSupportedException($"Field classification for {code} is not supported.");
        }
    }
}
