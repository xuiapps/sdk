using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitThrow(int offset)
    {
        var value = Pop(OpCode.Throw, offset);
        if (!value.IsNull && (value.Identity is not { } type || ExceptionTypes.Kind(type) is null))
            throw new NotSupportedException($"Only supported built-in exception objects can be thrown: {value.Identity} in {method.Identity} at IL_{offset:x4}.");
        CheckExceptionNull();
        writer.Throw(method.Storage.ExceptionTag);
        stack.Clear();
    }
}
