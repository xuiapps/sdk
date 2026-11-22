using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBrfalse(int offset) => EmitBranchTruth(OpCode.Brfalse, offset, false);
}
