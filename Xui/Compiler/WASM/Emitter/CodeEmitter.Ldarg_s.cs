using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdarg_s(Instruction.Variable instruction, int offset) =>
        LoadArgument(instruction.Index, instruction.Code, offset);
}
