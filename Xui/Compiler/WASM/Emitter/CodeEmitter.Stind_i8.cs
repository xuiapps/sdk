using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_i8(int offset) => EmitIndirect(OpCode.Stind_i8, offset, store: true);
}
