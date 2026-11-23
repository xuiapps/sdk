using System.IO.Compression;
using System.Text.Json;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;
using Xui.GPU.Software;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

/// <summary>Generated atlas, immutable quad layers, and vector labels. No SVG parsing at runtime.</summary>
public sealed class FloorplanGpuModel
{
    private static readonly Lazy<Task<FloorplanGpuModel>> shared = new(() => Task.Run(LoadEmbedded));
    public static Task<FloorplanGpuModel> LoadSharedAsync() => shared.Value;
    public static void Preload() => _ = LoadSharedAsync();
    public int Width { get; private init; }
    public int Height { get; private init; }
    public byte[][] Mips { get; private init; } = [];
    public FloorplanShader.Vertex[][] Layers { get; private init; } = [];
    public int[] StructureQuadCounts { get; private init; } = [];
    public LabelData[][] Labels { get; private init; } = [];
    private readonly Lazy<SampledTexture> software;
    private FloorplanGpuModel() => software = new(() => new(Width, Height, Mips));
    public Texture2D<Color4> SoftwareTexture => new(software.Value);
    public sealed record LabelData(string Text, Point Position, Font Font, TextAlign Align, AffineTransform Transform, Color Color, Rect Bounds);

    private static Stream Resource(string name) => typeof(FloorplanGpuModel).Assembly
        .GetManifestResourceStream("TestApp.FloorplanGpu." + name) ?? throw new InvalidDataException($"Missing floorplan GPU asset: {name}");
    private static FloorplanGpuModel LoadEmbedded()
    {
        using var atlas = Resource("atlas.bin.gz"); using var q0 = Resource("quads-l0.bin"); using var q1 = Resource("quads-l1.bin");
        using var q2 = Resource("quads-l2.bin"); using var q3 = Resource("quads-l3.bin"); using var json = Resource("atlas.json");
        using var g0 = Resource("structure-l0.bin"); using var g1 = Resource("structure-l1.bin");
        using var g2 = Resource("structure-l2.bin"); using var g3 = Resource("structure-l3.bin");
        return Load(atlas, [q0, q1, q2, q3], [g0, g1, g2, g3], json);
    }

