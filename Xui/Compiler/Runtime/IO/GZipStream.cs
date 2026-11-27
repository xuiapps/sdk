using System.IO.Compression;

namespace Xui.Compiler.Runtime.IO;

/// <summary>Reads a gzip asset prepared by the host before module startup; dynamic compression is unsupported.</summary>
public class GZipStream : ResourceStream
{
    private readonly Stream source;
    private readonly bool leaveOpen;
    public GZipStream(Stream stream, CompressionMode mode) : this(stream, mode, false) { }
    public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (mode != CompressionMode.Decompress) throw new NotSupportedException("The browser asset profile supports decompression only.");
        if (!stream.CanRead) throw new ArgumentException("The stream must support reading.");
        source = stream; this.leaveOpen = leaveOpen;
        if (!NativeResources.Gzip(this, stream)) throw new NotSupportedException("Gzip decompression requires a prepared resource stream at its beginning.");
    }
    public Stream BaseStream => source;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (!Closed && disposing && !leaveOpen) source.Dispose();
        base.Dispose(disposing);
    }
}
