using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_i(int offset) => EmitIndirect(OpCode.Stind_i, offset, store: true);
}
