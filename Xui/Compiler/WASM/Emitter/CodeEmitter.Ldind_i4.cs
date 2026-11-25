using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdind_i4(int offset) => EmitIndirect(OpCode.Ldind_i4, offset, false);
}
