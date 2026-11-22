namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>Special block type encodings, separate from value types and type indices.</summary>
public enum BlockTypeCode : byte
{
    /// <summary>A block with no parameters and no results.</summary>
    Empty = 0x40
}
