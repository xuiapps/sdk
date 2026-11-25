using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_i2(int offset) => EmitIndirect(OpCode.Ldind_i2, offset, store: false);
}
