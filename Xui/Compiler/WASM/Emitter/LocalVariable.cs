using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>A local's declared MSIL type and its WASM storage and evaluation-stack representations.</summary>
/// <remarks>Native integers use 32 bits on this WASM target. Unsupported signatures are rejected during layout.</remarks>
public readonly record struct LocalVariable(
    SignatureTypeCode TypeCode,
    TypeIdentity? Identity = null,
    bool IsByReference = false,
    bool UsesGc = false,
    ArraySignature? Array = null)
{
    internal bool IsReferenceAddress => IsByReference && (TypeCode is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray || Identity is { IsValueType: false });

    /// <summary>The source MSIL type pushed by a load of this local.</summary>
    public SignatureTypeCode StackType => Identity is not null ? SignatureTypeCode.TypeHandle : TypeCode switch
    {
        SignatureTypeCode.Boolean or SignatureTypeCode.Char or SignatureTypeCode.SByte or
        SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
        SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 => SignatureTypeCode.Int32,
        SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 => SignatureTypeCode.Int64,
        SignatureTypeCode.Single or SignatureTypeCode.Double => SignatureTypeCode.Double,
        SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr => SignatureTypeCode.IntPtr,
        SignatureTypeCode.String => SignatureTypeCode.String,
        SignatureTypeCode.Object => SignatureTypeCode.Object,
        SignatureTypeCode.SZArray => SignatureTypeCode.SZArray,
        _ => throw new NotSupportedException($"Unsupported local type: {TypeCode}.")
    };

    /// <summary>The local's storage type, before load promotion or after store narrowing.</summary>
    public ValueTypeCode WasmType => IsReferenceAddress || UsesGc ? ValueTypeCode.EqRef : IsByReference
        ? ValueTypeCode.I32
        : Identity is not null
        ? ValueTypeCode.I32
        : TypeCode switch
    {
        SignatureTypeCode.Single => ValueTypeCode.F32,
        _ => StackType switch
        {
            SignatureTypeCode.Int32 or SignatureTypeCode.IntPtr => ValueTypeCode.I32,
            SignatureTypeCode.Int64 => ValueTypeCode.I64,
            SignatureTypeCode.Double => ValueTypeCode.F64,
            SignatureTypeCode.String => ValueTypeCode.ExternRef,
            _ => throw new NotSupportedException($"Unsupported local type: {TypeCode}.")
        }
    };

    internal bool IsLinearValue => Identity is not null && !UsesGc && !IsByReference;

    internal bool NeedsNarrowing => TypeCode is SignatureTypeCode.Boolean or SignatureTypeCode.Char or
        SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or
        SignatureTypeCode.UInt16 or SignatureTypeCode.Single;

    internal static LocalVariable[] Read(Xui.Compiler.XuiNet.DotNet.Module module, BlobReader signature, StorageModel storage, GenericArguments? arguments = null, GenericArguments? methodArguments = null)
    {
        System.Collections.Immutable.ImmutableArray<SignatureType> declared;
        try { declared = SignatureReader.Locals(module, signature, arguments, methodArguments); }
        catch (NotSupportedException error)
        {
            throw new NotSupportedException($"Unsupported local type: {error.Message}", error);
        }
        var locals = new LocalVariable[declared.Length];
        for (int i = 0; i < declared.Length; i++)
        {
            var type = declared[i];
            if (type.IsByReference && !storage.Bind(type).IsReferenceAddress && (type.Identity is not { IsValueType: true } id || storage.IsGc(id)))
                throw new NotSupportedException("Managed-address locals require a stack-stored struct.");
            locals[i] = storage.Bind(type);
            _ = locals[i].WasmType;
        }
        return locals;
    }
}
