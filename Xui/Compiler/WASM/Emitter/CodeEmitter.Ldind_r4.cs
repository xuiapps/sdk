using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_r4(int offset) => EmitIndirect(OpCode.Ldind_r4, offset, store: false);
}
