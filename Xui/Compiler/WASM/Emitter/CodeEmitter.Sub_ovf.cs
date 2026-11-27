using Xui.Compiler.XuiNet.DotNet.Op;
namespace Xui.Compiler.WASM.Emitter;
public readonly ref partial struct CodeEmitter
{
    private void EmitSub_ovf(int offset) => EmitCheckedAddSub(OpCode.Sub_ovf, offset);
}
