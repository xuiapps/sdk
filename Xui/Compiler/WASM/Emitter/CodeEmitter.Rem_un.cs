using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitRem_un(int offset) => EmitDivision(OpCode.Rem_un, offset);
}
