using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class ButtonPressFeedbackTest
{
    [Fact]
    public void HomeNavigationKeepsReleasedHighlightAcrossPageReparenting()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>((478, 996),
            configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        var button = Assert.IsAssignableFrom<Button>(app.Window.RootView.FindViewById("CanvasTests"));
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        string Draw()
        {
            using var stream = new MemoryStream();
            using (var context = new SvgDrawingContext((1000, 1500), stream, Xui.Core.Fonts.Inter.Source, keepOpen: true)) button.Render(context);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        var idle = Draw();
        var point = button.Parent!.LocalToGlobal(button.Frame.Center);
        app.PointerDown(point);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
        app.PointerUp(point);
        Assert.True(navigation.IsTransitioning);
        app.Render();
        Assert.NotEqual(idle, Draw());
        app.AnimationFrame(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(200));
        Assert.NotEqual(idle, Draw());
        app.AnimationFrame(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(500));
        Assert.Equal(idle, Draw());
    }

    [Fact]
    public void QuickTapInsideScrollerShowsImmediatelyOnReleaseThenHoldsAndFades()
    {
        using var scene = new Scene(scroll: true);
        var idle = scene.Draw();
        scene.Down();
        Assert.Contains(scene.Button.Candidates, c => c.Handler is ScrollView && c.Gesture is IDrag);
        Assert.Equal(idle, scene.Draw());
        scene.Advance(50);
        Assert.Equal(idle, scene.Draw());
        scene.Up();
        Assert.Equal(1, scene.Clicks); // Action is not delayed by visual feedback.
        Assert.NotEqual(idle, scene.Draw());
        scene.Advance(199);
        Assert.Equal(1, scene.StateOpacity());
        scene.Advance(91);
        Assert.InRange(scene.StateOpacity(), 0.45, 0.55);
        scene.Advance(100);
        Assert.Equal(idle, scene.Draw());
    }

    [Fact]
    public void HoldWaitsForRecognitionAndScrollStealCancelsFeedbackAndClick()
    {
        using var scene = new Scene(scroll: true);
        var idle = scene.Draw();
        scene.Down(); scene.Advance(99);
        Assert.Equal(idle, scene.Draw());
        scene.Advance(2);
        Assert.NotEqual(idle, scene.Draw());
        var point = scene.Center;
        scene.App.PointerMove(point + new Vector(0, -80));
        Assert.IsAssignableFrom<IDrag>(scene.Button.GetCapturedGesture(0));
        scene.Advance(90);
        Assert.Equal(idle, scene.Draw());
        scene.App.PointerUp(point + new Vector(0, -80));
        Assert.Equal(0, scene.Clicks);
    }

    [Fact]
    public void EarlySwipeNeverFlashesPressedState()
    {
        using var scene = new Scene(scroll: true);
        var idle = scene.Draw();
        scene.Down(); scene.Advance(30);
        var point = scene.Center;
        scene.App.PointerMove(point + new Vector(0, -80));
        scene.App.PointerUp(point + new Vector(0, -80));
        scene.Advance(150);
        Assert.Equal(idle, scene.Draw());
        Assert.Equal(0, scene.Clicks);
    }

    [Fact]
    public void StandaloneButtonsHighlightImmediatelyAndKeepOverlappingRipples()
    {
        using var scene = new Scene();
        var idle = scene.Draw();
        var firstPoint = scene.Center + new Vector(-50, 0);
        scene.App.PointerDown(firstPoint);
        Assert.Empty(scene.Button.Candidates);
        Assert.NotEqual(idle, scene.Draw());
        scene.App.PointerUp(firstPoint);
        scene.Advance(50);
        scene.Down(); scene.Up();
        Assert.Equal(2, scene.Clicks);
        Assert.Equal(2, scene.RippleCount());
        Assert.All(scene.RipplePaths(), path => Assert.NotNull(path.Parent!.Attribute("clip-path")));
        scene.Advance(400);
        Assert.Equal(idle, scene.Draw());
        for (var i = 0; i < 20; i++) { scene.Down(); scene.Up(); }
        Assert.Equal(22, scene.Clicks);
        Assert.Equal(8, scene.RippleCount()); // Bounded storage during repeated taps.
        scene.Advance(400);
        Assert.Equal(idle, scene.Draw());
    }

    [Fact]
    public void PotentialHandlersComeFromTheEventRouteAndResetForEachContact()
    {
        using var scene = new Scene(advertise: true);
        var idle = scene.Draw();
        scene.Down();
        Assert.Single(scene.Button.Candidates);
        Assert.Same(scene.Surface, scene.Button.Candidates[0].Handler);
        Assert.Equal(idle, scene.Draw()); // No ScrollView anywhere in this page.
        scene.Up(); scene.Advance(400);
        scene.Surface.Advertise = false;
        scene.Down();
        Assert.Empty(scene.Button.Candidates);
        Assert.NotEqual(idle, scene.Draw());
        scene.App.Window.RootView.EventRouter.ReleasePointer(scene.Button, 0);
        scene.Up(); scene.Advance(100);
        Assert.Equal(1, scene.Clicks);
        Assert.Equal(idle, scene.Draw());
    }

    [Fact]
    public void OtherPointersAndReleaseOutsideDoNotTriggerTheButton()
    {
        using var scene = new Scene();
        scene.Down();
        scene.App.PointerDown(scene.Center, index: 1);
        scene.App.PointerUp(scene.Center, index: 1);
        Assert.Equal(0, scene.Clicks);
        Assert.Equal(1, scene.RippleCount());
        scene.App.PointerUp(scene.Center + new Vector(0, 100));
        Assert.Equal(0, scene.Clicks);
        scene.Advance(100);
        Assert.Equal(0, scene.RippleCount());
    }

    private sealed class ProbeButton : Button
    {
        public PointerGestureCandidate[] Candidates = [];
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down) Candidates = e.PotentialGestures.ToArray();
            base.OnPointerEvent(ref e, phase);
        }
        public void Draw(IContext context) => base.RenderCore(context);
    }

    private sealed class Surface : ViewCollection
    {
        public bool Advertise;
        public readonly ProbeButton Button = new()
        {
            Text = "Tap again", Variant = ButtonVariant.Text,
            HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch,
        };
        public Surface() => Add(Button);
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (Advertise && phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
                e.PushPotentialGesture(this, PointerGestures.DragHorizontalTentative);
            base.OnPointerEvent(ref e, phase);
        }
        protected override Size MeasureCore(Size available, IMeasureContext context)
        { Button.Measure((available.Width - 64, 56), context); return (available.Width, 1200); }
        protected override void ArrangeCore(Rect rect, IMeasureContext context) =>
            Button.Arrange((rect.X + 32, rect.Y + 150, rect.Width - 64, 56), context);
    }

    private sealed class Scene : IDisposable
    {
        public readonly TestSinglePageApp<Application, MainWindow> App;
        public readonly Surface Surface;
        public ProbeButton Button => Surface.Button;
        public int Clicks;
        private int time;
        public Point Center => Button.Parent!.LocalToGlobal(Button.Frame.Center);
        public Scene(bool scroll = false, bool advertise = false)
        {
            App = new((478, 996), configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
            App.Render();
            var link = App.Window.RootView.FindViewById("CanvasTests")!;
            Assert.True(link.TryFindParent<NavigationView>(out var navigation));
            Surface = new() { Advertise = advertise };
            Button.Clicked = () => Clicks++;
            navigation.Push(scroll ? new ScrollView { Content = Surface } : Surface);
            Advance(400);
        }
        public void Down() => App.PointerDown(Center);
        public void Up() => App.PointerUp(Center);
        public void Advance(int ms)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + ms));
            time += ms; App.Render();
        }
        public string Draw()
        {
            using var stream = new MemoryStream();
            using (var context = new SvgDrawingContext((1000, 1500), stream, Xui.Core.Fonts.Inter.Source, keepOpen: true)) Button.Draw(context);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        private IEnumerable<XElement> Paths() => XDocument.Parse(Draw()).Descendants(XName.Get("path", "http://www.w3.org/2000/svg"))
            .Where(p => !p.Ancestors(XName.Get("defs", "http://www.w3.org/2000/svg")).Any());
        public double StateOpacity() => (double?)Paths().First().Attribute("fill-opacity") ?? 1;
        public IEnumerable<XElement> RipplePaths() => Paths().Where(p => (double?)p.Attribute("fill-opacity") is > 0 and <= 0.12);
        public int RippleCount() => RipplePaths().Count();
        public void Dispose() => App.Dispose();
    }
}
