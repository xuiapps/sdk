namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>Bulk-memory subopcodes following the 0xfc prefix.</summary>
public enum BulkMemoryOpCode : uint
{
    /// <summary>Copies bytes, including overlapping ranges.</summary>
    MemoryCopy = 10,
    /// <summary>Fills a range with a byte.</summary>
    MemoryFill = 11
}
