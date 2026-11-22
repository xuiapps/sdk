namespace Xui.Compiler.WASM.Wasm.Section;

/// <summary>Identifies a section in a WebAssembly module's binary encoding.</summary>
/// <remarks>
/// Standard sections are declared below in their required order of appearance.
/// Their numeric IDs are not ordering keys: Tag precedes Global, and DataCount
/// precedes Code. Custom sections may appear anywhere between sections and repeat;
/// each standard section may appear at most once and may be omitted.
/// Enum.GetValues returns numeric order, not this declaration order.
/// See <see href="https://webassembly.github.io/spec/core/binary/modules.html#binary-module"/>.
/// </remarks>
public enum SectionId : byte
{
    /// <summary>Custom metadata; may appear anywhere in the section sequence.</summary>
    Custom = 0,

    /// <summary>Type definitions, including function signatures.</summary>
    Type = 1,

    /// <summary>Definitions supplied by the host or other modules.</summary>
    Import = 2,

    /// <summary>Type indices for functions defined in the module.</summary>
    Function = 3,

    /// <summary>Reference table definitions.</summary>
    Table = 4,

    /// <summary>Linear memory definitions.</summary>
    Memory = 5,

    /// <summary>Exception tag definitions; appears before Global.</summary>
    Tag = 13,

    /// <summary>Global variable definitions and their initializers.</summary>
    Global = 6,

    /// <summary>Named definitions exposed outside the module.</summary>
    Export = 7,

    /// <summary>The function invoked automatically during instantiation.</summary>
    Start = 8,

    /// <summary>Reference segments for table initialization.</summary>
    Element = 9,

    /// <summary>The number of data segments; appears before Code.</summary>
    DataCount = 12,

    /// <summary>Function bodies containing local declarations and instructions.</summary>
    Code = 10,

    /// <summary>Byte segments for linear memory initialization.</summary>
    Data = 11
}
