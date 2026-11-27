namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Transient stack effect of one instruction, retaining the lowest consumed prefix.</summary>
internal struct StackCursor(InstructionNode instruction)
{
    public StackState State { get; private set; } = instruction.Before!;
    public StackState Lowest { get; private set; } = instruction.Before!;

    public StackEntry Pop()
    {
        if (State.Parent is null) throw Error("Evaluation stack underflow");
        var type = State.Entry!.Value;
        State = State.Parent;
        if (State.Depth < Lowest.Depth) Lowest = State;
        return type;
    }

    public void Pop(StackEntry expected)
    {
        var actual = Pop();
        if (actual != expected) throw Error($"Evaluation stack type mismatch. Expected {expected}, found {actual}");
    }

    public void Store(StackEntry expected)
    {
        var actual = Pop();
        if (actual == expected) return;
        if (actual.Type == StackType.String && expected.DeclaredType?.Code == System.Reflection.Metadata.SignatureTypeCode.Object)
        { instruction.StoresStringAsObject = true; return; }
        if (actual.Type == StackType.ArrayReference && expected.Type == StackType.ObjectReference &&
            (expected.DeclaredType?.Code == System.Reflection.Metadata.SignatureTypeCode.Object ||
             actual.DeclaredType?.Array is { } array && expected.DeclaredType?.Identity is { } contract &&
             DotNet.Intrinsics.ArrayInterface.Matches(array, contract))) return;
        if (actual.Type == StackType.ObjectReference && expected.DeclaredType?.Code == System.Reflection.Metadata.SignatureTypeCode.Object) return;
        if (actual.Type == StackType.ObjectReference && expected.Type == StackType.ObjectReference &&
            actual.DeclaredType?.Identity is { } from && expected.DeclaredType?.Identity is { } to && from.IsAssignableTo(to)) return;
        if (actual.Type == StackType.Null && expected.Type is StackType.ObjectReference or StackType.String or StackType.ArrayReference) return;
        // CLI assignment compatibility permits Int32/native-int storage transitions.
        if (actual.Type is StackType.Int32 or StackType.NativeInt && expected.Type is StackType.Int32 or StackType.NativeInt) return;
        throw Error($"Evaluation stack type mismatch. Expected {expected}, found {actual}");
    }

    public void Push(StackEntry type) => State = State.Push(type);
    public InvalidProgramException Error(string message) => new($"{message} for {instruction.Code} at IL_{instruction.Offset:x4}.");
}