    public static FloorplanGpuModel Load(Stream compressedAtlas, IReadOnlyList<Stream> quadLayers, IReadOnlyList<Stream> structureLayers, Stream metadata)
    {
        using var gzip = new GZipStream(compressedAtlas, CompressionMode.Decompress, leaveOpen: true);
        using var atlas = new BinaryReader(gzip);
        Header(atlas, "XFPA"u8);
        int width = atlas.ReadInt32(), height = atlas.ReadInt32(), count = atlas.ReadInt32();
        if (width is < 1 or > 8192 || height is < 1 or > 8192 || count is < 1 or > 14) throw new InvalidDataException("Invalid atlas dimensions.");
        var mips = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            int expected = checked(Math.Max(1, width >> i) * Math.Max(1, height >> i) * 4);
            if (atlas.ReadInt32() != expected) throw new InvalidDataException("Invalid mip size.");
            mips[i] = atlas.ReadBytes(expected);
        }
        GpuTextureValidation.Validate(width, height, mips);
        if (atlas.BaseStream.ReadByte() != -1) throw new InvalidDataException("Trailing atlas bytes.");
        if (quadLayers.Count != 4) throw new InvalidDataException("Expected four quad layers.");
        var layers = new FloorplanShader.Vertex[4][];
        Span<float> values = stackalloc float[13];
        for (int level = 0; level < 4; level++)
        {
            using var quads = new BinaryReader(quadLayers[level], System.Text.Encoding.UTF8, leaveOpen: true);
            Header(quads, "XFPQ"u8);
            if (quads.ReadInt32() != level) throw new InvalidDataException("Unexpected quad layer index.");
            int quadCount = quads.ReadInt32();
            if (quadCount is < 0 or > 100000) throw new InvalidDataException("Invalid quad count.");
            var vertices = layers[level] = new FloorplanShader.Vertex[quadCount * 6];
            for (int q = 0; q < quadCount; q++)
            {
                for (int n = 0; n < values.Length; n++)
                {
                    values[n] = quads.ReadSingle();
                    if (!float.IsFinite(values[n])) throw new InvalidDataException("Nonfinite quad geometry.");
                }
                if (values[8] < 0 || values[9] < 0 || values[10] > 1 || values[11] > 1
                    || values[8] >= values[10] || values[9] >= values[11]) throw new InvalidDataException("Invalid atlas coordinates.");
                ReadOnlySpan<int> corners = [0, 1, 2, 0, 2, 3];
                for (int n = 0; n < 6; n++)
                {
                    int c = corners[n];
                    vertices[q * 6 + n] = new() { PositionUv = new(values[c * 2], values[c * 2 + 1],
                        c is 0 or 3 ? values[8] : values[10], c < 2 ? values[9] : values[11]), Detail = new(values[12], 0, 0, 1) };
                }
            }
            if (quads.BaseStream.ReadByte() != -1) throw new InvalidDataException("Trailing quad bytes.");
        }
        if (structureLayers.Count != 4) throw new InvalidDataException("Expected four structure layers.");
        var structureCounts = new int[4];
        Span<float> geometry = stackalloc float[22];
        for (int level = 0; level < 4; level++)
        {
            using var reader = new BinaryReader(structureLayers[level], System.Text.Encoding.UTF8, leaveOpen: true);
            Header(reader, "XFPG"u8);
            if (reader.ReadInt32() != level) throw new InvalidDataException("Unexpected structure layer index.");
            int quads = structureCounts[level] = reader.ReadInt32();
            if (quads is < 0 or > 500000) throw new InvalidDataException("Invalid structure quad count.");
            var combined = new FloorplanShader.Vertex[checked(quads * 6 + layers[level].Length)];
            for (int q = 0; q < quads; q++)
            {
                for (int i = 0; i < geometry.Length; i++)
                {
                    geometry[i] = reader.ReadSingle();
                    if (!float.IsFinite(geometry[i])) throw new InvalidDataException("Nonfinite structure geometry.");
                }
                if (geometry[20] is < 0 or > 1 || geometry[21] is < 0 or > 1) throw new InvalidDataException("Invalid structure color coordinates.");
                ReadOnlySpan<int> corners = [0, 1, 2, 0, 2, 3];
                for (int i = 0; i < 6; i++)
                {
                    int c = corners[i] * 5;
                    if (Math.Abs(geometry[c + 2]) > 4 || Math.Abs(geometry[c + 3]) > 4 || geometry[c + 4] is < 0 or > 1)
                        throw new InvalidDataException("Invalid structure edge coverage.");
                    combined[q * 6 + i] = new() { PositionUv = new(geometry[c], geometry[c + 1], geometry[20], geometry[21]),
                        Detail = new(0, geometry[c + 2], geometry[c + 3], geometry[c + 4]) };
                }
            }
            if (reader.BaseStream.ReadByte() != -1) throw new InvalidDataException("Trailing structure bytes.");
            layers[level].CopyTo(combined, quads * 6);
            layers[level] = combined; // Structure first, furniture above it; still one immutable buffer/draw per layer.
        }
        using var document = JsonDocument.Parse(metadata);
        var root = document.RootElement;
        if (root.GetProperty("Version").GetInt32() != 1 || root.GetProperty("Width").GetInt32() != width
            || root.GetProperty("Height").GetInt32() != height || root.GetProperty("MipCount").GetInt32() != count)
            throw new InvalidDataException("Atlas metadata does not match binary data.");
        if (!root.GetProperty("StructureQuadCounts").EnumerateArray().Select(x => x.GetInt32()).SequenceEqual(structureCounts))
            throw new InvalidDataException("Structure metadata does not match binary data.");
        var labels = root.GetProperty("Labels").EnumerateArray().Select(level => level.EnumerateArray().Select(label =>
        {
            var m = label.GetProperty("Transform").EnumerateArray().Select(x => (NFloat)x.GetDouble()).ToArray();
            var b = label.GetProperty("Bounds").EnumerateArray().Select(x => (NFloat)x.GetDouble()).ToArray();
            var c = label.GetProperty("Color").EnumerateArray().Select(x => (NFloat)x.GetDouble()).ToArray();
            return new LabelData(label.GetProperty("Text").GetString()!,
                new((NFloat)label.GetProperty("X").GetDouble(), (NFloat)label.GetProperty("Y").GetDouble()),
                new((NFloat)label.GetProperty("Size").GetDouble(), "Inter", new FontWeight((NFloat)label.GetProperty("Weight").GetDouble())),
                (TextAlign)label.GetProperty("Align").GetInt32(), new(m[0], m[1], m[2], m[3], m[4], m[5]), new(c[0], c[1], c[2], c[3]), new(b[0], b[1], b[2], b[3]));
        }).ToArray()).ToArray();
        if (labels.Length != 4) throw new InvalidDataException("Expected four label layers.");
        return new() { Width = width, Height = height, Mips = mips, Layers = layers, Labels = labels, StructureQuadCounts = structureCounts };
    }

    private static void Header(BinaryReader reader, ReadOnlySpan<byte> magic)
    {
        if (!reader.ReadBytes(4).AsSpan().SequenceEqual(magic) || reader.ReadInt32() != 1)
            throw new InvalidDataException("Unrecognized floorplan GPU asset version.");
    }
}
