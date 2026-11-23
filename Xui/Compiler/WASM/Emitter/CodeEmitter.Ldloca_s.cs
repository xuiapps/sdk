using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdloca_s(Instruction.Variable instruction, int offset)
    {
        LoadLocalAddress(instruction.Index, instruction.Code, offset);
    }
}
