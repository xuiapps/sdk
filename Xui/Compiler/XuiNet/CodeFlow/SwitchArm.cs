namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>One distinct switch destination and its structured body.</summary>
public sealed record SwitchArm(
    InstructionNode Entry,
    SequenceBlock Body,
    bool CanContinue);
