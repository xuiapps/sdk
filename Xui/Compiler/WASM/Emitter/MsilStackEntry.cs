using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Compile-time type information for one value on the MSIL evaluation stack.</summary>
/// <remarks>
/// Source MSIL types, not runtime values or the temporary WASM values emitted within an opcode.
/// Int32 also represents Boolean stack values; Double represents the floating-point category F,
/// emitted as f64 until a call requires f32. Int64 represents both signed and unsigned 64-bit values;
/// the opcode determines the signedness of an operation. IntPtr represents native integers
/// using the WASM target width of 32 bits.
/// Named value types retain their module-qualified identity; managed addresses retain their element type.
/// Storage may be linear memory or a GC object; class values retain reference identity.
/// </remarks>
public readonly struct MsilStackEntry
{
    /// <summary>Creates a stack entry for a supported built-in type.</summary>
    public MsilStackEntry(SignatureTypeCode typeCode, TypeIdentity? identity = null, bool isByReference = false, ArraySignature? array = null)
    {
        TypeCode = typeCode;
        Array = array;
        Identity = identity;
        IsByReference = isByReference;
    }

    /// <summary>The exact managed array element signature, when present.</summary>
    public ArraySignature? Array { get; }

    internal bool IsReferenceAddress => IsByReference && (TypeCode is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray || Identity is { IsValueType: false });

    internal bool IsNull { get; init; }
    internal GcLocation? Location { get; init; }

    /// <summary>The module-qualified identity of a named value type, when present.</summary>
    public TypeIdentity? Identity { get; }
    /// <summary>Whether this entry denotes a managed address of the declared type.</summary>
    public bool IsByReference { get; }

    /// <summary>The value's type code, such as String or Int32.</summary>
    public SignatureTypeCode TypeCode { get; }
}
