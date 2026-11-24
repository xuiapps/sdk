using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelema(Instruction.Token instruction, int offset) => EmitArrayLoad(instruction.Code, offset, ArrayElement(instruction.Value), address: true);
}
