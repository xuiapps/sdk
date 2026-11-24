using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStobj(Instruction.Token instruction, int offset)
    {
        if (TryReferenceObject(instruction, offset)) return;
        if (TryGcObject(instruction, offset)) return;
        var identity = ObjectType(instruction.Value);
        PopAssignable(new(System.Reflection.Metadata.SignatureTypeCode.TypeHandle, identity), instruction.Code, offset);
        PopOwner(identity, true, instruction.Code, offset);
        CopyStruct(identity);
    }
}
