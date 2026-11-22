namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>GC instruction subopcodes currently emitted after the GC prefix.</summary>
public enum GcOpCode : uint
{
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
    ArrayNewData = 0x09
}
