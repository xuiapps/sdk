using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_r8(int offset) => EmitIndirect(OpCode.Stind_r8, offset, store: true);
}
