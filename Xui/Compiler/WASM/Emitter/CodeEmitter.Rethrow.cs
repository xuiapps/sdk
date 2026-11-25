using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitRethrow()
    {
        if (caughtExceptions.Count == 0) throw new InvalidProgramException("rethrow outside a catch handler.");
        writer.LocalGet(caughtExceptions.Peek()).ThrowRef();
        stack.Clear();
    }
}
