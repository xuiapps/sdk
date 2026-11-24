using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStelem_ref(int offset) => EmitArrayStore(OpCode.Stelem_ref, offset);
}
