using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void LinearReferenceAddress() => writer.I32Load().LocalSet(method.GcI32)
        .RefNull(ValueTypeCode.EqRef).LocalGet(method.GcI32);

    private bool TryVariableReferenceAddress(int index, bool argument, LocalVariable type)
    {
        var slots = argument ? method.Frame?.ReferenceArguments : method.Frame?.ReferenceLocals;
        if (slots is null || !slots.TryGetValue(index, out int address)) return false;
        writer.RefNull(ValueTypeCode.EqRef);
        FrameAddress(address);
        writer.I32Load();
        stack.Push(new(type.TypeCode, type.Identity, true, type.Array));
        return true;
    }

    private void StoreReferenceVariable(int address, LocalVariable type)
    {
        var scratch = GcScratch(type);
        writer.LocalSet(scratch);
        FrameAddress(address);
        writer.LocalGet(scratch);
        StoreReference(type);
    }
}
