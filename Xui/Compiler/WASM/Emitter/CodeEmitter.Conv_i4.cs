using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_i4(int offset)
    {
        ConvertToInt32(OpCode.Conv_i4, offset);
    }
}
