using Xui.Apps.TestApp.Pages.FloorplanGpu;
using Xui.Core.Math2D;
using Xui.GPU.Backends.Metal;
using Xui.GPU.Backends.Hlsl;
using Xui.GPU.Hardware.Metal;
using Xui.GPU.Software;
using Xui.Runtime.Software;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Component;

public class FloorplanGpuTest
{
    [Fact]
    public async Task StructurePreservesWallOpeningsDoorSwingsAndRotatedLeaves()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        Assert.All(model.StructureQuadCounts, count => Assert.True(count > 1000));
        using var renderer = new FloorplanGpuRenderer(model);
        using var fb = new Framebuffer(240, 240, false);
        var transition = new FloorplanLayerTransition();
        transition.Request(3); transition.Advance((NFloat).6);
        RenderPatch(110, 290);
        Assert.Equal(0xB8B8B8FFu, Pixel(145, 322, 110, 290)); // Solid wall beside the opening.
        Assert.Equal(0xFFFFFFFFu, Pixel(130, 321, 110, 290)); // Opening is not bridged by a quad.
        Assert.Equal(0x496F60FFu, Pixel(120, 312, 110, 290)); // Open leaf extends upwards.
        Assert.Equal(0x496F60FFu, Pixel(134.14, 307.86, 110, 290)); // Quarter-circle swing.
        Assert.Equal(0xFFFFFFFFu, Pixel(130, 312, 110, 290)); // Swing has an unfilled interior.
        RenderPatch(195, 240);
        Assert.Equal(0x496F60FFu, Pixel(214, 254, 195, 240)); // A 90-degree leaf extends rightwards.

