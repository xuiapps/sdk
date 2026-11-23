using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBrtrue_s(int offset) => EmitBranchTruth(OpCode.Brtrue_s, offset, true);
}
