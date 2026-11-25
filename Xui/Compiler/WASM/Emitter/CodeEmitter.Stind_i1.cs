using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_i1(int offset) => EmitIndirect(OpCode.Stind_i1, offset, store: true);
}
