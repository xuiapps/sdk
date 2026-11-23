using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_r8(double value)
    {
        writer.F64Const(value);
        // Use f64 for the MSIL floating-point evaluation stack category F.
        stack.Push(new MsilStackEntry(SignatureTypeCode.Double));
    }
}
