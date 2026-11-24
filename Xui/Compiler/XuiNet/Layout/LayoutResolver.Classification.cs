using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.Layout;

public sealed partial class LayoutResolver
{
    private readonly Dictionary<TypeIdentity, StorageKind> classifications = new();
    private readonly HashSet<TypeIdentity> classifying = [];

    /// <summary>Recursively classifies instance fields independently of the target's byte layout.</summary>
    /// <remarks>Static fields are ignored. Open generic and unresolved external value types throw rather than being
    /// assumed reference-free. A managed byref also requires tracking, even when it points to stack storage.</remarks>
    public StorageKind Classify(TypeDefinitionHandle handle) => Classify(new TypeIdentity(module, handle));

    /// <summary>Classifies a closed specialization, including nested generic value fields.</summary>
    public StorageKind Classify(TypeIdentity identity)
    {
        if (identity.Module != module) return For(identity.Module).Classify(identity);
        var handle = (TypeDefinitionHandle)identity.Handle;
        if (classifications.TryGetValue(identity, out var found)) return found;
        if (!classifying.Add(identity)) throw new BadImageFormatException("Recursive value-type field classification.");
        try
        {
            var type = module.GetType(handle);
            if (!IsCoreType(type.BaseType, "System", "ValueType") && !IsCoreType(type.BaseType, "System", "Enum"))
                throw new NotSupportedException($"Only value types can be classified: {type.Name}.");
            if (module.Metadata.GetTypeDefinition(handle).GetGenericParameters().Count != (identity.Arguments?.Values.Length ?? 0) || identity.Arguments?.ContainsGenericParameters == true)
                throw new NotSupportedException($"Generic classification requires a closed specialization: {type.Name}.");
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
                    if (ClassifyField(ref signature, identity.Arguments) == StorageKind.ContainsManagedReferences)
                        result = StorageKind.ContainsManagedReferences;
                }
                catch (NotSupportedException error)
                {
                    throw new NotSupportedException($"Cannot classify {type.Name}.{field.Name}: {error.Message}", error);
                }
            }
            classifications.Add(identity, result);
            return result;
        }
        finally { classifying.Remove(identity); }
    }

    // The outer field encoding is sufficient for arrays, references and unmanaged pointers.
    // Their element/pointee is not embedded storage and must not be traversed as a nested struct.
    private StorageKind ClassifyField(ref BlobReader reader, GenericArguments? arguments = null)
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
                return ClassifyField(ref reader, arguments);
            case SignatureTypeCode.TypeHandle:
                var kindReader = raw;
                if (kindReader.ReadByte() == (byte)SignatureTypeKind.Class) return StorageKind.ContainsManagedReferences;
                goto case SignatureTypeCode.GenericTypeInstance;
            case SignatureTypeCode.GenericTypeParameter:
            case SignatureTypeCode.GenericTypeInstance:
                var signature = raw;
                var declared = SignatureReader.Type(module, ref signature, arguments);
                reader = signature;
                if (declared.IsReferenceType || declared.IsByReference || declared.Code is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray or SignatureTypeCode.Array)
                    return StorageKind.ContainsManagedReferences;
                if (declared.Identity is not { } resolved) return StorageKind.LinearMemory;
                if (DotNet.Intrinsics.NativeFloat.Is(resolved)) return StorageKind.LinearMemory;
                if (resolved.Handle.Kind == HandleKind.TypeDefinition) return Classify(resolved);
                throw new NotSupportedException("External value-type classification is not resolved.");
            default:
                throw new NotSupportedException($"Field classification for {code} is not supported.");
        }
    }
}
