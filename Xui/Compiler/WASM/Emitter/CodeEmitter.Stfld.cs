using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStfld(Instruction.Token instruction, int offset)
    {
        if (TryManagedField(instruction, offset)) return;
        if (TryGcField(instruction, offset)) return;
        var field = Field(instruction.Value);
        PopAssignable(field.Type, instruction.Code, offset);
        PopOwner(field.Owner, true, instruction.Code, offset);
        writer.LocalSet(FieldScratch(field.Type)).I32Const(field.Offset).I32Add();
        StoreField(field.Type);
    }
}
