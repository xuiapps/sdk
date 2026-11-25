using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStind_i4(int offset) => EmitIndirect(OpCode.Stind_i4, offset, true);
}
