namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_i4_0() => EmitLdc_i4(0);
}
