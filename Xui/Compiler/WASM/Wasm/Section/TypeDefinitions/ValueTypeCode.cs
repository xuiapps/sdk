namespace Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

/// <summary>Binary codes for numeric, vector, and reference value types.</summary>
/// <remarks>
/// These types can appear in parameter and result lists, locals, globals, and fields.
/// A void result is represented by an empty result list, not a value type code.
/// Explicit reference codes are followed by a signed heap type encoding.
/// See <see href="https://webassembly.github.io/spec/core/binary/types.html#value-types"/>.
/// </remarks>
public enum ValueTypeCode : byte
{
    /// <summary>A 32-bit integer.</summary>
    I32 = 0x7f,

    /// <summary>A nullable GC reference with identity comparison.</summary>
    EqRef = 0x6d,

    /// <summary>A 64-bit integer.</summary>
    I64 = 0x7e,

    /// <summary>A 32-bit floating-point value.</summary>
    F32 = 0x7d,

    /// <summary>A 64-bit floating-point value.</summary>
    F64 = 0x7c,

    /// <summary>A 128-bit SIMD vector.</summary>
    V128 = 0x7b,

    /// <summary>A nullable function reference.</summary>
    FuncRef = 0x70,

    /// <summary>A nullable host reference, used here for native JavaScript strings.</summary>
    ExternRef = 0x6f,

    /// <summary>A nullable reference followed by its heap type.</summary>
    RefNull = 0x63,

    /// <summary>A non-null reference followed by its heap type.</summary>
    Ref = 0x64
}
