using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_r8(int offset) => EmitIndirect(OpCode.Ldind_r8, offset, store: false);
}
