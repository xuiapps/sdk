namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryVariableScalarAddress(int index, bool argument, LocalVariable type)
    {
        var slots = argument ? method.Frame?.ScalarArguments : method.Frame?.ScalarLocals;
        if (slots is null || !slots.TryGetValue(index, out int address)) return false;
        if (!argument && !initLocals) throw new NotSupportedException("Addressable scalar locals currently require initlocals.");
        writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
        FrameAddress(address);
        stack.Push(new(type.TypeCode, isByReference: true));
        return true;
    }

    private void StoreScalarVariable(int address, LocalVariable type)
    {
        writer.LocalSet(FieldScratch(type));
        FrameAddress(address);
        StoreField(type);
    }
}
