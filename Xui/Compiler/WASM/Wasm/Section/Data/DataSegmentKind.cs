namespace Xui.Compiler.WASM.Wasm.Section.Data;

/// <summary>Identifies how a data segment is encoded and initialized.</summary>
public enum DataSegmentKind : uint
{
    /// <summary>An active segment targeting memory zero, followed by an offset expression.</summary>
    Active = 0,

    /// <summary>A passive segment available to instructions such as array.new_data.</summary>
    Passive = 1,

    /// <summary>An active segment with an explicit memory index and offset expression.</summary>
    ActiveWithMemoryIndex = 2
}
