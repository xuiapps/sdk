using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_u2(int offset)
    {
        ConvertToInt32(OpCode.Conv_u2, offset);
        writer.I32Const(65535).I32And();
    }
}
