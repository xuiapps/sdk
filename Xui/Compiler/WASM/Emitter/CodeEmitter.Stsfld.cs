using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStsfld(Instruction.Token instruction, int offset) => EmitStaticField(instruction, offset);
}
