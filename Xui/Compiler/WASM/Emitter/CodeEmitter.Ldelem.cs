using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem(Instruction.Token instruction, int offset) => EmitArrayLoad(instruction.Code, offset, ArrayElement(instruction.Value));
}
