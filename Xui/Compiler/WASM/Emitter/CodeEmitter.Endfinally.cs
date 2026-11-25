using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitEndfinally()
    {
        if (cleanupExits.Count == 0) throw new InvalidProgramException("endfinally outside a cleanup handler.");
        BranchExceptionLabel(cleanupExits.Peek());
    }
}
