namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>An enum member's name and unsigned underlying bits, read from the Constant table.</summary>
public readonly record struct EnumConstant(
    string Name,
    ulong Bits);
