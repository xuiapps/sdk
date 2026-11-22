namespace Xui.Compiler.WASM.Wasm.Indices;

/// <summary>Identifies an enclosing WASM control label by relative nesting depth.</summary>
public readonly struct LabelIndex(uint value)
{
    /// <summary>Zero targets the innermost label; larger values count outwards.</summary>
    public readonly uint Value = value;
}