        void RenderPatch(int x, int y) => renderer.RenderSoftware(fb, (0, 0, 240, 240),
            AffineTransform.Scale((6, 6)) * AffineTransform.Translate((-x, -y)), transition);
        unsafe uint Pixel(double x, double y, int originX, int originY) => fb.ColorData[(int)((y - originY) * 6) * 240 + (int)((x - originX) * 6)];
    }

    [Fact]
    public async Task AtlasKeepsSvgOrientationAndSimplifiedWorkstations()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        using var metadata = typeof(FloorplanGpuModel).Assembly.GetManifestResourceStream("TestApp.FloorplanGpu.atlas.json")!;
        using var json = System.Text.Json.JsonDocument.Parse(metadata);
        var sprites = json.RootElement.GetProperty("Sprites").EnumerateArray().ToArray();

        // Independent SVG landmarks: the chair back and toilet cistern are above their seats.
        // Metal/software agreement alone cannot detect an upside-down atlas shared by both.
        Assert.Equal(new byte[] { 199, 200, 200, 255 }, Pixel("task-chair", 0, 0, -28));
        Assert.Equal(new byte[] { 222, 222, 223, 255 }, Pixel("task-chair", 0, 0, 0));
        Assert.Equal(255, Pixel("toilet", 0, 20, -32)[3]);
        Assert.Equal(0, Pixel("toilet", 0, 20, 32)[3]);
        Assert.Equal(new byte[] { 170, 183, 191, 255 }, Pixel("basin", 2, 0, -18));
        Assert.Equal(new byte[] { 211, 225, 229, 255 }, Pixel("basin", 2, 0, 15));
        for (int level = 0; level < 2; level++)
        foreach (string part in new[] { "workstation-desk", "workstation-chair" })
        {
            var sprite = sprites.Single(s => s.GetProperty("Id").GetString() == part && s.GetProperty("Level").GetInt32() == level);
            float u = (float)sprite.GetProperty("X").GetInt32() / model.Width;
            float v = (float)sprite.GetProperty("Y").GetInt32() / model.Height;
            int placements = Enumerable.Range(0, model.Layers[level].Length / 6)
                .Count(i => (float)model.Layers[level][i * 6].PositionUv.Z == u && (float)model.Layers[level][i * 6].PositionUv.W == v);
            Assert.Equal(97, placements);
        }

        byte[] Pixel(string id, int level, double x, double y)
        {
            var s = sprites.Single(s => s.GetProperty("Id").GetString() == id && s.GetProperty("Level").GetInt32() == level);
            var bounds = s.GetProperty("LocalBounds").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            int px = s.GetProperty("X").GetInt32() + (int)((x - bounds[0]) * 2);
            int py = s.GetProperty("Y").GetInt32() + (int)((y - bounds[1]) * 2);
            return model.Mips[0].AsSpan((py * model.Width + px) * 4, 4).ToArray();
        }
    }

    [Fact]
    public async Task GeneratedAssetsHaveMipsIndependentQuadsAndVectorLabels()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        using var source = typeof(FloorplanGpuModel).Assembly.GetManifestResourceStream("TestApp.Floorplan.Office.svg")!;
        using var metadata = typeof(FloorplanGpuModel).Assembly.GetManifestResourceStream("TestApp.FloorplanGpu.atlas.json")!;
        using var json = System.Text.Json.JsonDocument.Parse(metadata);
        Assert.Equal(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source)), json.RootElement.GetProperty("SourceSha256").GetString());
        Assert.DoesNotContain(json.RootElement.GetProperty("Sprites").EnumerateArray(), sprite =>
            sprite.GetProperty("Id").GetString()!.Contains("door", StringComparison.Ordinal));
        Assert.Equal(5, model.Mips.Length);
        Assert.Equal(4, model.Layers.Length);
        Assert.All(model.Layers, layer => Assert.True(layer.Length / 6 > 400));
        Assert.True(model.Layers[3].Length > model.Layers[0].Length);
        Assert.All(model.Labels, layer => Assert.NotEmpty(layer));
        var msl = FloorplanShader.GenerateShaderSource(new MslCodeGenerator());
        Assert.Contains("texture2d<float>", msl);
        Assert.Contains("[[texture(0)]]", msl);
        Assert.Contains(".sample(", msl);
        Assert.Contains("level(", msl);
        var hlsl = FloorplanShader.GenerateShaderSource(new HlslCodeGenerator());
        Assert.Contains("register(t0)", hlsl); Assert.Contains("register(s0)", hlsl);
        Assert.Contains(".SampleLevel(", hlsl);
    }

    [Fact]
    public void TransitionsRevealThenRemoveAndQueueOnlyTheLatestRequestedLayer()
    {
        var t = new FloorplanLayerTransition(); t.Request(1);
        Assert.Equal(0, t.IncomingOpacity); Assert.Equal(1, t.CurrentOpacity);
        t.Advance((NFloat).15);
        Assert.Equal(.5, (double)t.IncomingOpacity, 5); Assert.Equal(1, t.CurrentOpacity);
        t.Request(2); t.Request(3);
        t.Advance((NFloat).3);
        Assert.Equal(1, t.IncomingOpacity); Assert.Equal(.5, (double)t.CurrentOpacity, 5);
        t.Advance((NFloat).15);
        Assert.Equal(1, t.Current); Assert.Equal(3, t.Incoming);
        t.Advance((NFloat).6);
        Assert.Equal(3, t.Current); Assert.False(t.Active);
        t.Request(0); t.Advance((NFloat).6);
        Assert.Equal(0, t.Current); Assert.False(t.Active);
    }

    [Fact]
    public async Task SoftwareRendersEveryLayerAndItsTransitions()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        RenderSoftware(model);
    }

    private static unsafe void RenderSoftware(FloorplanGpuModel model)
    {
        using var renderer = new FloorplanGpuRenderer(model);
        using var fb = new Framebuffer(600, 480, false);
        Rect viewport = (0, 0, 600, 480);
        var camera = AffineTransform.Scale(((NFloat).6, (NFloat).6)) * AffineTransform.Translate((0, -120));
        var transition = new FloorplanLayerTransition();
        var frames = new List<byte[]>();
        for (int level = 0; level < 4; level++)
        {
            transition.Request(level); transition.Advance((NFloat).6);
            renderer.RenderSoftware(fb, viewport, camera, transition);
            var rgba = Pixels(fb);
            Assert.True(rgba.Where((_, i) => i % 4 != 3).Count(x => x < 200) > 1000);
            Assert.All(rgba.Where((_, i) => i % 4 == 3), alpha => Assert.Equal(255, alpha));
            frames.Add(rgba);
            var output = Path.Combine(Path.GetTempPath(), $"xui-floorplan-gpu-level{level}.png");
            PngEncoder.SaveRGBA(output, rgba, fb.Width, fb.Height);
        }
        Assert.False(frames[0].AsSpan().SequenceEqual(frames[3]));
    }

    [IOS.IOSDrawingContextTest.CoreGraphicsFact]
    public async Task MetalExecutesGeneratedTextureShaderAndMatchesSoftware()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        CompareNative(model);
    }

    private static unsafe void CompareNative(FloorplanGpuModel model)
    {
        using var device = new IOSGpuDevice();
        using var target = device.CreateRenderTarget(300, 240);
        using var renderer = new FloorplanGpuRenderer(model);
        Task.Run(() => renderer.PrepareAndWarm(device, new MslCodeGenerator())).GetAwaiter().GetResult();
        using var fb = new Framebuffer(300, 240, false);
        Rect viewport = (0, 0, 300, 240);
        var camera = AffineTransform.Scale(((NFloat).3, (NFloat).3)) * AffineTransform.Translate((0, -120));
        var transition = new FloorplanLayerTransition();
        transition.Request(3); transition.Advance((NFloat).45);
        renderer.RenderNative(target, viewport, camera, transition);
        var actual = new byte[300 * 240 * 4]; target.ReadbackPixelsBgra(actual);
        renderer.RenderSoftware(fb, viewport, camera, transition);
        var expected = Pixels(fb);
        double error = 0;
        for (int i = 0; i < actual.Length; i += 4)
        {
            error += Math.Abs(actual[i] - expected[i + 2]);
            error += Math.Abs(actual[i + 1] - expected[i + 1]);
            error += Math.Abs(actual[i + 2] - expected[i]);
            error += Math.Abs(actual[i + 3] - expected[i + 3]);
        }
        Assert.True(error / actual.Length < 3, $"Mean channel error: {error / actual.Length}");
    }

    private static unsafe byte[] Pixels(Framebuffer fb)
    {
        var rgba = new byte[fb.Width * fb.Height * 4];
        for (int i = 0; i < fb.Width * fb.Height; i++)
        {
            uint p = fb.ColorData[i];
            rgba[i * 4] = (byte)(p >> 24); rgba[i * 4 + 1] = (byte)(p >> 16);
            rgba[i * 4 + 2] = (byte)(p >> 8); rgba[i * 4 + 3] = (byte)p;
        }
        return rgba;
    }
}
