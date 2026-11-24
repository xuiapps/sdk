using System.Runtime.CompilerServices;

public struct Point { public short X; public int Y; }
[InlineArray(3)] public struct Points { private Point first; }
[InlineArray(4)] public struct Labels { private string first; }
public struct Holder { public byte Head; public Points Values; public byte Tail; }
