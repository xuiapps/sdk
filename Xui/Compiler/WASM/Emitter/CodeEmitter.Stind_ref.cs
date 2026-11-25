using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_ref(int offset) => EmitIndirect(OpCode.Stind_ref, offset, store: true);
}
