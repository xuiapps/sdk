namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>The four catch-clause encodings accepted by try_table.</summary>
public enum CatchKind : byte { Catch = 0, CatchRef = 1, CatchAll = 2, CatchAllRef = 3 }
