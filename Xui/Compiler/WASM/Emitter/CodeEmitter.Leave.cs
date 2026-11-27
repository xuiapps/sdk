using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLeave(int target)
    {
        while (stack.TryPop(out _)) writer.Drop();
        foreach (var exit in exceptionExits)
            if (exit.Block.Continuations.Contains(target))
            {
                if (exit.Block.Continuations.Count > 1)
                    writer.I32Const(exit.Block.Continuations.ToList().IndexOf(target) - 1).LocalSet(method.ExceptionSelectors[exit.Block.Start]);
                BranchExceptionLabel(exit.Level); return;
            }
        throw new NotSupportedException($"leave to IL_{target:x4} is outside the structured exception continuation.");
    }
}
