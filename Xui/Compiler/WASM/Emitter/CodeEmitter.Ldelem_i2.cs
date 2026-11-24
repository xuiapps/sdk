using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_i2(int offset) => EmitArrayLoad(OpCode.Ldelem_i2, offset);
}
