namespace Xui.Compiler.WASM.Wasm.Section.Element;

/// <summary>Supported function-index segment encodings.</summary>
public enum SegmentKind : uint
{
    /// <summary>Function indices, active in table zero.</summary>
    ActiveFunctions = 0,
    /// <summary>Declares functions available to ref.func without initializing a table.</summary>
    DeclarativeFunctions = 3
}
