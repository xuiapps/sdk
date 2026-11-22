namespace Xui.Compiler.XuiNet.Layout;

/// <summary>Whether a value type's instance fields require managed-reference tracking.</summary>
/// <remarks>This classification does not select a WASM representation or guarantee that byte layout is supported.</remarks>
public enum StorageKind
{
    /// <summary>Contains no managed references; raw byte storage needs no GC reference tracking.</summary>
    LinearMemory,

    /// <summary>Contains object references or managed byrefs, directly or through nested value types.</summary>
    /// <remarks>Requires a representation that preserves references and their lifetimes; this does not imply boxing.</remarks>
    ContainsManagedReferences
}
