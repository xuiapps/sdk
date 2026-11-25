using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_r4(int offset) => EmitArrayLoad(OpCode.Ldelem_r4, offset);
}
