using System.Runtime.InteropServices;
using nfloat = System.Runtime.InteropServices.NFloat;

namespace Samples.Layout;

public struct Padded
{
    public byte Tag;
    public double Value;
    public short Count;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct Packed
{
    public byte Tag;
    public int Value;
    public short Code;
}

public struct Nested
{
    public byte Tag;
    public Packed Value;
    public short End;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Union
{
    [FieldOffset(0)] public int Bits;
    [FieldOffset(0)] public float Value;
    [FieldOffset(12)] public short Tail;
}

[StructLayout(LayoutKind.Sequential, Size = 11)]
public struct Sized
{
    public int Value;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MinimumSize
{
    public int Value;
}

public struct Empty { }

public enum Kind : ushort { First, Second }

public struct SmallFields
{
    public bool Enabled;
    public char Character;
    public Kind Kind;
}

public struct NativeFields
{
    public nint Address;
    public nfloat Value;
}

public struct WithStatic
{
    public static long Shared;
    public int Value;
}
