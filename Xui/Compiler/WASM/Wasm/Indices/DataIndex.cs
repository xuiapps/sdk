namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>Identifies a data segment within a WebAssembly module.</summary>
public readonly struct DataIndex(uint value)
{
    /// <summary>The zero-based index encoded in the module.</summary>
    public readonly uint Value = value;
}
