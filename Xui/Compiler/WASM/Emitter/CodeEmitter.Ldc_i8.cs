using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_i8(long value)
    {
        writer.I64Const(value);
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int64));
    }
}
