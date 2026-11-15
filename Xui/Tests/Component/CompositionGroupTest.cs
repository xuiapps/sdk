using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;

namespace Xui.Tests.Component;

public class CompositionGroupTest
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    [Fact]
    public void ExportRequestsUnchangedContentAndPreservesExplicitNestingAndDrawOrder()
    {
        var key = new PrivateKey();
        var document = Render(context =>
        {
            context.FillRect((0, 0, 10, 10));
            using (var outer = context.Group(new GroupOptions { Key = key, HasUpdates = false }))
            {
                Assert.True(outer.NeedsRedraw);
                context.FillRect((10, 0, 10, 10));
                using (var inner = context.Group(new GroupOptions { Key = key, HasUpdates = false }))
                {
                    Assert.True(inner.NeedsRedraw);
                    context.FillRect((20, 0, 10, 10));
                }
                context.FillRect((30, 0, 10, 10));
            }
            context.FillRect((40, 0, 10, 10));
            using var empty = context.Group(new GroupOptions { Key = new PrivateKey() });
        });
        Assert.Equal(new[] { "rect", "g", "rect", "g" }, document.Root!.Elements().Select(e => e.Name.LocalName));
        var group = document.Root.Elements(Svg + "g").First();
        Assert.Equal(new[] { "rect", "g", "rect" }, group.Elements().Select(e => e.Name.LocalName));
        Assert.Single(group.Element(Svg + "g")!.Elements(Svg + "rect"));
        Assert.Empty(document.Root.Elements(Svg + "g").Last().Elements());
        Assert.All(document.Descendants(Svg + "g"), g => Assert.Null(g.Attribute("id")));
    }

    [Fact]
    public void GroupTransformClipAndPaintAreScopedAndBoundsDoNotTranslateTheOrigin()
    {
        var document = Render(context =>
        {
            context.SetFill(Colors.Red);
            using (context.Group(new GroupOptions
            {
                Key = new object(), Bounds = (2, 3, 50, 60), ClipToBounds = true,
                Transform = AffineTransform.Translate((7, 9))
            }))
            {
                context.SetFill(Colors.Blue);
                context.Save();
                context.Scale((2, 2));
                context.FillRect((0, 0, 80, 90));
                context.Restore();
            }
            context.FillRect((0, 0, 10, 10));
        });
        var outer = Assert.Single(document.Root!.Elements(Svg + "g"));
        Assert.Equal("translate(7,9)", (string?)outer.Attribute("transform"));
        Assert.Single(document.Descendants(Svg + "clipPath"));
        var inside = Assert.Single(outer.Descendants(Svg + "rect"));
        var outside = Assert.Single(document.Root.Elements(Svg + "rect"));
        Assert.NotEqual((string?)inside.Attribute("fill"), (string?)outside.Attribute("fill"));
        Assert.Contains(inside.Ancestors(), e => e.Attribute("clip-path") is not null);
        Assert.Null(outside.Attribute("transform"));
        Assert.Null(outside.Attribute("clip-path"));
        Assert.Equal("0", (string?)inside.Attribute("x"));
    }

    [Fact]
    public void InvalidOptionsDoNotOpenAGroupOrCorruptFollowingDrawing()
    {
        var document = Render(context =>
        {
            Assert.Throws<ArgumentNullException>(() => { context.Group(default); });
            Assert.Throws<ArgumentException>(() => { context.Group(new GroupOptions { Key = 1 }); });
            Assert.Throws<ArgumentException>(() => { context.Group(new GroupOptions { Key = new object(), ClipToBounds = true }); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { context.Group(new GroupOptions { Key = new object(), Bounds = (0, 0, -1, 1) }); });
            using var group = context.Group(new GroupOptions { Key = new object() });
            context.FillRect((0, 0, 10, 10));
        });
        Assert.Single(document.Descendants(Svg + "g"));
        Assert.Single(document.Descendants(Svg + "rect"));
    }

    [Fact]
    public void SaveRestoreCannotEscapeTheGroupAndUnbalancedSavesCanBeRecovered()
    {
        Render(context =>
        {
            var group = context.Group(new GroupOptions { Key = new object() });
            Assert.Throws<InvalidOperationException>(() => context.Restore());
            context.Save();
            Assert.Throws<InvalidOperationException>(() => context.EndGroup());
            context.Restore();
            group.Dispose();
            group.Dispose();
            Assert.Throws<InvalidOperationException>(() => context.EndGroup());
        });
    }

    [Fact]
    public void ScopeClosesOnExceptionAndSiblingStateIsRestored()
    {
        var document = Render(context =>
        {
            Assert.Throws<InvalidOperationException>((Action)(() =>
            {
                using var group = context.Group(new GroupOptions { Key = new object() });
                context.Translate((20, 30));
                context.FillRect((0, 0, 10, 10));
                throw new InvalidOperationException();
            }));
            context.FillRect((0, 0, 10, 10));
        });
        Assert.Single(document.Root!.Elements(Svg + "rect"));
    }

    [Fact]
    public void ImmediateBackendAlwaysDrawsEvenWhenViewReportsNoUpdates()
    {
        var document = Render(context =>
        {
            var immediate = new ImmediateCompositionContext(context);
            using (var group = immediate.Group(new GroupOptions
            {
                Key = new object(), HasUpdates = false, Transform = AffineTransform.Translate((3, 4))
            }))
            {
                Assert.True(group.NeedsRedraw);
                context.FillRect((0, 0, 10, 10));
            }
            context.FillRect((0, 0, 10, 10));
        });
        Assert.Single(document.Root!.Elements(Svg + "rect"));
        Assert.Single(document.Root.Elements(Svg + "g"));
    }

    [Fact]
    public void ViewCanReadInvalidationInsideRenderAndFreshExportStillDrawsCleanView()
    {
        var view = new GroupView();
        view.InvalidateRender();
        Render(view.Render);
        Assert.True(view.LastHasUpdates);
        var export = Render(view.Render);
        Assert.False(view.LastHasUpdates);
        Assert.Equal(2, view.DrawCount);
        Assert.Single(export.Descendants(Svg + "rect"));

        view.InvalidateRender();
        Render(view.Render);
        Assert.True(view.LastHasUpdates);
    }

    [Fact]
    public void NewOptionsRequestDrawingAndUseIdentityTransform()
    {
        var options = new GroupOptions { Key = new object() };
        Assert.True(options.HasUpdates);
        Assert.True(options.Transform.IsIdentity);
        Assert.False(options.ClipToBounds);
        Assert.Null(options.Bounds);
    }

    [Fact]
    public void FailedRenderAndInvalidationDuringRenderingRemainDirty()
    {
        var view = new GroupView { Fail = true };
        Assert.Throws<InvalidOperationException>(() => Render(view.Render));
        Assert.NotEqual(0, (int)(view.Flags & View.ViewFlags.RenderChanged));
        view.Fail = false;
        Render(view.Render);
        Assert.True(view.LastHasUpdates);

        view.InvalidateDuringRender = true;
        Render(view.Render);
        Assert.NotEqual(0, (int)(view.Flags & View.ViewFlags.RenderChanged));
        view.InvalidateDuringRender = false;
        Render(view.Render);
        Assert.True(view.LastHasUpdates);
        Render(view.Render);
        Assert.False(view.LastHasUpdates);
    }

    private static XDocument Render(Action<IContext> render)
    {
        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext((200, 200), stream, keepOpen: true))
            render(context);
        stream.Position = 0;
        return XDocument.Load(stream);
    }

    private sealed class PrivateKey
    {
        public override string ToString() => throw new InvalidOperationException("Keys must not be serialized.");
        public override bool Equals(object? obj) => throw new InvalidOperationException("Keys use reference identity.");
        public override int GetHashCode() => throw new InvalidOperationException("Keys use reference identity.");
    }

    private sealed class GroupView : View
    {
        public GroupView() => Frame = (0, 0, 100, 100);

        public bool LastHasUpdates;
        public int DrawCount;
        public bool Fail;
        public bool InvalidateDuringRender;

        protected override void RenderCore(IContext context)
        {
            LastHasUpdates = HasRenderUpdates;
            using var group = context.Group(new GroupOptions { Key = this, HasUpdates = HasRenderUpdates });
            if (group.NeedsRedraw)
            {
                DrawCount++;
                context.FillRect((0, 0, 10, 10));
                if (Fail) throw new InvalidOperationException();
                if (InvalidateDuringRender) InvalidateRender();
            }
        }
    }
}
