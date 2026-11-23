using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Module indices required by built-in strings and optional linear-memory frames.</summary>
/// <param name="Utf16ArrayType">The mutable packed i16 array used to materialize string data.</param>
/// <param name="FromCharCodeArrayFunction">The wasm:js-string import that creates a JavaScript string.</param>
/// <param name="StackPointer">The mutable frame stack pointer, when linear memory is used.</param>
public readonly record struct EmissionRuntime(
    TypeIndex Utf16ArrayType,
    FunctionIndex FromCharCodeArrayFunction,
    GlobalIndex? StackPointer = null);
