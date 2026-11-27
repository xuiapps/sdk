using System.IO;

namespace Xui.Compiler.Runtime.IO;

/// <summary>A synchronous stream surface for the browser asset profile.</summary>
public abstract class Stream : IDisposable
{
    protected bool Closed;
    public abstract bool CanRead { get; }
    public abstract bool CanSeek { get; }
    public abstract bool CanWrite { get; }
    public abstract long Length { get; }
    public abstract long Position { get; set; }
    public abstract int Read(Span<byte> buffer);
    public virtual int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return Read(buffer.AsSpan(offset, count));
    }
    public virtual int ReadByte() { Span<byte> value = stackalloc byte[1]; return Read(value) == 0 ? -1 : value[0]; }
    /// <summary>Accumulates short reads until the minimum is met or the stream ends.</summary>
    public int ReadAtLeast(Span<byte> buffer, int minimumBytes, bool throwOnEndOfStream = true)
    {
        if ((uint)minimumBytes > (uint)buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(minimumBytes));
        int total = 0;
        while (total < minimumBytes)
        {
            int count = Read(buffer.Slice(total));
            if (count == 0)
            {
                if (throwOnEndOfStream) throw new EndOfStreamException();
                break;
            }
            total += count;
        }
        return total;
    }
    public void ReadExactly(Span<byte> buffer)
    {
        int offset = 0;
        while (offset < buffer.Length) { int count = Read(buffer.Slice(offset)); if (count == 0) throw new EndOfStreamException(); offset += count; }
    }
    public void ReadExactly(byte[] buffer, int offset, int count) => ReadExactly(buffer.AsSpan(offset, count));
    public virtual void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();
    public virtual void Write(byte[] buffer, int offset, int count) { ArgumentNullException.ThrowIfNull(buffer); Write(buffer.AsSpan(offset, count)); }
    public virtual void WriteByte(byte value) { Span<byte> bytes = stackalloc byte[1]; bytes[0] = value; Write(bytes); }
    public virtual void Flush() { EnsureOpen(); }
    public virtual long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public virtual void SetLength(long value) => throw new NotSupportedException();
    public void CopyTo(Stream destination) => CopyTo(destination, 81920);
    public virtual void CopyTo(Stream destination, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
        var buffer = new byte[bufferSize];
        int count;
        while ((count = Read(buffer, 0, buffer.Length)) != 0) destination.Write(buffer, 0, count);
    }
    protected void EnsureOpen() { if (Closed) throw new ObjectDisposedException(null); }
    protected virtual void Dispose(bool disposing) { Closed = true; }
    public void Dispose() => Dispose(true);
    public virtual void Close() => Dispose();
}
