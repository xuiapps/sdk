using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitPop(int offset)
    {
        _ = Pop(OpCode.Pop, offset);
        writer.Drop();
    }
}
