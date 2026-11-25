using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Emits I32DivS; callers implement managed failure checks.</summary>
    public readonly Writer I32DivS() { body.Instruction(OpCode.I32DivS); return this; }
    /// <summary>Emits I32DivU; callers implement managed failure checks.</summary>
    public readonly Writer I32DivU() { body.Instruction(OpCode.I32DivU); return this; }
    /// <summary>Emits I32RemS; callers implement managed failure checks.</summary>
    public readonly Writer I32RemS() { body.Instruction(OpCode.I32RemS); return this; }
    /// <summary>Emits I32RemU; callers implement managed failure checks.</summary>
    public readonly Writer I32RemU() { body.Instruction(OpCode.I32RemU); return this; }
    /// <summary>Emits I64DivS; callers implement managed failure checks.</summary>
    public readonly Writer I64DivS() { body.Instruction(OpCode.I64DivS); return this; }
    /// <summary>Emits I64RemS; callers implement managed failure checks.</summary>
    public readonly Writer I64RemS() { body.Instruction(OpCode.I64RemS); return this; }
    /// <summary>Divides unsigned i64 operands; the divisor must be nonzero.</summary>
    public readonly Writer I64DivU() { body.Instruction(OpCode.I64DivU); return this; }
    /// <summary>Takes the unsigned i64 remainder; the divisor must be nonzero.</summary>
    public readonly Writer I64RemU() { body.Instruction(OpCode.I64RemU); return this; }
}
