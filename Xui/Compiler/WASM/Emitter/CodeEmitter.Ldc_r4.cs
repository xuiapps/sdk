using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_r4(float value)
    {
        writer.F32Const(value).F64PromoteF32();
        // Use f64 for the MSIL floating-point evaluation stack category F.
        stack.Push(new MsilStackEntry(SignatureTypeCode.Double));
    }
}
