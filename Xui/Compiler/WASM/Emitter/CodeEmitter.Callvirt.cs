using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCallvirt(Instruction.Token instruction, int offset) => EmitCall(instruction, offset);
}
