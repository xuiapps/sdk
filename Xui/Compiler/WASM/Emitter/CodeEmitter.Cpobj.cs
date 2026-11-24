using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitCpobj(Instruction.Token instruction, int offset)
    {
        if (TryGcObject(instruction, offset)) return;
        var identity = ObjectType(instruction.Value);
        PopOwner(identity, true, instruction.Code, offset);
        PopOwner(identity, true, instruction.Code, offset);
        CopyStruct(identity);
    }
}
