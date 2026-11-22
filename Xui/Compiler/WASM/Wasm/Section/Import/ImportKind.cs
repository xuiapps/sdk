namespace Xui.Compiler.WASM.Wasm.Section.Import;

/// <summary>Identifies the kind of definition supplied by an import.</summary>
public enum ImportKind : byte
{
    /// <summary>A function described by a type index.</summary>
    Function = 0,

    /// <summary>A reference table.</summary>
    Table = 1,

    /// <summary>A linear memory.</summary>
    Memory = 2,

    /// <summary>A global value.</summary>
    Global = 3,

    /// <summary>An exception tag.</summary>
    Tag = 4
}
