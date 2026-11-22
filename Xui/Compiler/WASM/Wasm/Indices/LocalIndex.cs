namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>Identifies a parameter or local within a WebAssembly function.</summary>
public readonly struct LocalIndex(uint value)
{
    /// <summary>The zero-based index encoded in the function.</summary>
    public readonly uint Value = value;
}
