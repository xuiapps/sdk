using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLoop(LoopBlock loop)
    {
        loopExitInitialization.Remove(loop);
        CheckStack(loop.Header.Before!, loop.Header.Offset);
        var type = method.BlockTypes[loop];
        writer.Block(type);
        labels.Push((loop, LoopTransferKind.Break));
        writer.Loop(type);
        labels.Push((loop, LoopTransferKind.Continue));
        EmitFlow(loop.Body);
        writer.End().End();
        labels.Pop();
        labels.Pop();
        stack.Clear();
        RestorePrefix(loop.Exit.Before!);
        // Only actual exits contribute; a store on a continue path alone cannot initialize an exit local.
        loopExitInitialization[loop].CopyTo(initializedLocals, 0);
    }

    private void EmitLoopTransfer(LoopTransfer transfer)
    {
        var target = transfer.Kind == LoopTransferKind.Continue ? transfer.Target.Header : transfer.Target.Exit;
        CheckStack(target.Before!, target.Offset);
        if (transfer.Kind == LoopTransferKind.Break)
        {
            if (!loopExitInitialization.TryGetValue(transfer.Target, out var initialized))
                loopExitInitialization.Add(transfer.Target, (bool[])initializedLocals.Clone());
            else
                for (int i = 0; i < initialized.Length; i++) initialized[i] &= initializedLocals[i];
        }
        uint depth = 0;
        foreach (var label in labels)
        {
            if (label.Loop == transfer.Target && label.Kind == transfer.Kind)
            {
                writer.Br(new LabelIndex(depth));
                return;
            }
            depth++;
        }
        throw new InvalidProgramException("Loop transfer has no enclosing target label.");
    }
}
