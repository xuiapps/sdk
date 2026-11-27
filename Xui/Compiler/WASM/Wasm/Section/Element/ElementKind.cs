namespace Xui.Compiler.WASM.Wasm.Section.Element;

/// <summary>The element kind used by legacy function-index segments.</summary>
public enum ElementKind : byte
{
    /// <summary>A sequence of function indices, implicitly typed as funcref.</summary>
    FunctionReference = 0
}
