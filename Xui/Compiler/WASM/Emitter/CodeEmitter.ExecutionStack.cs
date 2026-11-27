using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryExecutionStack(Instruction.Token instruction)
    {
        if (ExecutionStack.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } operation) return false;
        writer.GlobalGet(method.Storage.ExecutionStack.Depth).I32Const(ExecutionStackRuntime.SoftLimit).I32LtU();
        if (operation == "TryEnsureSufficientExecutionStack") stack.Push(new(SignatureTypeCode.Int32));
        else
        {
            writer.I32Eqz().If();
            ThrowException(ExceptionKind.InsufficientExecutionStackException);
            writer.End();
        }
        return true;
    }

    // Check before reserving the linear frame so a refused call owns no resources.
    private void CheckExecutionDepth()
    {
        if (!method.Storage.ExecutionStack.Enabled) return;
        writer.GlobalGet(method.Storage.ExecutionStack.Depth).I32Const(ExecutionStackRuntime.HardLimit).I32LtU().I32Eqz().If();
        ThrowException(ExceptionKind.InsufficientExecutionStackException);
        writer.End();
    }

    private void EnterExecutionDepth()
    {
        if (!method.Storage.ExecutionStack.Enabled) return;
        var depth = method.Storage.ExecutionStack.Depth;
        writer.GlobalGet(depth).I32Const(1).I32Add().GlobalSet(depth);
    }

    private void LeaveExecutionDepth()
    {
        if (!method.Storage.ExecutionStack.Enabled) return;
        var depth = method.Storage.ExecutionStack.Depth;
        writer.GlobalGet(depth).I32Const(1).I32Sub().GlobalSet(depth);
    }
}
