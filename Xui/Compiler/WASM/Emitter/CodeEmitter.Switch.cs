using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.CodeFlow;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitSwitch(SwitchBlock block)
    {
        CheckStack(block.Selector.Before!, block.Selector.Offset);
        Pop(SignatureTypeCode.Int32, block.Selector.Code, block.Selector.Offset);
        writer.LocalSet(method.SwitchSelector);
        CheckStack(block.Selector.After!, block.Selector.Offset);
        var incoming = stack.Reverse().ToArray();
        var initialLocals = (bool[])initializedLocals.Clone();
        MsilStackEntry[]? result = null;
        bool[]? resultLocals = null;
        int end = labels.Count;
        writer.Block(method.BlockTypes[block]); labels.Push((null, default));
        // Each landing block returns the input suffix intact to its selected arm.
        for (int i = block.Arms.Count - 1; i >= 0; i--)
        {
            writer.Block(method.SwitchArmTypes[block]); labels.Push((null, default));
        }
        writer.LocalGet(method.SwitchSelector).BrTable(block.CaseArms.Select(index => new LabelIndex((uint)index)).ToArray(), new((uint)block.DefaultArm));
        foreach (var arm in block.Arms)
        {
            writer.End(); labels.Pop();
            stack.Clear(); foreach (var value in incoming) stack.Push(value);
            initialLocals.CopyTo(initializedLocals, 0);
            EmitFlow(arm.Body);
            if (arm.CanContinue)
            {
                CoerceNullStrings(block.Join!.Before!);
                CheckStack(block.Join!.Before!, block.Join.Offset);
                var values = stack.Reverse().ToArray();
                if (result is null) { result = values; resultLocals = (bool[])initializedLocals.Clone(); }
                else
                {
                    for (int i = 0; i < initializedLocals.Length; i++) resultLocals![i] &= initializedLocals[i];
                }
            }
            writer.Br(new((uint)(labels.Count - end - 1)));
        }
        writer.End(); labels.Pop();
        stack.Clear();
        if (block.Join is not null)
        {
            if (result is null) throw new InvalidProgramException("Switch continuation has no incoming arm.");
            foreach (var value in result) stack.Push(value);
            resultLocals!.CopyTo(initializedLocals, 0);
            CheckStack(block.Join.Before!, block.Join.Offset);
        }
        else writer.Unreachable();
    }
}
