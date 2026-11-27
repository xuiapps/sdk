using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdlen(int offset)
    {
        var value = Pop(OpCode.Ldlen, offset);
        if (value.Array is not { } array) throw new NotSupportedException("ldlen requires a typed vector.");
        if (!array.IsVector) throw new NotSupportedException("Rectangular arrays expose Length through System.Array.");
        CheckExceptionNull();
        writer.RefCast(method.Storage.ArrayShape(array).Type).ArrayLen();
        stack.Push(new(SignatureTypeCode.IntPtr));
    }
}
