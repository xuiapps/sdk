namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdc_i4_s(int value) => EmitLdc_i4(value);
}
