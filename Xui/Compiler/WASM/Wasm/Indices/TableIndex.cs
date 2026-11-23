namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>An index in the module's reference-table space.</summary>
public readonly record struct TableIndex(uint Value);
