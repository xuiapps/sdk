namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>GC instruction subopcodes currently emitted after the GC prefix.</summary>
public enum GcOpCode : uint
{
    /// <summary>Allocates an array filled with the supplied element value.</summary>
    ArrayNew = 0x06,
    /// <summary>Allocates a zero/null-filled array.</summary>
    ArrayNewDefault = 0x07,
    /// <summary>Reads an unpacked array element.</summary>
    ArrayGet = 0x0b,
    /// <summary>Writes an array element.</summary>
    ArraySet = 0x0e,
    /// <summary>Reads the array length.</summary>
    ArrayLen = 0x0f,

    /// <summary>Copies a range between GC arrays, preserving packed code units.</summary>
    ArrayCopy = 0x11,

    /// <summary>Tests a reference against a non-null heap type.</summary>
    RefTest = 0x14,

    /// <summary>Creates an array from a fixed number of stack values, including in constant expressions.</summary>
    ArrayNewFixed = 0x08,

    /// <summary>Allocates a struct from field values.</summary>
    StructNew = 0x00,
    /// <summary>Allocates a struct with zero/null fields.</summary>
    StructNewDefault = 0x01,
    /// <summary>Reads an unpacked struct field.</summary>
    StructGet = 0x02,
    /// <summary>Writes a mutable struct field.</summary>
    StructSet = 0x05,
    /// <summary>Casts to a non-null heap type, trapping on mismatch or null.</summary>
    RefCast = 0x16,

    /// <summary>Creates an array from a data segment; followed by type and data indices.</summary>
    ArrayNewData = 0x09,

    /// <summary>Externalizes an internal GC reference while preserving its identity.</summary>
    ExternConvertAny = 0x1b
}
