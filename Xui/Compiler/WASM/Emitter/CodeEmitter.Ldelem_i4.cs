using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_i4(int offset) => EmitArrayLoad(OpCode.Ldelem_i4, offset);
}
