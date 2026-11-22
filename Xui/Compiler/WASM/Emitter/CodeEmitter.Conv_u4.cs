using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_u4(int offset)
    {
        ConvertToInt32(OpCode.Conv_u4, offset);
    }
}
