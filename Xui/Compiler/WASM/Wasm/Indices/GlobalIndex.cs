namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>Identifies an imported or defined global within a WebAssembly module.</summary>
public readonly struct GlobalIndex(uint value)
{
    /// <summary>The zero-based index encoded in the module.</summary>
    public readonly uint Value = value;
}
