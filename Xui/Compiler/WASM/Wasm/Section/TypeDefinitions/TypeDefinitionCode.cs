namespace Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

/// <summary>Binary markers for composite type definitions and their wrappers in a type section.</summary>
/// <remarks>
/// Function, Struct, and Array introduce composite types. Subtype markers wrap a
/// composite type with supertype information; RecursiveGroup wraps a list of subtypes.
/// See <see href="https://webassembly.github.io/spec/core/binary/types.html"/>.
/// </remarks>
public enum TypeDefinitionCode : byte
{
    /// <summary>A recursive group containing a vector of subtype definitions.</summary>
    RecursiveGroup = 0x4e,

    /// <summary>A final subtype: a supertype-index vector followed by a composite type.</summary>
    FinalSubtype = 0x4f,

    /// <summary>A non-final subtype: a supertype-index vector followed by a composite type.</summary>
    Subtype = 0x50,

    /// <summary>A GC array type containing an element field type.</summary>
    Array = 0x5e,

    /// <summary>A GC struct type containing a vector of field types.</summary>
    Struct = 0x5f,

    /// <summary>A function signature containing parameter and result type vectors.</summary>
    Function = 0x60
}
