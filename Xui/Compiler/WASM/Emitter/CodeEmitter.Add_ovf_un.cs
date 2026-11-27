using Xui.Compiler.XuiNet.DotNet.Op;
namespace Xui.Compiler.WASM.Emitter;
public readonly ref partial struct CodeEmitter
{
    private void EmitAdd_ovf_un(int offset) => EmitCheckedAddSub(OpCode.Add_ovf_un, offset);
}
