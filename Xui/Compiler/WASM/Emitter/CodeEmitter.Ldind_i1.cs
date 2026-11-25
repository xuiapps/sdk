using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_i1(int offset) => EmitIndirect(OpCode.Ldind_i1, offset, store: false);
}
