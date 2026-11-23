namespace Xui.Compiler.WASM.Wasm.Section.Memory;

/// <summary>Encoding of unshared WASM32 memory limits.</summary>
public enum LimitsKind : byte
{
    /// <summary>Only the initial page count is present.</summary>
    Minimum = 0,
    /// <summary>Initial and maximum page counts are present.</summary>
    MinimumAndMaximum = 1
}
