namespace Xui.Compiler.WASM.Wasm.Section.Export;

/// <summary>Identifies the index space of an exported definition.</summary>
public enum ExportKind : byte
{
    /// <summary>A function.</summary>
    Function = 0,
    /// <summary>A reference table.</summary>
    Table = 1,
    /// <summary>A linear memory.</summary>
    Memory = 2,
    /// <summary>A global variable.</summary>
    Global = 3,
    /// <summary>An exception tag.</summary>
    Tag = 4
}
