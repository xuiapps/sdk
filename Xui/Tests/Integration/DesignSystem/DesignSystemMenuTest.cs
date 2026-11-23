using Xui.Tests.Integration.TestApp;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Abstract.Events;
using Xui.DevKit.UI.Design;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Middleware.EmulatorLegacy.Devices;
using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.DesignSystem;

public partial class DesignSystemMenuTest
{
    [Fact]
    public void ColorAndRoundnessGallery()
    {
        using var scene = new Scene();
        scene.App.MarkdownParagraph("# Color and roundness\n\nThe hue ring preserves the selected shade; the inner triangle mixes white (upper left), black (lower left), and hue (right). Rounded tracks and their inset highlights share corner centers.");
        var wheel = scene.Find("DesignSystem.Hue");
        var center = wheel.Parent!.LocalToGlobal(wheel.Frame.Center);
        var radius = (nfloat)115 * .83f - 12;
        var halfHeight = nfloat.Sqrt(3) * radius / 2;
        foreach (var (name, white, black) in new[] { ("Vivid", .02f, .05f), ("Light", .6f, .05f), ("Dark", .02f, .6f) })
        {
            var hue = 1 - white - black;
            var point = center + new Vector(-radius / 2 + hue * radius * 1.5f, -halfHeight + (black + hue / 2) * halfHeight * 2);
            scene.App.PointerDown(point);
            scene.App.PointerUp(point);
            scene.Advance(16);
            scene.App.Snapshot(name);
        }
        // Restore a strong accent for comparing geometry.
        var vivid = center + new Vector(radius - 8, 0);
        scene.App.PointerDown(vivid);
        scene.App.PointerUp(vivid);
        scene.Advance(16);
        var shape = scene.Find("DesignSystem.Shape");
        var names = new[] { "Square", "Subtle", "Round", "Pill", "Soft" };
        for (var index = 0; index < names.Length; index++)
        {
            scene.Reveal(shape);
            scene.TapCell(shape, index, 5);
            scene.App.Snapshot(names[index]);
        }
    }

