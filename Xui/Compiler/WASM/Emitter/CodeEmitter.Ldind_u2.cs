using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_u2(int offset) => EmitIndirect(OpCode.Ldind_u2, offset, store: false);
}
