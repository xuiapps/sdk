using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBrtrue(int offset) => EmitBranchTruth(OpCode.Brtrue, offset, true);
}
