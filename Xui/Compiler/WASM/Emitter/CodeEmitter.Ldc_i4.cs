using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_i4(int value)
    {
        writer.I32Const(value);
        stack.Push(new MsilStackEntry(SignatureTypeCode.Int32));
    }
}
