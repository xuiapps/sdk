using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_ref(int offset) => EmitIndirect(OpCode.Ldind_ref, offset, store: false);
}
