namespace Xui.Compiler.WASM.Wasm.Section;

/// <summary>Controls whether a global, struct field, or array element can be modified.</summary>
public enum Mutability : byte
{
    /// <summary>The stored value cannot be reassigned.</summary>
    Immutable = 0,

    /// <summary>The stored value can be reassigned.</summary>
    Mutable = 1
}
