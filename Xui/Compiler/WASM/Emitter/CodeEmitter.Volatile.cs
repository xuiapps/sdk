namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitVolatile()
    {
        // This target has one agent, private linear memory and nonshared GC objects.
        // Ordinary loads/stores preserve all observable ordering in that model.
        // A future shared-memory target must lower volatile accesses with acquire/
        // release semantics rather than reuse this single-agent lowering.
    }
}
