using System.IO;

namespace Xui.Compiler.Runtime.IO;

/// <summary>A read-only cursor over a packaged asset, with bounded borrowed linear-memory reads.</summary>
public class ResourceStream : Stream
{
    protected ResourceStream() { }
    public static Stream? Open(string assembly, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var stream = new ResourceStream();
        return NativeResources.Open(stream, assembly, name) ? stream : null;
    }
    public override bool CanRead => !Closed;
    public override bool CanSeek => !Closed;
    public override bool CanWrite => false;
    public override long Length { get { EnsureOpen(); return NativeResources.Length(this); } }
    public override long Position {
        get { EnsureOpen(); return NativeResources.Position(this); }
        set { EnsureOpen(); if (value < 0 || value > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(value)); NativeResources.Seek(this, value); }
    }
    public override unsafe int Read(Span<byte> buffer)
    {
        EnsureOpen();
        byte* scratch = stackalloc byte[4096];
        int total = 0;
        while (total < buffer.Length)
        {
            int count = NativeResources.Read(this, (nint)scratch, Math.Min(4096, buffer.Length - total));
            if (count < 0) throw new InvalidDataException("Invalid gzip resource.");
            if (count == 0) break;
            new ReadOnlySpan<byte>(scratch, count).CopyTo(buffer.Slice(total, count));
            total += count;
        }
        return total;
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        long position = origin switch { SeekOrigin.Begin => offset, SeekOrigin.Current => Position + offset, SeekOrigin.End => Length + offset,
            _ => throw new ArgumentException("Invalid seek origin.") };
        if (position < 0) throw new IOException("Attempted to seek before the beginning of the stream.");
        Position = position; return position;
    }
    protected override void Dispose(bool disposing) { if (!Closed) NativeResources.Close(this); base.Dispose(disposing); }
}
