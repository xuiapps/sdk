namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A proven boolean argument for a direct call specialization; the ordinary ABI is retained.</summary>
public readonly record struct ConstantBooleanArgument(
    int Index,
    bool Value);
