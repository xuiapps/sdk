namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>Identifies a type definition within a WebAssembly module.</summary>
public readonly struct TypeIndex(uint value)
{
    /// <summary>The zero-based index encoded in the module.</summary>
    public readonly uint Value = value;
}
