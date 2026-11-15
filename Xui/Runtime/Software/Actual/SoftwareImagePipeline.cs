using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Software.Actual;

/// <summary>Cached packaged PNG resources for deterministic, self-contained SVG rendering.</summary>
public sealed class SoftwareImagePipeline : IImagePipeline
{
    private readonly string baseDirectory;
    private readonly ConcurrentDictionary<string, SoftwareImage.Resource> resources = new(StringComparer.Ordinal);

    public SoftwareImagePipeline(string? baseDirectory = null) => this.baseDirectory = baseDirectory ?? AppContext.BaseDirectory;

    public IImage CreateImage() => new SoftwareImage(this);

    internal SoftwareImage.Resource Load(string uri)
    {
        var path = AssetUri.TryGetRelativePath(uri, out var asset)
            ? Path.Combine(baseDirectory, "Assets", asset)
            : Path.IsPathRooted(uri) ? uri : Path.Combine(baseDirectory, uri);
        return resources.GetOrAdd(Path.GetFullPath(path), static path => SoftwareImage.Resource.FromPng(File.ReadAllBytes(path)));
    }
}

/// <summary>An image handle retaining the original PNG bytes for SVG embedding.</summary>
public sealed class SoftwareImage : IImage
{
    private readonly SoftwareImagePipeline pipeline;
    internal Resource? Content { get; private set; }
    internal SoftwareImage(SoftwareImagePipeline pipeline) => this.pipeline = pipeline;
    public Size Size => Content?.Size ?? Size.Empty;
    public void Load(string uri) => Content = pipeline.Load(uri);
    public async Task LoadAsync(string uri) => Content = await Task.Run(() => pipeline.Load(uri)).ConfigureAwait(false);

    public void LoadPixels(int width, int height, ReadOnlySpan<byte> bgra32Data)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        var count = checked(width * height * 4);
        if (bgra32Data.Length != count) throw new ArgumentException("Expected width * height * 4 BGRA bytes.", nameof(bgra32Data));
        var rgba = bgra32Data.ToArray();
        for (var i = 0; i < rgba.Length; i += 4) (rgba[i], rgba[i + 2]) = (rgba[i + 2], rgba[i]);
        using var stream = new MemoryStream();
        PngEncoder.SaveRGBA(stream, rgba, width, height);
        Content = Resource.FromPng(stream.ToArray());
    }

    internal sealed class Resource(Size size, string dataUri)
    {
        public Size Size { get; } = size;
        public string DataUri { get; } = dataUri;
        public static Resource FromPng(byte[] data)
        {
            if (data.Length < 33 || !data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ||
                !data.AsSpan(12, 4).SequenceEqual("IHDR"u8))
                throw new NotSupportedException("The software image pipeline currently supports PNG images only.");
            var width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(16, 4));
            var height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(20, 4));
            if (width <= 0 || height <= 0) throw new InvalidDataException("Invalid PNG dimensions.");
            return new Resource(new Size(width, height), "data:image/png;base64," + Convert.ToBase64String(data));
        }
    }
}
