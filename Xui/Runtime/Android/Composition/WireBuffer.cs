using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Android.Composition;

// Android transport v1: little endian, int32 enums/lengths, float32 scalars,
// UTF-16 text. Independent of CLR struct layout and nfloat width.
internal sealed class WireBuffer : IDisposable
{
    private byte[] data = [];
    internal int Length { get; private set; }
    internal ReadOnlySpan<byte> Data => data.AsSpan(0, Length);
    internal void Clear() => Length = 0;
    private Span<byte> Allocate(int count)
    {
        int end = checked(Length + count);
        if (end > data.Length)
        {
            var next = ArrayPool<byte>.Shared.Rent(Math.Max(end, Math.Max(256, data.Length * 2)));
            Data.CopyTo(next);
            if (data.Length != 0) ArrayPool<byte>.Shared.Return(data);
            data = next;
        }
        var result = data.AsSpan(Length, count); Length = end; return result;
    }
    internal void Int(int value) => BinaryPrimitives.WriteInt32LittleEndian(Allocate(4), value);
    internal void Float(NFloat value) => BinaryPrimitives.WriteSingleLittleEndian(Allocate(4), (float)value);
    internal void Bytes(ReadOnlySpan<byte> value) => value.CopyTo(Allocate(value.Length));
    internal void Text(ReadOnlySpan<char> value)
    {
        Int(value.Length);
        foreach (char c in value) BinaryPrimitives.WriteUInt16LittleEndian(Allocate(2), c);
    }
    internal void Point(Point p) { Float(p.X); Float(p.Y); }
    internal void Vector(Vector v) { Float(v.X); Float(v.Y); }
    internal void Rect(Rect r) { Float(r.X); Float(r.Y); Float(r.Width); Float(r.Height); }
    internal void Matrix(AffineTransform m) { Float(m.A); Float(m.B); Float(m.C); Float(m.D); Float(m.Tx); Float(m.Ty); }
    internal void Color(Color c) { Float(c.Red); Float(c.Green); Float(c.Blue); Float(c.Alpha); }
    internal void PatchInt(int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset, 4), value);
    public void Dispose() { if (data.Length != 0) ArrayPool<byte>.Shared.Return(data); data = []; Length = 0; }
}
