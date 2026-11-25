using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitPop(int offset)
    {
        if (Pop(OpCode.Pop, offset).IsByReference) writer.Drop();
        writer.Drop();
    }
}
