using Xui.Core.Canvas.Composition.Commands.Records;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Filter;
using Xui.Core.Math2D;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class CanvasCommandBufferTest
{
    private static GraphicsNode SingleChild(GraphicsNode node)
    {
        Assert.Equal(1, node.Count);
        return Assert.IsAssignableFrom<GraphicsNode>(node[0]);
    }

    [Fact]
    public void PooledResourceArraysClearReferencesOnGrowthResetReplacementAndDisposal()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var pool = new TrackingResourcePool();
        using var copy = new CanvasCommandBuffer(ArrayPool<byte>.Shared, pool);
        Assert.Empty(pool.Outstanding);
        void Record(int count)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            for (var i = 0; i < count; i++) scene.FillText("Retained string", (10, 20));
            scene.EndFrame();
            copy.CopyFrom(scene.Root.Commands);
        }
        Record(1);
        var first = Assert.Single(pool.Outstanding);
        Assert.All(first.Skip(copy.ResourceCount), value => Assert.Null(value));
        Record(40);
        Assert.Equal(1, pool.Returns);
        Assert.All(first, value => Assert.Null(value));
        var current = Assert.Single(pool.Outstanding);
        Assert.NotSame(first, current);
        Record(1);
        Assert.All(current.Skip(copy.ResourceCount), value => Assert.Null(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => copy.GetResource(new ResourceRef(copy.ResourceCount)));
        copy.Clear();
        Assert.All(current, value => Assert.Null(value));
        Record(1);
        Assert.Same(current, Assert.Single(pool.Outstanding));
        copy.Dispose();
        copy.Dispose();
        Assert.All(current, value => Assert.Null(value));
        Assert.Empty(pool.Outstanding);
        Assert.Equal(2, pool.Returns);
    }

    [Fact]
    public void VariablePayloadsImmediatelyFollowTheirHeaders()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        var start = scene.Root.Commands.Data.Length;
        Span<char> text = stackalloc char[] { 'O', 'd', 'd' };
        Span<nfloat> dashes = stackalloc nfloat[] { 2, 3, 5 };
        Span<GradientStop> stops = stackalloc GradientStop[] { new(0, Colors.Red), new(1, Colors.Blue) };
        scene.FillText(text, (10, 20));
        scene.SetLineDash(dashes);
        scene.SetFill(new LinearGradient((1, 2), (30, 40), stops));
        scene.FillRect((4, 5, 60, 70));
        text.Clear(); dashes.Clear(); stops.Clear();
        scene.EndFrame();

        var data = scene.Root.Commands.Data[start..];
        var textCommand = MemoryMarshal.Read<TextCommand>(data);
        Assert.Equal(3, textCommand.Length);
        data = data[Unsafe.SizeOf<TextCommand>()..];
        Assert.Equal("Odd", new string(MemoryMarshal.Cast<byte, char>(data[..(3 * sizeof(char))])));
        data = data[(3 * sizeof(char))..];
        var dashCommand = MemoryMarshal.Read<DashesCommand>(data);
        Assert.Equal(CanvasOp.Dashes, dashCommand.Op);
        Assert.Equal(3, dashCommand.Length);
        data = data[Unsafe.SizeOf<DashesCommand>()..];
        var dashBytes = 3 * Unsafe.SizeOf<nfloat>();
        Assert.Equal(new nfloat[] { 2, 3, 5 }, MemoryMarshal.Cast<byte, nfloat>(data[..dashBytes]).ToArray());
        data = data[dashBytes..];
        var gradient = MemoryMarshal.Read<FillLinearCommand>(data);
        Assert.Equal(CanvasOp.FillLinear, gradient.Op);
        Assert.Equal(2, gradient.Length);
        data = data[Unsafe.SizeOf<FillLinearCommand>()..];
        var stopBytes = 2 * Unsafe.SizeOf<GradientStop>();
        var copiedStops = MemoryMarshal.Cast<byte, GradientStop>(data[..stopBytes]);
        Assert.Equal(Colors.Red, copiedStops[0].Color);
        Assert.Equal(Colors.Blue, copiedStops[1].Color);
        data = data[stopBytes..];
        Assert.Equal(CanvasOp.FillRect, MemoryMarshal.Read<Command>(data).Op);
        Assert.Equal(Unsafe.SizeOf<FillRectCommand>(), data.Length);
    }

    [Fact]
    public void GrowthReturnsOldRentalsClearReusesCapacityAndDisposeReturnsTheLastRental()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var pool = new TrackingPool();
        using var copy = new CanvasCommandBuffer(pool);
        Assert.Equal(0, pool.Rents);
        void Record(int length)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            scene.FillText(new string('x', length).AsSpan(), (10, 20));
            scene.EndFrame();
            copy.CopyFrom(scene.Root.Commands);
            Assert.True(copy.Data.SequenceEqual(scene.Root.Commands.Data));
        }
        Record(3);
        Assert.Equal(1, pool.Rents);
        Record(8193);
        Assert.Equal(2, pool.Rents);
        Assert.Equal(1, pool.Returns);
        copy.Clear();
        Assert.Equal(1, pool.Returns);
        Record(3);
        Assert.Equal(2, pool.Rents);
        copy.Dispose();
        copy.Dispose();
        Assert.Equal(pool.Rents, pool.Returns);
        Assert.Empty(pool.Outstanding);
        Assert.Equal(0, copy.ResourceCount);
        Assert.Throws<ObjectDisposedException>(() => copy.Clear());
        Assert.Throws<ObjectDisposedException>(() => copy.CopyFrom(scene.Root.Commands));
    }

    [Fact]
    public void AppendingABorrowedPayloadSurvivesBufferGrowth()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        var first = scene.Root.Commands.Data.Length;
        var text = new string('z', 200);
        scene.FillText(text.AsSpan(), (10, 20));
        var second = scene.Root.Commands.Data.Length;
        var borrowed = MemoryMarshal.Cast<byte, char>(scene.Root.Commands.Data[(first + Unsafe.SizeOf<TextCommand>())..]);
        scene.FillText(borrowed, (30, 40));
        scene.EndFrame();
        var copied = MemoryMarshal.Cast<byte, char>(scene.Root.Commands.Data[(second + Unsafe.SizeOf<TextCommand>())..]);
        Assert.Equal(text, new string(copied));
    }

    [Fact]
    public void InlinePaintPayloadsSurviveRestoreAndControlCachedGroupReuse()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var options = new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false };
        void Frame(Color color, nfloat dash, bool redraw)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            Span<GradientStop> stops = stackalloc GradientStop[] { new(0, color), new(1, Colors.Blue) };
            scene.SetFill(new LinearGradient((0, 0), (100, 100), stops));
            scene.SetLineDash([dash, 3]);
            scene.Save();
            scene.SetLineDash([99, 100, 101]);
            scene.SetFill(new RadialGradient((10, 10), 20, stops));
            scene.Restore();
            using (var group = scene.Group(options))
            {
                Assert.Equal(redraw, group.NeedsRedraw);
                if (group.NeedsRedraw) scene.FillRect((0, 0, 10, 10));
            }
            scene.EndFrame();
            scene.AcceptFrame();
        }
        Frame(Colors.Red, 2, true);
        Frame(Colors.Red, 2, false);
        Frame(Colors.Green, 2, true);
        Frame(Colors.Green, 4, true);
        Frame(Colors.Green, 4, false);
    }

    [Fact]
    public void ReplayRejectsReentrantMutationAndReleasesItsBorrowAfterFailure()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        var count = scene.Root.Commands.Count;
        Assert.Throws<InvalidOperationException>(() => scene.Root.Commands.Replay(scene));
        Assert.Equal(count, scene.Root.Commands.Count);
        scene.FillRect((1, 2, 3, 4));
        scene.EndFrame();
    }

    [Fact]
    public void StringsAndImagesShareResourceIdsWhileTextSpansAreCopied()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var label = new string("Retained string".ToCharArray());
        var family = new string("Inter".ToCharArray());
        var image = new SoftwareImagePipeline().CreateImage();
        scene.BeginFrame((0, 0, 200, 200), 1);
        var buffer = scene.Root.Commands;
        var start = buffer.Data.Length;
        var firstId = buffer.ResourceCount;
        scene.SetFont(new Xui.Core.Canvas.Font(19, family));
        scene.FillText(label, (10, 20));
        scene.DrawImage(image, (1, 2, 3, 4));
        var beforeSpan = buffer.ResourceCount;
        Span<char> characters = stackalloc char[] { 'H', 'i' };
        scene.FillText(characters, (30, 40));
        characters.Fill('X');
        Assert.Equal(beforeSpan, buffer.ResourceCount);
        scene.EndFrame();

        var data = buffer.Data[start..];
        var font = MemoryMarshal.Read<FontCommand>(data);
        Assert.Equal(firstId, font.Family.Id);
        Assert.Same(family, buffer.GetResource(font.Family));
        Assert.Equal((nfloat)19, font.Size);
        data = data[Unsafe.SizeOf<FontCommand>()..];
        var text = MemoryMarshal.Read<TextResourceCommand>(data);
        Assert.Equal(CanvasOp.TextResource, text.Op);
        Assert.Equal(firstId + 1, text.Text.Id);
        Assert.Same(label, buffer.GetResource(text.Text));
        data = data[Unsafe.SizeOf<TextResourceCommand>()..];
        var picture = MemoryMarshal.Read<ImageCommand>(data);
        Assert.Equal(firstId + 2, picture.Image.Id);
        Assert.Same(image, buffer.GetResource(picture.Image));
        data = data[Unsafe.SizeOf<ImageCommand>()..];
        Assert.Equal(CanvasOp.Text, MemoryMarshal.Read<TextCommand>(data).Op);
        Assert.Equal(sizeof(int), Unsafe.SizeOf<ResourceRef>());
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<FontCommand>());

        using var copy = new CanvasCommandBuffer();
        copy.CopyFrom(buffer);
        buffer.Clear();
        Assert.Equal(0, buffer.ResourceCount);
        Assert.Same(label, copy.GetResource(text.Text));
        Assert.Same(image, copy.GetResource(picture.Image));
        // Replay into another recorder preserves string identity and copies the span payload again.
        scene.BeginFrame((0, 0, 200, 200), 1);
        var replayStart = buffer.Data.Length;
        copy.Replay(scene);
        scene.EndFrame();
        var replayTextOffset = replayStart + start + Unsafe.SizeOf<FontCommand>();
        var replayText = MemoryMarshal.Read<TextResourceCommand>(buffer.Data[replayTextOffset..]);
        Assert.Same(label, buffer.GetResource(replayText.Text));
    }

    [Fact]
    public void RestoreTruncatesSharedResourcesAndFontValuesStillControlReuse()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var image = new SoftwareImagePipeline().CreateImage();
        var temporaryImage = new SoftwareImagePipeline().CreateImage();
        var options = new GroupOptions { Key = new object(), ViewBacking = true, Bounds = (0, 0, 100, 100), HasUpdates = false };
        void Frame(nfloat size, bool redraw)
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            // Different string instances with equal content must not invalidate cached drawing.
            scene.SetFont(new Xui.Core.Canvas.Font(size, new string("Inter".ToCharArray())));
            scene.SetFill(new ImagePattern(image));
            scene.Save();
            scene.SetFont(new Xui.Core.Canvas.Font(99, "Temporary"));
            scene.SetStroke(new ImagePattern(temporaryImage));
            scene.Restore();
            scene.SetStroke(new ImagePattern(image));
            using (var group = scene.Group(options))
            {
                Assert.Equal(redraw, group.NeedsRedraw);
                if (group.NeedsRedraw) scene.FillRect((0, 0, 10, 10));
            }
            scene.EndFrame();
            var inherited = SingleChild(scene.Root).Inherited;
            Assert.Equal(4, inherited.ResourceCount);
            Assert.Same(image, inherited.GetResource(new ResourceRef(2)));
            Assert.Same(image, inherited.GetResource(new ResourceRef(3)));
            scene.AcceptFrame();
        }
        Frame(16, true);
        Frame(16, false);
        Frame(18, true);
    }

    [Fact]
    public void PackedRecordsCanBeReadByTheirCommonHeaderWithoutAlignmentPadding()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        var start = scene.Root.Commands.Data.Length;
        scene.Save();
        scene.FillRect((11, 22, 33, 44));
        scene.Restore();
        scene.EndFrame();

        var data = scene.Root.Commands.Data[start..];
        Assert.Equal(Unsafe.SizeOf<Command>(), Unsafe.SizeOf<SaveCommand>());
        Assert.Equal(3 * Unsafe.SizeOf<CanvasOp>() + 4 * Unsafe.SizeOf<nfloat>(), data.Length);
        Assert.Equal(CanvasOp.Save, MemoryMarshal.Read<Command>(data).Op);
        data = data[Unsafe.SizeOf<SaveCommand>()..];
        Assert.Equal(CanvasOp.FillRect, MemoryMarshal.Read<Command>(data).Op);
        Assert.Equal(new Rect(11, 22, 33, 44), MemoryMarshal.Read<FillRectCommand>(data).Bounds);
        data = data[Unsafe.SizeOf<FillRectCommand>()..];
        Assert.Equal(CanvasOp.Restore, MemoryMarshal.Read<Command>(data).Op);
        Assert.Equal(Unsafe.SizeOf<RestoreCommand>(), data.Length);
    }

    [Fact]
    public void CopySurvivesSourceClearAndReplaysAllOperationsLikeImmediateDrawing()
    {
        var image = new SoftwareImagePipeline().CreateImage();
        image.LoadPixels(2, 1, [0, 0, 255, 255, 0, 255, 0, 128]);
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        scene.BeginFrame((0, 0, 200, 200), 1);
        Draw(scene, image);
        scene.EndFrame();
        using var copy = new CanvasCommandBuffer();
        copy.CopyFrom(scene.Root.Commands);
        var count = copy.Count;
        copy.CopyFrom(copy);
        Assert.Equal(count, copy.Count);
        scene.Root.Commands.Clear();
        Assert.Empty(scene.Root.Commands.Data.ToArray());

        Assert.Equal(Render(context => Draw(context, image)), Render(copy.Replay));
    }

    [Fact]
    public void WarmRecordingDoesNotAllocatePerCommand()
    {
        using var stream = new MemoryStream();
        using var measure = new SvgDrawingContext((200, 200), stream, keepOpen: true);
        using var scene = new RetainedCanvasContext(measure);
        var image = new SoftwareImagePipeline().CreateImage();
        void Record()
        {
            scene.BeginFrame((0, 0, 200, 200), 1);
            for (var i = 0; i < 128; i++)
            {
                scene.Save();
                scene.SetFill(Colors.Red);
                scene.SetFont(new Xui.Core.Canvas.Font(16, "Inter"));
                scene.FillText("Packed text", (10, 20));
                scene.FillText("Copied text".AsSpan(), (10, 40));
                scene.DrawImage(image, (1, 2, 3, 4));
                scene.FillRect((i, 1, 30, 40));
                scene.Restore();
            }
            scene.EndFrame();
            scene.AcceptFrame();
        }
        for (var i = 0; i < 10; i++) Record();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10; i++) Record();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    private static string Render(Action<IContext> draw)
    {
        using var output = new MemoryStream();
        using (var svg = new SvgDrawingContext((200, 200), output, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
            draw(svg);
        output.Position = 0;
        return XDocument.Load(output).ToString();
    }

    private static void Draw(IContext context, IImage image)
    {
        context.Save();
        context.GlobalAlpha = (nfloat)0.7;
        context.LineWidth = 3;
        context.MiterLimit = 7;
        context.LineDashOffset = 2;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Bevel;
        Span<nfloat> dashes = stackalloc nfloat[] { 3, 5, 7 };
        context.SetLineDash(dashes);
        dashes.Fill(99);
        context.Translate((3, 4));
        context.Scale((2, 3));
        context.Rotate((nfloat)0.2);
        context.Transform(AffineTransform.Translate((5, 6)));
        context.SetTransform(AffineTransform.Scale((1, 2)));
        context.SetFilter(new Brightness((nfloat)0.6));
        context.SetFill(Colors.Red);
        context.SetStroke(Colors.Blue);
        context.FillRect((1, 2, 30, 40));
        context.StrokeRect((2, 3, 40, 50));
        context.BeginPath();
        context.MoveTo((1, 2));
        context.LineTo((3, 4));
        context.CurveTo((5, 6), (7, 8));
        context.CurveTo((9, 10), (11, 12), (13, 14));
        context.ArcTo((15, 16), (17, 18), 5);
        context.Arc((50, 60), 10, (nfloat)0.2, 2, Winding.CounterClockWise);
        context.Ellipse((70, 80), 12, 18, (nfloat)0.3, (nfloat)0.4, 3, Winding.CounterClockWise);
        context.ClosePath();
        context.Stroke();
        context.Fill(FillRule.EvenOdd);
        context.BeginPath();
        context.Rect((1, 2, 90, 100));
        context.RoundRect((3, 4, 70, 80), new CornerRadius(2, 4, 6, 8));
        context.Clip();
        context.SetFilter();
        Span<GradientStop> stops = stackalloc GradientStop[] { new(0, Colors.Red), new(1, Colors.Blue) };
        context.SetFill(new LinearGradient((1, 2), (30, 40), stops));
        context.FillRect((2, 3, 40, 50));
        context.SetStroke(new LinearGradient((3, 4), (50, 60), stops));
        context.StrokeRect((3, 4, 50, 60));
        context.SetFill(new RadialGradient((1, 2), 3, (40, 50), 60, stops));
        context.FillRect((4, 5, 60, 70));
        context.SetStroke(new RadialGradient((2, 3), 4, (50, 60), 70, stops));
        context.StrokeRect((5, 6, 70, 80));
        stops.Clear();
        context.SetFill(new ImagePattern(image, PatternRepeat.RepeatX));
        context.FillRect((1, 2, 30, 40));
        context.SetStroke(new ImagePattern(image, PatternRepeat.RepeatY));
        context.StrokeRect((2, 3, 40, 50));
        context.DrawImage(image, (10, 20, 30, 40), (nfloat)0.4);
        context.DrawImage(image, (0, 0, 1, 1), (20, 30, 40, 50), (nfloat)0.8);
        context.SetFill(Colors.Black);
        context.SetFont(new Xui.Core.Canvas.Font(18, "Inter", FontWeight.Bold));
        context.TextAlign = TextAlign.Center;
        context.TextBaseline = TextBaseline.Top;
        Span<char> text = stackalloc char[] { 'H', 'i' };
        context.FillText(text, (40, 50));
        text.Fill('X');
        context.FillText("Retained string", (40, 75));
        context.Restore();
    }

    private sealed class TrackingPool : ArrayPool<byte>
    {
        public int Rents;
        public int Returns;
        public HashSet<byte[]> Outstanding { get; } = new();

        public override byte[] Rent(int minimumLength)
        {
            var buffer = new byte[minimumLength];
            Array.Fill(buffer, (byte)0xCC);
            Outstanding.Add(buffer);
            Rents++;
            return buffer;
        }

        public override void Return(byte[] array, bool clearArray = false)
        {
            Assert.True(Outstanding.Remove(array), "The array must be returned exactly once to its owning pool.");
            Assert.True(clearArray);
            Array.Clear(array);
            Returns++;
        }
    }

    private sealed class TrackingResourcePool : ArrayPool<object?>
    {
        public int Returns;
        public HashSet<object?[]> Outstanding { get; } = new();

        public override object?[] Rent(int minimumLength)
        {
            var buffer = new object?[minimumLength];
            Array.Fill(buffer, new object()); // Simulate dirty storage from another borrower.
            Outstanding.Add(buffer);
            return buffer;
        }

        public override void Return(object?[] array, bool clearArray = false)
        {
            Assert.True(Outstanding.Remove(array));
            Assert.All(array, value => Assert.Null(value)); // Cleared before entering the pool.
            Returns++;
        }
    }
}
