namespace Xui.Compiler.XuiNet.Layout;

/// <summary>The byte offset and storage size of an instance field within its unboxed struct.</summary>
public readonly record struct FieldLayout(
    int Offset,
    int Size);
