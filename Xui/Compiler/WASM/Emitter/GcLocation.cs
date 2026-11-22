namespace Xui.Compiler.WASM.Emitter;

/// <summary>A borrowed field location: the runtime value is its owner reference; the field range is static.</summary>
internal readonly record struct GcLocation(
    GcShape Owner,
    int First);
