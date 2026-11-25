namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>Non-trapping floating-to-integer conversion subopcodes following the miscellaneous prefix.</summary>
public enum SaturatingConversionOpCode : uint
{
    /// <summary>Truncates f32 to signed i32, saturating overflow and mapping NaN to zero.</summary>
    I32TruncSatF32S = 0,
    /// <summary>Truncates f32 to unsigned i32, saturating overflow and mapping NaN to zero.</summary>
    I32TruncSatF32U = 1,
    /// <summary>Truncates f64 to signed i32, saturating overflow and mapping NaN to zero.</summary>
    I32TruncSatF64S = 2,
    /// <summary>Truncates f64 to unsigned i32, saturating overflow and mapping NaN to zero.</summary>
    I32TruncSatF64U = 3,
    /// <summary>Truncates f32 to signed i64, saturating overflow and mapping NaN to zero.</summary>
    I64TruncSatF32S = 4,
    /// <summary>Truncates f32 to unsigned i64, saturating overflow and mapping NaN to zero.</summary>
    I64TruncSatF32U = 5,
    /// <summary>Truncates f64 to signed i64, saturating overflow and mapping NaN to zero.</summary>
    I64TruncSatF64S = 6,
    /// <summary>Truncates f64 to unsigned i64, saturating overflow and mapping NaN to zero.</summary>
    I64TruncSatF64U = 7
}
