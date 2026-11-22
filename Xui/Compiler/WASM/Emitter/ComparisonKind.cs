namespace Xui.Compiler.WASM.Emitter;

/// <summary>The relation tested by an MSIL conditional branch.</summary>
internal enum ComparisonKind
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}
