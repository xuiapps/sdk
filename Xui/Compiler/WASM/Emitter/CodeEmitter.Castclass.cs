using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCastclass(Instruction.Token instruction, int offset) => EmitTypeTest(instruction, offset, true);
}
