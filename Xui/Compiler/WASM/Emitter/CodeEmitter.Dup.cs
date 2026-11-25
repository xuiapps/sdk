namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitDup(int offset)
    {
        if (!stack.TryPeek(out var value))
            throw new InvalidProgramException($"Evaluation stack underflow for Dup at IL_{offset:x4}.");
        if (value.IsByReference)
        {
            writer.LocalSet(method.GcI32).LocalSet(method.GcOwner)
                .LocalGet(method.GcOwner).LocalGet(method.GcI32).LocalGet(method.GcOwner).LocalGet(method.GcI32);
            stack.Push(value);
            return;
        }
        if (IsGc(value) || value.IsNull)
        {
            writer.LocalTee(method.GcSource).LocalGet(method.GcSource);
            if (!value.IsByReference && value.Identity is { IsValueType: true } gcIdentity) CloneGc(gcIdentity);
            stack.Push(value);
            return;
        }
        if (value.Identity is { } identity && !value.IsByReference)
        {
            writer.LocalTee(Frame.I32Scratch).LocalGet(Frame.I32Scratch);
            CopyValue(identity, offset);
            stack.Push(value);
            return;
        }
        var temporary = method.DupTemporary(value.IsByReference ? System.Reflection.Metadata.SignatureTypeCode.Int32 : value.TypeCode);
        writer.LocalTee(temporary).LocalGet(temporary);
        stack.Push(value);
    }
}
