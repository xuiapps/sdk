using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Calls the table entry on the stack after the function arguments.</summary>
    public readonly Writer CallIndirect(TypeIndex signature, TableIndex table = default)
    { body.Instruction(OpCode.CallIndirect).U32(signature.Value).U32(table.Value); return this; }

    /// <summary>Begins a loop with no parameters or results.</summary>
    public readonly Writer Loop()
    { body.Instruction(OpCode.Loop).Byte((byte)BlockTypeCode.Empty); return this; }
}