    [Fact]
    public void NestedSegmentsHaveConcentricCornersForEveryShape()
    {
        using var scene = new Scene();
        var shape = scene.Find("DesignSystem.Shape");
        var design = (IDesignSystem)shape.GetService(typeof(IDesignSystem))!;
        XNamespace ns = "http://www.w3.org/2000/svg";
        foreach (var index in new[] { 0, 1, 2, 3, 4 })
        {
            scene.Reveal(shape);
            scene.TapCell(shape, index, 5);
            var paths = RenderView(shape).Descendants(ns + "path").Take(2).ToArray();
            static double StartX(XElement path) => double.Parse(path.Attribute("d")!.Value.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
            var outer = nfloat.Max(0, design.Shape.Large.TopLeft - design.Spacing.Passive.L);
            var inner = nfloat.Max(0, outer - 4);
            var width = (shape.Frame.Width - 8) / 5;
            Assert.Equal((double)(shape.Frame.Left + outer), StartX(paths[0]), precision: 2);
            Assert.Equal((double)(shape.Frame.Left + 4 + index * width + inner), StartX(paths[1]), precision: 2);
        }
    }

    [Fact]
    public void SegmentHighlightStaysSingleAndRetargetsFromItsVisiblePosition()
    {
        using var scene = new Scene();
        var tabs = scene.Find("DesignSystem.Tabs");

        var start = HighlightX(tabs);
        var destination = start + (double)(tabs.Frame.Width - 8) / 2;
        var next = scene.Cell(tabs, 1, 2);
        scene.App.PointerDown(next);
        scene.Advance(20);
        Assert.Equal(start, HighlightX(tabs));
        scene.App.PointerUp(next);
        Assert.Equal(start, HighlightX(tabs));
        scene.Advance(16);
        Assert.True(HighlightX(tabs) > start, "The first animation frame should already move the highlight.");
        scene.Advance(64);
        var middle = HighlightX(tabs);
        Assert.InRange(middle, start + 1, destination - 1);

        var back = scene.Cell(tabs, 0, 2);
        scene.App.PointerDown(back);
        Assert.Equal(middle, HighlightX(tabs));
        scene.App.PointerUp(back);
        Assert.Equal(middle, HighlightX(tabs));
        scene.Advance(16);
        Assert.InRange(HighlightX(tabs), start + 0.01, middle - 0.01);
        scene.Advance(164);
        Assert.Equal(start, HighlightX(tabs));

        scene.TapCell(tabs, 1, 2, settle: false);
        scene.Advance(164);
        Assert.Equal(destination, HighlightX(tabs), precision: 2);
    }

    [Fact]
    public void RotatingTheWheelDoesNotInvalidateLayout()
    {
        using var scene = new Scene();
        var mobile = scene.Find("DesignSystem.Mobile");
        var wheel = scene.Find("DesignSystem.Hue");
        var center = wheel.Parent!.LocalToGlobal(wheel.Frame.Center);
        var design = Assert.IsAssignableFrom<IDesignSystem>(wheel.GetService(typeof(IDesignSystem)));
        var typography = design.Typography;
        var spacing = design.Spacing;
        var shape = design.Shape;
        var motion = design.Motion;
        var originalColor = design.Colors.Primary.Background;
        scene.App.PointerDown(center + new Vector(105, 0));
        for (var step = 1; step <= 16; step++)
        {
            var angle = (nfloat)step * nfloat.Pi / 16;
            scene.App.PointerMove(center + new Vector(nfloat.Cos(angle) * 105, nfloat.Sin(angle) * 105));
            var dirty = Descendants(mobile).Prepend(mobile).Where(view =>
                (view.Flags & (View.ViewFlags.MeasureChanged | View.ViewFlags.ArrangeChanged | View.ViewFlags.DescendantArrangeChanged)) != 0).ToArray();
            Assert.True(dirty.Length == 0, $"Color update invalidated layout in {dirty.Length} views: " +
                string.Join(", ", dirty.Select(view => view.Id ?? view.GetType().Name)));
            Assert.Same(typography, design.Typography);
            Assert.Same(spacing, design.Spacing);
            Assert.Same(shape, design.Shape);
            Assert.Same(motion, design.Motion);
            scene.Advance(8);
        }
        scene.App.PointerUp(center - new Vector(105, 0));
        Assert.NotEqual(originalColor, design.Colors.Primary.Background);
        Assert.Equal(0, TabPosition(scene.Find("DesignSystem.Tabs")), precision: 3);
    }

    [Fact]
    public void PaletteContinuesScrollingAfterRelease()
    {
        using var scene = new Scene();
        var scroll = scene.Scroll;
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Left + 5, scroll.Frame.Center.Y + 80));
        scene.App.PointerDown(start);
        scene.Advance(20);
        scene.App.PointerMove(start - new Vector(0, 50));
        scene.Advance(20);
        scene.App.PointerMove(start - new Vector(0, 110));
        scene.App.PointerUp(start - new Vector(0, 110));
        var released = Translation(scroll);
        Assert.True(released < -50);
        scene.Advance(16); // Initialize the decay on the first animation tick.
        scene.Advance(80);
        Assert.True(Translation(scroll) < released - 10, "The palette should continue scrolling after release.");
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void ScrollbarReachesBothViewportEnds(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant: variant);
        var scroll = scene.Scroll;
        static string[] Bar(View view)
        {
            XNamespace ns = "http://www.w3.org/2000/svg";
            return Assert.Single(RenderView(view).Descendants(ns + "path"),
                path => (string?)path.Attribute("fill-opacity") == "0.35")
                .Attribute("d")!.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        static double Number(string value) => double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(0, Number(Bar(scroll)[2]), precision: 2);
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Left + 5, scroll.Frame.Center.Y));
        scene.App.PointerDown(start);
        scene.Advance(20);
        var end = start - new Vector(0, 10000);
        scene.App.PointerMove(end);
        scene.Advance(200); // Let release velocity expire, then settle any overscroll.
        scene.App.PointerUp(end);
        scene.Advance(16);
        scene.Advance(2000);
        var bar = Bar(scroll);
        var bottom = Number(bar[Array.IndexOf(bar, "V") + 1]) + (double)scroll.ScrollbarWidth / 2;
        Assert.Equal((double)scroll.Frame.Height, bottom, precision: 2);
    }

    [Fact]
    public void PullingPastTheTopSpringsBack()
    {
        using var scene = new Scene();
        var scroll = scene.Scroll;
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Left + 5, scroll.Frame.Center.Y));
        scene.App.PointerDown(start);
        scene.Advance(20);
        scene.App.PointerMove(start + new Vector(0, 160));
        var pulled = Translation(scroll);
        Assert.InRange((double)pulled, 1, 159);
        scene.Advance(200); // Pause to isolate spring return from fling velocity.
        scene.App.PointerUp(start + new Vector(0, 160));
        scene.Advance(16);
        scene.Advance(80);
        Assert.InRange((double)Translation(scroll), 0, (double)pulled - 1);
        scene.Advance(1500);
        Assert.InRange((double)Translation(scroll), -0.01, 0.01);
    }

    [Fact]
    public void ScrollingAcrossOptionsDoesNotChangeTheDesign()
    {
        using var scene = new Scene();
        var harmony = scene.Find("DesignSystem.Harmony");
        scene.Reveal(harmony);
        var design = Assert.IsAssignableFrom<IDesignSystem>(harmony.GetService(typeof(IDesignSystem)));
        var changes = 0;
        design.Changed += _ => changes++;
        var start = scene.Cell(harmony, 3, 5);
        scene.App.PointerDown(start);
        Assert.Equal(0, changes);
        scene.Advance(20);
        scene.App.PointerMove(start - new Vector(0, 105));
        scene.Advance(200);
        scene.App.PointerUp(start - new Vector(0, 105));
        scene.Advance(1000);
        Assert.Equal(0, changes);
        scene.Reveal(harmony);
        scene.TapCell(harmony, 3, 5);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void TabsAnimateReverseAndLeaveOnlyTheSelectedPageInteractive()
    {
        using var scene = new Scene();
        var tabs = scene.Find("DesignSystem.Tabs");
        var preview = scene.Find("DesignSystem.PreviewScroll");
        Assert.False(preview.HitTest(preview.Frame.Center));
        scene.TapCell(tabs, 1, 2, settle: false);
        scene.Advance(60);
        var middle = scene.App.Render();
        Assert.False(preview.HitTest(preview.Frame.Center));
        Assert.False(scene.Scroll.HitTest(scene.Scroll.Frame.Center));
        // Reverse before completion; the indicator and pages must return to their settled positions.
        scene.TapCell(tabs, 0, 2);
        Assert.True(scene.Scroll.HitTest(scene.Scroll.Frame.Center));
        Assert.False(preview.HitTest(preview.Frame.Center));
        scene.TapCell(tabs, 1, 2);
        Assert.True(preview.HitTest(preview.Frame.Center));
        Assert.False(scene.Scroll.HitTest(scene.Scroll.Frame.Center));
        Assert.NotEqual(middle, scene.App.Render());
        var button = Assert.IsAssignableFrom<Button>(scene.Find("DesignSystem.PrimaryAction"));
        scene.Tap(button);
        Assert.Equal("Your next adventure is planned.", Assert.IsAssignableFrom<Label>(scene.Find("DesignSystem.ActionStatus")).Text);
    }

    [Theory]
    [InlineData(320)]
    [InlineData(430)]
    public void PreviewMarginsContainTextAndControlsAcrossDensityChanges(int width)
    {
        using var scene = new Scene(width: width);
        var buttonHeights = new List<nfloat>();
        var selectorHeights = new List<nfloat>();
        var toggleHeights = new List<nfloat>();
        var checkboxHeights = new List<nfloat>();
        foreach (var density in new[] { 0, 1, 2 })
        {
            scene.TapCell(scene.Find("DesignSystem.Tabs"), 0, 2);
            var sizing = scene.Find("DesignSystem.Sizing");
            scene.Reveal(sizing);
            scene.TapCell(sizing, density, 3);
            selectorHeights.Add(sizing.Frame.Height);
            Assert.Equal(3, Descendants(scene.Scroll).OfType<Card>().Count());
            Assert.True(sizing.Frame.Height < scene.Find("DesignSystem.Surface").Frame.Height,
                "Single-line choices should be shorter than two-line surface choices.");
            scene.TapCell(scene.Find("DesignSystem.Tabs"), 1, 2);
            var preview = scene.Find("DesignSystem.PreviewScroll");
            var textLeft = Assert.IsAssignableFrom<Card>(scene.Find("DesignSystem.HeroCard")).Content!.Frame.Left;
            Assert.All(Descendants(preview).OfType<Label>().Take(3), label =>
                Assert.Equal((double)textLeft, (double)label.Frame.Left, precision: 2));
            buttonHeights.Add(scene.Find("DesignSystem.PrimaryAction").Frame.Height);
            toggleHeights.Add(Descendants(preview).OfType<Toggle>().Single().Frame.Height);
            checkboxHeights.Add(Descendants(preview).OfType<Checkbox>().Single().Frame.Height);
            foreach (var card in Descendants(preview).OfType<Card>())
            {
                Assert.True(card.Frame.Left >= 16);
                Assert.True(card.Frame.Right <= width - 16 + 0.01f);
                Assert.Equal((nfloat)12, card.ShadowExtent);
                Assert.Equal((nfloat)0.06f, card.ShadowColor.Alpha);
                var heading = Assert.IsAssignableFrom<Label>(card.Parent![0]);
                Assert.True(heading.Frame.Bottom + 10 <= card.Frame.Top + 0.01f);
                Assert.Equal((double)card.Content!.Frame.Left, (double)heading.Frame.Left, precision: 2);
                foreach (var child in Descendants(card).Where(v => v is Label or Button or Toggle or Checkbox))
                {
                    Assert.True(child.Frame.Left >= card.Frame.Left + 1, child.Id);
                    Assert.True(child.Frame.Right <= card.Frame.Right - 1, $"{child.Id}: {child.Frame}");
                    Assert.True(child.Frame.Bottom <= card.Frame.Bottom + 0.01f, child.Id);
                }
            }
            var rows = Descendants(preview).Where(v => v.Count == 2 && v[0] is Label && v[1] is Toggle or Checkbox);
            Assert.All(rows, row => Assert.True(row[0].Frame.Right + 8 <= row[1].Frame.Left));
        }
        Assert.True(buttonHeights[2] > buttonHeights[0], "Mobile density should enlarge the live preview's controls.");
        Assert.True(selectorHeights[0] < selectorHeights[1] && selectorHeights[1] < selectorHeights[2]);
        Assert.True(toggleHeights[0] < toggleHeights[1] && toggleHeights[1] < toggleHeights[2]);
        Assert.True(checkboxHeights[0] < checkboxHeights[1] && checkboxHeights[1] < checkboxHeights[2]);
    }

    [Theory]
    [InlineData(320, 0.19, false)]
    [InlineData(320, 0.21, true)]
    [InlineData(430, 0.19, false)]
    [InlineData(430, 0.21, true)]
    public void SlowSwipesCommitAtAboutOneFifthOfThePage(int width, double distance, bool commits)
    {
        using var scene = new Scene(width: width);
        var tabs = scene.Find("DesignSystem.Tabs");
        foreach (var initial in new[] { 0, 1 })
        {
            scene.TapCell(tabs, initial, 2);
            var page = scene.Find(initial == 0 ? "DesignSystem.PaletteScroll" : "DesignSystem.PreviewScroll");
            var start = page.Parent!.LocalToGlobal(new Point(initial == 0 ? page.Frame.Right - 32 : page.Frame.Left + 40, page.Frame.Top + 100));
            scene.App.PointerDown(start);
            Point end = start;
            for (var step = 1; step <= 5; step++)
            {
                scene.Advance(100);
                end = start + new Vector(page.Frame.Width * (nfloat)distance * step / 5 * (initial == 0 ? -1 : 1), 0);
                scene.App.PointerMove(end);
            }
            scene.Advance(250); // Commit on distance even after the release velocity has expired.
            scene.App.PointerUp(end);
            scene.Advance(180);
            Assert.Equal(commits ? 1 - initial : initial, TabPosition(tabs), precision: 3);
        }
    }

    [Fact]
    public void EveryShapePresetUpdatesTheSegmentTrackAndHighlight()
    {
        using var scene = new Scene();
        var tabs = scene.Find("DesignSystem.Tabs");
        var shape = scene.Find("DesignSystem.Shape");
        var design = (IDesignSystem)tabs.GetService(typeof(IDesignSystem))!;
        XNamespace ns = "http://www.w3.org/2000/svg";
        foreach (var index in new[] { 0, 1, 2, 3, 4 })
        {
            scene.Reveal(shape);
            scene.TapCell(shape, index, 5);
            var glass = Assert.Single(RenderView(scene.Find("DesignSystem.Mobile")).Descendants(ns + "rect"),
                r => (string?)r.Attribute("data-xui-material") == "LiquidGlass");
            var highlight = RenderView(tabs).Descendants(ns + "path").First();
            var trackRadius = index == 4 ? tabs.Frame.Height / 2 : nfloat.Min(design.Shape.Full.TopLeft, tabs.Frame.Height / 2);
            var highlightRadius = nfloat.Max(0, trackRadius - 4);
            static double StartX(XElement path) => double.Parse(path.Attribute("d")!.Value.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal((double)trackRadius, (double?)glass.Attribute("rx") ?? 0, precision: 2);
            Assert.Equal((double)(tabs.Frame.Left + 4 + highlightRadius), StartX(highlight), precision: 2);
        }
    }

    [Fact]
    public void SurfacePresetsTintThePageAndCardsBeneathGlassImmediately()
    {
        using var scene = new Scene();
        var mobile = scene.Find("DesignSystem.Mobile");
        var surface = scene.Find("DesignSystem.Surface");
        XNamespace ns = "http://www.w3.org/2000/svg";
        var fills = new List<(string? Page, string? Card)>();
        foreach (var index in new[] { 0, 1, 2, 3, 4 })
        {
            scene.Reveal(surface);
            scene.TapCell(surface, index, 5);
            var rectangles = RenderView(mobile).Descendants(ns + "rect").ToArray();
            var page = Assert.Single(rectangles, r => (double?)r.Attribute("width") == (double)mobile.Frame.Width &&
                (double?)r.Attribute("height") == (double)scene.App.Window.DisplayArea.Height && r.Attribute("fill") is not null);
            Assert.Single(rectangles, r => (string?)r.Attribute("data-xui-material") == "LiquidGlass");
            var paletteCard = scene.Find("DesignSystem.Section.Palette");
            var card = Assert.Single(RenderView(paletteCard).Descendants(ns + "rect"), r =>
                (double?)r.Attribute("width") == (double)paletteCard.Frame.Width &&
                (double?)r.Attribute("height") == (double)paletteCard.Frame.Height &&
                (string?)r.Attribute("fill-opacity") == "1" && r.Attribute("filter") is null);
            fills.Add(((string?)page.Attribute("fill"), (string?)card.Attribute("fill")));
        }
        Assert.NotEqual(fills[0].Page, fills[1].Page);
        Assert.NotEqual(fills[0].Page, fills[3].Page);
        Assert.Equal(fills[0].Page, fills[2].Page);
        Assert.Equal(fills[0].Page, fills[4].Page);
        Assert.NotEqual(fills[0].Card, fills[2].Card);
        Assert.NotEqual(fills[0].Card, fills[4].Card);
        Assert.Equal(fills[0].Card, fills[1].Card);
        Assert.Equal(fills[0].Card, fills[3].Card);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TabMotionIsContinuousAndIndependentOfThemePreset(int preset)
    {
        using var scene = new Scene();
        var motion = scene.Find("DesignSystem.Motion");
        scene.Reveal(motion);
        scene.TapCell(motion, preset, 4);
        var tabs = scene.Find("DesignSystem.Tabs");
        scene.TapCell(tabs, 1, 2, settle: false);
        var preview = scene.Find("DesignSystem.PreviewScroll");
        Assert.False(preview.HitTest(preview.Frame.Center));
        Assert.False(scene.Scroll.HitTest(scene.Scroll.Frame.Center));
        var previous = TabPosition(tabs);
        Assert.True(previous > 0);
        for (var elapsed = 24; elapsed <= 176; elapsed += 8)
        {
            scene.Advance(8);
            var position = TabPosition(tabs);
            Assert.True(position > previous, "Every 120 Hz frame must advance, without sampled-curve plateaus.");
            // SVG coordinates are rounded to hundredths of a pixel.
            var expected = (double)Xui.Core.Animation.Easing.EaseInOutCubic((nfloat)elapsed / 180);
            Assert.InRange(position, expected - 0.0001, expected + 0.0001);
            previous = position;
        }
        scene.Advance(4);
        Assert.Equal(1, TabPosition(tabs), precision: 3);
        Assert.True(preview.HitTest(preview.Frame.Center));
    }

    [Fact]
    public void HorizontalDragMovesBothPagesAndIndicatorThenSnapsInEitherDirection()
    {
        using var scene = new Scene();
        var tabs = scene.Find("DesignSystem.Tabs");
        var scroll = scene.Scroll;
        var width = scroll.Frame.Width;
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Right - 32, scroll.Frame.Top + 100));
        scene.App.PointerDown(start);
        scene.Advance(40);
        scene.App.PointerMove(start - new Vector(width * 0.04f, 0));
        Assert.Equal(0.04, TabPosition(tabs), precision: 3);
        var transforms = XDocument.Parse(scene.App.Render()).Descendants()
            .Select(element => (string?)element.Attribute("transform"))
            .Where(value => value is not null && value.StartsWith("translate(") && value.EndsWith(",0)"))
            .Select(value => double.Parse(value![10..^3], System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        Assert.Contains(transforms, x => Math.Abs(x + (double)width * 0.04) < 0.01);
        Assert.Contains(transforms, x => Math.Abs(x - (double)width * 0.96) < 0.01);
        Assert.False(scroll.HitTest(scroll.Frame.Center));
        scene.Advance(150); // Holding still must not animate the pages away from the finger.
        Assert.Equal(0.04, TabPosition(tabs), precision: 3);
        scene.App.PointerUp(start - new Vector(width * 0.04f, 0));
        scene.Advance(180);
        Assert.Equal(0, TabPosition(tabs), precision: 3);
        Assert.True(scroll.HitTest(scroll.Frame.Center));

        scene.App.PointerDown(start);
        scene.Advance(40);
        scene.App.PointerMove(start - new Vector(width * 0.7f, 0));
        Assert.Equal(0.7, TabPosition(tabs), precision: 3);
        scene.Advance(150);
        scene.App.PointerUp(start - new Vector(width * 0.7f, 0));
        scene.Advance(180);
        var preview = scene.Find("DesignSystem.PreviewScroll");
        Assert.Equal(1, TabPosition(tabs), precision: 3);
        Assert.True(preview.HitTest(preview.Frame.Center));

        start = preview.Parent!.LocalToGlobal(new Point(preview.Frame.Left + 40, preview.Frame.Top + 100));
        scene.App.PointerDown(start);
        scene.Advance(40);
        scene.App.PointerMove(start + new Vector(width * 0.7f, 0));
        Assert.Equal(0.3, TabPosition(tabs), precision: 3);
        scene.Advance(150);
        scene.App.PointerUp(start + new Vector(width * 0.7f, 0));
        scene.Advance(180);
        Assert.Equal(0, TabPosition(tabs), precision: 3);
        Assert.True(scroll.HitTest(scroll.Frame.Center));
    }

    [Fact]
    public void ShortFlickChangesTabAndTouchingAnAnimationResumesItsDestination()
    {
        using var scene = new Scene();
        var tabs = scene.Find("DesignSystem.Tabs");
        var scroll = scene.Scroll;
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Right - 32, scroll.Frame.Top + 100));
        scene.App.PointerDown(start);
        scene.Advance(16);
        var end = start - new Vector(scroll.Frame.Width * 0.12f, 0);
        scene.App.PointerMove(end);
        scene.App.PointerUp(end);
        scene.Advance(40);
        var position = TabPosition(tabs);
        scene.App.PointerDown(start);
        scene.Advance(50);
        Assert.Equal(position, TabPosition(tabs));
        scene.App.PointerUp(start);
        scene.Advance(180);
        Assert.Equal(1, TabPosition(tabs), precision: 3);
    }

    [Fact]
    public void CancelledDragReturnsToTheOriginalTabAndReenablesItsControls()
    {
        using var scene = new Scene();
        var scroll = scene.Scroll;
        var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Right - 32, scroll.Frame.Top + 100));
        scene.App.PointerDown(start);
        scene.Advance(16);
        var end = start - new Vector(scroll.Frame.Width * 0.7f, 0);
        scene.App.PointerMove(end);
        scene.App.Touches(new Touch { Index = 0, Phase = TouchPhase.Cancel, Position = end });
        scene.Advance(180);
        Assert.Equal(0, TabPosition(scene.Find("DesignSystem.Tabs")), precision: 3);
        Assert.True(scroll.HitTest(scroll.Frame.Center));
    }

    [Fact]
    public void PageSwipeLeavesTheNavigationBackEdgeAvailable()
    {
        using var scene = new Scene();
        var mobile = scene.Find("DesignSystem.Mobile");
        Assert.True(mobile.TryFindParent<Xui.DevKit.UI.Mobile.Navigation.NavigationView>(out var navigation));
        var start = mobile.Parent!.LocalToGlobal(new Point(mobile.Frame.Left + 10, mobile.Frame.Top + 100));
        scene.App.PointerDown(start);
        scene.Advance(16);
        var end = start + new Vector(mobile.Frame.Width * 0.8f, 0);
        scene.App.PointerMove(end);
        scene.Advance(16);
        scene.App.PointerUp(end);
        scene.Advance(16);
        scene.Advance(400);
        Assert.False(navigation.CanGoBack);
    }

    [Theory]
    [InlineData(TestRuntimeVariant.IPhoneEmulator)]
    [InlineData(TestRuntimeVariant.AndroidPunchHoleEmulator)]
    public void DesignSystemLiveUpdates(TestRuntimeVariant variant)
    {
        using var scene = new Scene(variant: variant);
        scene.App.SvgFontResolver = new EmbeddedFonts();
        scene.App.MarkdownHeading("Design system — edit, preview, interact", 1);
        scene.App.MarkdownParagraph("The mobile editor uses vector choices and moving selection indicators. " +
            "Scroll inertia is delivered through the page animation loop; Apple profiles also spring back at the edges. " +
            "The palette and preview retain their scroll positions and share a single live design system. " +
            "Images and fonts are embedded in the SVG so these snapshots remain self-contained.");
        scene.App.Snapshot("Palette");
        var paletteSvg = XDocument.Parse(scene.App.Render());
        XNamespace svgNamespace = "http://www.w3.org/2000/svg";
        XNamespace xlink = "http://www.w3.org/1999/xlink";
        var glass = Assert.Single(paletteSvg.Descendants(svgNamespace + "rect"),
            rect => (string?)rect.Attribute("data-xui-material") == "LiquidGlass");
        Assert.True((double)scene.Scroll.Frame.Bottom > (double)glass.Attribute("y")!,
            "The scroll viewport must extend beneath the floating material.");
        var images = paletteSvg.Descendants(svgNamespace + "image").ToArray();
        Assert.Equal(2, images.Length);
        var wheelImage = Assert.Single(images, image => (string?)image.Attribute(xlink + "href") ==
            "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "ColorWheel.png"))));
        Assert.Equal("data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "Assets", "ColorWheel.png"))), (string?)wheelImage.Attribute(xlink + "href"));
        var hue = scene.Find("DesignSystem.Hue");
        var design = Assert.IsAssignableFrom<IDesignSystem>(hue.GetService(typeof(IDesignSystem)));
        var original = design.Colors.Primary.Background;
        var center = hue.Parent!.LocalToGlobal(hue.Frame.Center);
        scene.App.PointerDown(center + new Vector(105, 0));
        scene.Advance(20);
        scene.App.PointerMove(center + new Vector(0, 105));
        scene.App.PointerUp(center + new Vector(0, 105));
        scene.Advance(400);
        Assert.NotEqual(original, design.Colors.Primary.Background);
        scene.App.MarkdownParagraph("Dragging the wheel updates the theme immediately, without replacing the design-system provider.");
        scene.App.Snapshot("ChangedColor");
        var shape = scene.Find("DesignSystem.Shape");
        var originalShape = design.Shape.Medium;
        scene.Reveal(shape);
        scene.TapCell(shape, 3, 5);
        Assert.Same(design, shape.GetService(typeof(IDesignSystem)));
        Assert.NotEqual(originalShape, design.Shape.Medium);
        scene.App.Snapshot("ShapeAndDensity");
        var motion = scene.Find("DesignSystem.Motion");
        var originalDuration = design.Motion.Standard.DefaultDuration;
        scene.Reveal(motion);
        scene.TapCell(motion, 3, 4);
        Assert.True(design.Motion.Standard.DefaultDuration > originalDuration);
        scene.App.Snapshot("MotionOptions");
        scene.TapCell(scene.Find("DesignSystem.Tabs"), 1, 2, settle: false);
        scene.Advance(120);
        scene.App.MarkdownParagraph("A sliding segment and eased page movement connect the editor to the live preview.");
        scene.App.Snapshot("SwitchingToPreview");
        scene.Advance(1000);
        scene.App.Snapshot("LivePreview");
        var expander = Assert.IsAssignableFrom<Expander>(scene.Find("DesignSystem.MotionPreview"));
        Assert.Equal((double)expander.Frame.Left - 12, (double)expander.CullingFrame.Left, precision: 2);
        scene.Reveal(expander);
        scene.Tap(expander, header: true);
        scene.Advance(1000);
        Assert.True(expander.IsExpanded);
        scene.Reveal(expander);
        scene.App.MarkdownParagraph("Cards, actions, toggles, and the expandable section use the updated tokens. " +
            "Descriptions wrap inside their cards, and controls have a separate aligned column.");
        scene.App.Snapshot("ControlsAndMotion");
        var svg = scene.App.Render();
        Assert.Contains("@font-face", svg);
        Assert.Contains(XDocument.Parse(svg).Descendants(XName.Get("text", "http://www.w3.org/2000/svg")), t => t.Value == "Notifications");
    }

    private static IEnumerable<View> Descendants(View view)
    {
        for (var i = 0; i < view.Count; i++)
        {
            yield return view[i];
            foreach (var child in Descendants(view[i])) yield return child;
        }
    }

    private static XDocument RenderView(View view)
    {
        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext((430, 1000), stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
            view.Render(context);
        stream.Position = 0;
        return XDocument.Load(stream);
    }

    private static double HighlightX(View tabs)
    {
        var document = RenderView(tabs);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var paths = document.Descendants(ns + "path").ToArray();
        // The material is supplied by the parent group. The toolbar itself draws
        // exactly one selection fill, followed by its icons and labels.
        var highlight = Assert.Single(paths, path => (string?)path.Attribute("fill") == (string?)paths[0].Attribute("fill"));
        var tokens = highlight.Attribute("d")!.Value.Split(' ');
        var arc = Array.IndexOf(tokens, "A");
        var radius = arc < 0 ? 0 : double.Parse(tokens[arc + 1], System.Globalization.CultureInfo.InvariantCulture);
        return double.Parse(highlight.Attribute("d")!.Value.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture) - (double)radius;
    }

    private static double TabPosition(View tabs) =>
        (HighlightX(tabs) - (double)tabs.Frame.Left - 4) / ((double)(tabs.Frame.Width - 8) / 2);

    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) => new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }

    private static nfloat Translation(ScrollView scroll) => ((TransformView)scroll[0]).Transform.Ty;

    [Fact]
    public void ShadeTriangleSelectsWhiteBlackAndHueAndRetainsPalette()
    {
        using var scene = new Scene();
        var wheel = scene.Find("DesignSystem.Hue");
        var design = (IDesignSystem)wheel.GetService(typeof(IDesignSystem))!;
        var palette = design.Colors;
        var center = wheel.Parent!.LocalToGlobal(wheel.Frame.Center);
        var radius = (nfloat)115 * .83f - 12;
        var white = center + new Vector(-radius / 2, -nfloat.Sqrt(3) * radius / 2);
        var black = center + new Vector(-radius / 2, nfloat.Sqrt(3) * radius / 2);
        var hue = center + new Vector(radius, 0);
        scene.App.PointerDown(center);
        // Continue outside each corner: projection should land exactly on that vertex.
        scene.App.PointerMove(white + new Vector(-5, -5));
        Assert.InRange((double)new Oklch(palette.Primary.Background).L, .999, 1.001);
        scene.App.PointerMove(black + new Vector(-5, 5));
        Assert.InRange((double)new Oklch(palette.Primary.Background).L, 0, .001);
        scene.App.PointerMove(hue + new Vector(5, 0));
        Assert.InRange((double)new Oklch(palette.Primary.Background).C, .1, .4);
        for (var i = 0; i < 32; i++)
        {
            var point = center + new Vector(0, (i % 16 - 8) * 2);
            scene.App.PointerMove(point);
            scene.Advance(8);
        }
        scene.App.PointerUp(center);
        Assert.Same(palette, design.Colors);
        var selected = palette.Primary.Background;
        scene.App.PointerMove(white);
        Assert.Equal(selected, palette.Primary.Background);
    }
}
