using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_i8(int offset) => EmitIndirect(OpCode.Ldind_i8, offset, store: false);
}
