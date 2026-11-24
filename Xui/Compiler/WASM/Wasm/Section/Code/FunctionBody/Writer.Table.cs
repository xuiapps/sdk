using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Loads the indexed reference from a table.</summary>
    public readonly Writer TableGet(TableIndex table) { body.Instruction(OpCode.TableGet).U32(table.Value); return this; }
    /// <summary>Stores a reference at the supplied table index.</summary>
    public readonly Writer TableSet(TableIndex table) { body.Instruction(OpCode.TableSet).U32(table.Value); return this; }
    /// <summary>Fills a range: start index, reference, then entry count.</summary>
    public readonly Writer TableFill(TableIndex table) { body.Instruction(OpCode.Misc).U32((uint)TableOpCode.Fill).U32(table.Value); return this; }
}
