using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdelem_ref(int offset) => EmitArrayLoad(OpCode.Ldelem_ref, offset);
}
