using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_u1(int offset)
    {
        ConvertToInt32(OpCode.Conv_u1, offset);
        writer.I32Const(255).I32And();
    }
}
