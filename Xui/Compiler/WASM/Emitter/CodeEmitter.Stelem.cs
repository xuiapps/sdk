using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStelem(Instruction.Token instruction, int offset) => EmitArrayStore(instruction.Code, offset, ArrayElement(instruction.Value));
}
