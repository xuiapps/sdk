using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_u1(int offset) => EmitIndirect(OpCode.Ldind_u1, offset, store: false);
}
