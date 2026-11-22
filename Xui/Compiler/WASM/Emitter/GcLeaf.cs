namespace Xui.Compiler.WASM.Emitter;

/// <summary>A flattened field and its byte offset when the enclosing value also has linear storage.</summary>
internal readonly record struct GcLeaf(
    LocalVariable Storage,
    int MemoryOffset);
