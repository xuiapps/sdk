using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitFlow(SequenceBlock sequence)
    {
        foreach (var child in sequence.Children)
        {
            switch (child)
            {
                case InstructionBlock block:
                    var node = block.Instruction;
                    CheckStack(node.Before!, node.Offset);
                    if (node.Code is not (OpCode.Br or OpCode.Br_s))
                    {
                        var reader = method.Body.Instructions;
                        reader.Seek(node.Offset);
                        EmitInstruction(ref reader);
                    }
                    CheckStack(node.After!, node.Offset);
                    break;
                case LoopBlock loop:
                    EmitLoop(loop);
                    break;
                case LoopTransfer transfer:
                    EmitLoopTransfer(transfer);
                    break;
                case ConditionalBlock conditional:
                    EmitConditional(conditional);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported structured node {child.GetType().Name}.");
            }
        }
    }

    private void EmitConditional(ConditionalBlock block)
    {
        var node = block.Condition;
        CheckStack(node.Before!, node.Offset);
        EmitCondition(node);
        CheckStack(node.After!, node.Offset);
        var incomingValues = stack.Reverse().ToArray();
        var initialized = (bool[])initializedLocals.Clone();
        writer.If(method.BlockTypes[block]);
        labels.Push((null, default));
        EmitFlow(block.Taken);
        var takenInitialized = (bool[])initializedLocals.Clone();
        writer.Else();
        stack.Clear();
        foreach (var value in incomingValues) stack.Push(value);
        initialized.CopyTo(initializedLocals, 0);
        EmitFlow(block.NotTaken);
        for (int i = 0; i < initializedLocals.Length; i++) initializedLocals[i] &= takenInitialized[i];
        writer.End();
        labels.Pop();
        if (block.Join is { } join) CheckStack(join.Before!, join.Offset);
        else writer.Unreachable(); // Both arms transfer control; the continuation cannot be reached.
    }

    private void RestorePrefix(StackState state)
    {
        if (state.Parent is null) return;
        RestorePrefix(state.Parent);
        stack.Push(SourceEntry(state.Entry!.Value));
    }

    private void EmitCondition(InstructionNode node)
    {
        switch (node.Code)
        {
            case OpCode.Beq: EmitBeq(node.Offset); break;
            case OpCode.Beq_s: EmitBeq_s(node.Offset); break;
            case OpCode.Bge: EmitBge(node.Offset); break;
            case OpCode.Bge_s: EmitBge_s(node.Offset); break;
            case OpCode.Bge_un: EmitBge_un(node.Offset); break;
            case OpCode.Bge_un_s: EmitBge_un_s(node.Offset); break;
            case OpCode.Bgt: EmitBgt(node.Offset); break;
            case OpCode.Bgt_s: EmitBgt_s(node.Offset); break;
            case OpCode.Bgt_un: EmitBgt_un(node.Offset); break;
            case OpCode.Bgt_un_s: EmitBgt_un_s(node.Offset); break;
            case OpCode.Ble: EmitBle(node.Offset); break;
            case OpCode.Ble_s: EmitBle_s(node.Offset); break;
            case OpCode.Ble_un: EmitBle_un(node.Offset); break;
            case OpCode.Ble_un_s: EmitBle_un_s(node.Offset); break;
            case OpCode.Blt: EmitBlt(node.Offset); break;
            case OpCode.Blt_s: EmitBlt_s(node.Offset); break;
            case OpCode.Blt_un: EmitBlt_un(node.Offset); break;
            case OpCode.Blt_un_s: EmitBlt_un_s(node.Offset); break;
            case OpCode.Bne_un: EmitBne_un(node.Offset); break;
            case OpCode.Bne_un_s: EmitBne_un_s(node.Offset); break;
            case OpCode.Brfalse: EmitBrfalse(node.Offset); break;
            case OpCode.Brfalse_s: EmitBrfalse_s(node.Offset); break;
            case OpCode.Brtrue: EmitBrtrue(node.Offset); break;
            case OpCode.Brtrue_s: EmitBrtrue_s(node.Offset); break;
            default: throw new NotSupportedException($"Unsupported conditional {node.Code} at IL_{node.Offset:x4}.");
        }
    }

    private void CheckStack(StackState expected, int offset)
    {
        var state = expected;
        bool widen = false;
        foreach (var actual in stack)
        {
            if (expected.Entry is not { } entry)
                throw new InvalidProgramException($"Emission stack differs from analysis at IL_{offset:x4}.");
            var wanted = SourceEntry(entry);
            if (!SameSourceType(actual, wanted))
            {
                if (entry.Type != StackType.ObjectReference ||
                    !(wanted.TypeCode == SignatureTypeCode.Object && IsObjectReference(actual)) &&
                    (wanted.Identity is not { } target || (!actual.IsNull &&
                    (actual.IsByReference || actual.Identity is not { } from || !from.IsAssignableTo(target)))))
                    throw new InvalidProgramException($"Emission stack differs from analysis at IL_{offset:x4}.");
                widen = true;
            }
            expected = expected.Pop();
        }
        if (widen)
        {
            // All class references share eqref storage; only the tracked source type changes at a join.
            var values = stack.ToArray();
            for (int i = 0; i < values.Length; i++, state = state.Pop())
                if (!SameSourceType(values[i], SourceEntry(state.Entry!.Value))) values[i] = SourceEntry(state.Entry.Value);
            stack.Clear();
            for (int i = values.Length - 1; i >= 0; i--) stack.Push(values[i]);
        }
        if (expected.Depth != 0)
            throw new InvalidProgramException($"Emission stack depth differs from analysis at IL_{offset:x4}.");
    }

    private static bool SameSourceType(MsilStackEntry left, MsilStackEntry right) =>
        left.TypeCode == right.TypeCode && left.Identity == right.Identity && left.IsByReference == right.IsByReference && left.IsNull == right.IsNull;

    private static MsilStackEntry SourceEntry(StackEntry entry) => entry.Type == StackType.Null
        ? new(SignatureTypeCode.Object) { IsNull = true } : entry.DeclaredType is { } declared
        ? new(declared.Code, declared.Identity, declared.IsByReference)
        : new(SourceType(entry.Type));

    private static SignatureTypeCode SourceType(StackType type) => type switch
    {
        StackType.Int32 => SignatureTypeCode.Int32,
        StackType.Int64 => SignatureTypeCode.Int64,
        StackType.NativeInt => SignatureTypeCode.IntPtr,
        StackType.Float => SignatureTypeCode.Double,
        StackType.String => SignatureTypeCode.String,
        _ => throw new NotSupportedException($"Unsupported stack type {type}.")
    };
}
