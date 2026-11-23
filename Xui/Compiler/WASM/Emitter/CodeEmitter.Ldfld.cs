using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdfld(Instruction.Token instruction, int offset)
    {
        if (TryGcField(instruction, offset)) return;
        var field = Field(instruction.Value);
        PopOwner(field.Owner, false, instruction.Code, offset);
        writer.I32Const(field.Offset).I32Add();
        LoadField(field.Type, offset);
    }
}
