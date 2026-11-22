namespace Xui.Compiler.WASM.Emitter;

/// <summary>The contiguous leaf range occupied by one declared field.</summary>
internal readonly record struct GcField(
    LocalVariable Storage,
    int First,
    int Count);
