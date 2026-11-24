using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_u2(int offset) => EmitArrayLoad(OpCode.Ldelem_u2, offset);
}
