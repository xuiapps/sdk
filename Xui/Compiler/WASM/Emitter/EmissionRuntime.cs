using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Module indices required by built-in strings and optional linear-memory frames.</summary>
/// <param name="Utf16ArrayType">The mutable packed i16 array used to materialize string data.</param>
/// <param name="FromCharCodeArrayFunction">The wasm:js-string import that creates a JavaScript string.</param>
/// <param name="StackPointer">The mutable frame stack pointer, when linear memory is used.</param>
/// <param name="ReferenceStackPointer">The next free frame-owned reference slot, when needed.</param>
/// <param name="ObjectReferences">GC-visible slots holding object/interface references.</param>
/// <param name="StringReferences">GC-visible slots holding native JavaScript strings.</param>
/// <param name="StringConcat">Optional JS string concatenation used for string concatenation and argument exception messages.</param>
/// <param name="StringEquals">Optional ordinal JS string equality for default string comparers.</param>
/// <param name="StringLength">Optional JS string length used for string concatenation and argument exception messages.</param>
public readonly record struct EmissionRuntime(
    TypeIndex Utf16ArrayType,
    FunctionIndex FromCharCodeArrayFunction,
    GlobalIndex? StackPointer = null,
    GlobalIndex? ReferenceStackPointer = null,
    TableIndex? ObjectReferences = null,
    TableIndex? StringReferences = null,
    FunctionIndex? StringConcat = null,
    FunctionIndex? StringLength = null,
    FunctionIndex? StringEquals = null);
