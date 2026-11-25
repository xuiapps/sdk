using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitUnbox_any(Instruction.Token instruction, int offset) => EmitUnboxing(instruction, offset, address: false);
}
