using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStelem_i1(int offset) => EmitArrayStore(OpCode.Stelem_i1, offset);
}
