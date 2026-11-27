using System.IO;

namespace Xui.Compiler.Runtime.IO;

/// <summary>Little-endian primitive reads for immutable browser asset formats.</summary>
public class BinaryReader : IDisposable
{
    private readonly bool leaveOpen;
    private bool closed;
    public virtual Stream BaseStream { get; }
    public BinaryReader(Stream input) { ArgumentNullException.ThrowIfNull(input); if (!input.CanRead) throw new ArgumentException("Stream must be readable."); BaseStream = input; }
    public BinaryReader(Stream input, System.Text.Encoding encoding) : this(input, encoding, false) { }
    public BinaryReader(Stream input, System.Text.Encoding encoding, bool leaveOpen) : this(input)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        this.leaveOpen = leaveOpen;
    }
    public virtual byte ReadByte() { EnsureOpen(); int value = BaseStream.ReadByte(); if (value < 0) throw new EndOfStreamException(); return (byte)value; }
    public virtual sbyte ReadSByte() => unchecked((sbyte)ReadByte());
    public virtual bool ReadBoolean() => ReadByte() != 0;
    public virtual ushort ReadUInt16() { EnsureOpen(); Span<byte> bytes = stackalloc byte[2]; BaseStream.ReadExactly(bytes); return (ushort)(bytes[0] | bytes[1] << 8); }
    public virtual short ReadInt16() => unchecked((short)ReadUInt16());
    public virtual uint ReadUInt32() { EnsureOpen(); Span<byte> bytes = stackalloc byte[4]; BaseStream.ReadExactly(bytes); return (uint)(bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24); }
    public virtual int ReadInt32() => unchecked((int)ReadUInt32());
    public virtual ulong ReadUInt64() => ReadUInt32() | (ulong)ReadUInt32() << 32;
    public virtual long ReadInt64() => unchecked((long)ReadUInt64());
    public virtual float ReadSingle() => BitConverter.Int32BitsToSingle(ReadInt32());
    public virtual double ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());
    public virtual byte[] ReadBytes(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        EnsureOpen();
        byte[] bytes = new byte[count]; int total = 0;
        while (total < count) { int read = BaseStream.Read(bytes, total, count - total); if (read == 0) break; total += read; }
        if (total == count) return bytes;
        byte[] partial = new byte[total]; bytes.AsSpan(0, total).CopyTo(partial); return partial;
    }
    private void EnsureOpen() { if (closed) throw new ObjectDisposedException(null); }
    protected virtual void Dispose(bool disposing) { if (!closed && disposing && !leaveOpen) BaseStream.Dispose(); closed = true; }
    public void Dispose() => Dispose(true);
    public virtual void Close() => Dispose();
}
