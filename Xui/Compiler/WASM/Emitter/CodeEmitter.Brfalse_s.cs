using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBrfalse_s(int offset) => EmitBranchTruth(OpCode.Brfalse_s, offset, false);
}
