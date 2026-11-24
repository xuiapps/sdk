namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>Table subopcodes following the miscellaneous instruction prefix.</summary>
public enum TableOpCode : uint
{
    /// <summary>Fills a range of table entries with the supplied reference.</summary>
    Fill = 17
}
