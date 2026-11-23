using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStloc_s(Instruction.Variable instruction, int offset) =>
        StoreLocal(new LocalIndex(instruction.Index), instruction.Code, offset);
}
