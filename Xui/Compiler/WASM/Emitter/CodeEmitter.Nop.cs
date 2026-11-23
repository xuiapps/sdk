namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNop()
    {
        // MSIL nop has no observable effect and requires no WASM instruction.
    }
}
