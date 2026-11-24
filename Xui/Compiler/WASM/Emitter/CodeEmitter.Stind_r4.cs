using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_r4(int offset) => EmitIndirect(OpCode.Stind_r4, offset, store: true);
}
