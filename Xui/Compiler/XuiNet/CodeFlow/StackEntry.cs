using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A normalized MSIL stack category, with exact type information for structs and managed addresses.</summary>
public readonly record struct StackEntry(
    StackType Type,
    SignatureType? DeclaredType = null)
{
    /// <summary>Creates a primitive stack category.</summary>
    public static implicit operator StackEntry(StackType type) => new(type);

    /// <summary>Normalizes value loads while retaining exact struct and address identities.</summary>
    public static StackEntry From(SignatureType type)
    {
        // The readonly signature modifier restricts source use, not the address ABI.
        // Mutable addresses can be supplied to an in parameter; stack joins use the element type.
        if (type.IsByReference) return new(StackType.ManagedPointer, type with { IsReadOnlyByReference = false });
        if (type.Identity is { } identity && DotNet.Intrinsics.NativeFloat.Is(identity)) return StackType.Float;
        return type.Code switch
        {
            SignatureTypeCode.Object => new(StackType.ObjectReference, type),
            SignatureTypeCode.TypeHandle => new StackEntry(type.IsReferenceType ? StackType.ObjectReference : StackType.ValueType, type),
            SignatureTypeCode.Boolean or SignatureTypeCode.Char or SignatureTypeCode.SByte or SignatureTypeCode.Byte or
            SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 => StackType.Int32,
            SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 => StackType.Int64,
            SignatureTypeCode.Single or SignatureTypeCode.Double => StackType.Float,
            SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr => StackType.NativeInt,
            SignatureTypeCode.String => StackType.String,
            SignatureTypeCode.SZArray => new(StackType.ArrayReference, type),
            _ => throw new NotSupportedException($"Unsupported stack signature type {type}.")
        };
    }

    /// <summary>Includes concrete type names for value types and addresses in diagnostic snapshots.</summary>
    public override string ToString() => DeclaredType?.ToString() ?? Type.ToString();
}
