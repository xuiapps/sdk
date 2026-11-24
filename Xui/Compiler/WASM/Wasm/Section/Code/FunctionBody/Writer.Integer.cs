using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Divides unsigned i64 operands; the divisor must be nonzero.</summary>
    public readonly Writer I64DivU() { body.Instruction(OpCode.I64DivU); return this; }
    /// <summary>Takes the unsigned i64 remainder; the divisor must be nonzero.</summary>
    public readonly Writer I64RemU() { body.Instruction(OpCode.I64RemU); return this; }
}
