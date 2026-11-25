using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitInitobj(Instruction.Token instruction, int offset)
    {
        if (TryScalarObject(instruction, offset)) return;
        if (TryReferenceObject(instruction, offset)) return;
        EmitManagedStructObject(instruction, offset);
    }
}
