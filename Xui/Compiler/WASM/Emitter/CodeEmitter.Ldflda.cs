using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdflda(Instruction.Token instruction, int offset)
    {
        if (TryManagedField(instruction, offset)) return;
        if (TryGcField(instruction, offset)) return;
        var field = Field(instruction.Value);
        PopOwner(field.Owner, true, instruction.Code, offset);
        writer.I32Const(field.Offset).I32Add();
        if (StorageModel.IsReference(field.Type))
        {
            LinearReferenceAddress();
        }
        else LinearValueAddress();
        stack.Push(new(field.Type.TypeCode, field.Type.Identity, isByReference: true, array: field.Type.Array));
    }
}
