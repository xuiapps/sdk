using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Allocates a GC array from the element value and length on the stack.</summary>
    public readonly Writer ArrayNew(TypeIndex type) { body.Instruction(GcOpCode.ArrayNew).U32(type.Value); return this; }
    /// <summary>Allocates a zero/null-filled GC array with the length on the stack.</summary>
    public readonly Writer ArrayNewDefault(TypeIndex type) { body.Instruction(GcOpCode.ArrayNewDefault).U32(type.Value); return this; }
    /// <summary>Reads an unpacked element at the index on the stack.</summary>
    public readonly Writer ArrayGet(TypeIndex type) { body.Instruction(GcOpCode.ArrayGet).U32(type.Value); return this; }
    /// <summary>Stores the top value at the array/index below it.</summary>
    public readonly Writer ArraySet(TypeIndex type) { body.Instruction(GcOpCode.ArraySet).U32(type.Value); return this; }
    /// <summary>Copies array elements: destination, destination index, source, source index, length.</summary>
    public readonly Writer ArrayCopy(TypeIndex destination, TypeIndex source)
    { body.Instruction(GcOpCode.ArrayCopy).U32(destination.Value).U32(source.Value); return this; }
    /// <summary>Reads the length of a GC array.</summary>
    public readonly Writer ArrayLen() { body.Instruction(GcOpCode.ArrayLen); return this; }
}
