using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_r8(int offset) => EmitArrayLoad(OpCode.Ldelem_r8, offset);
}
