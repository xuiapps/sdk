namespace Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

/// <summary>Packed integer storage types for GC array elements and struct fields.</summary>
public enum PackedTypeCode : byte
{
    /// <summary>An eight-bit integer.</summary>
    I8 = 0x78,

    /// <summary>A sixteen-bit integer, used for UTF-16 code units.</summary>
    I16 = 0x77
}
