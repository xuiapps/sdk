namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Extracts block parameter and result types above an interned prefix.</summary>
internal static class StackSignature
{
    internal static IReadOnlyList<StackEntry> Suffix(StackState state, StackState prefix)
    {
        var types = new StackEntry[state.Depth - prefix.Depth];
        for (int i = types.Length - 1; i >= 0; i--, state = state.Pop()) types[i] = state.Entry!.Value;
        if (!ReferenceEquals(state, prefix)) throw new InvalidProgramException("Block stack prefix mismatch.");
        return Array.AsReadOnly(types);
    }
}
