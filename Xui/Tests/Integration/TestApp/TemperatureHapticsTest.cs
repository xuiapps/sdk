using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using Xui.Core.Actual;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class TemperatureHapticsTest
{
    [Fact]
    public void EveryRulerLineTicksIncludingFastMovesAndReversals()
    {
        var feedback = new RecordingFeedback();
        using var scene = new DialScene(feedback);
        Assert.Same(feedback, scene.Dial.GetService(typeof(IHapticFeedback)));
        Assert.Equal(0, feedback.Ticks);
        scene.App.PointerDown(scene.Center);
        Assert.Equal(1, feedback.Preparations);
        Assert.Equal(0, feedback.Ticks);

        scene.Move(-64); // 21 -> 23: four steps in one native input event.
        Assert.Equal((nfloat)23, scene.Dial.Temperature);
        Assert.Equal(4, feedback.Ticks);
        scene.Move(-32); // Reverse two steps.
        Assert.Equal(6, feedback.Ticks);
        scene.Move(-33); scene.Move(-34); // Between lines: no feedback.
        scene.App.Render();
        Assert.Equal(6, feedback.Ticks);

        scene.Move(-32); // Returning to the line ticks; leaving it did not.
        Assert.Equal(7, feedback.Ticks);

        scene.Move(1000); // Clamp at 16, count only reachable steps.
        Assert.Equal((nfloat)16, scene.Dial.Temperature);
        Assert.Equal(19, feedback.Ticks);
        scene.Move(1100);
        Assert.Equal(19, feedback.Ticks);
        scene.Move(-1000); // All 28 half-degree steps to the other end.
        Assert.Equal((nfloat)30, scene.Dial.Temperature);
        Assert.Equal(47, feedback.Ticks);
        scene.App.PointerUp(scene.Center + new Vector(-1000, 0));
        scene.Advance(350);
        scene.Advance(350);
        Assert.Equal(48, feedback.Ticks); // One completion tick; later frames stay silent.
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void FeedbackOccursOnLinesAndOnceAtSnapCompletion(int direction)
    {
        var feedback = new RecordingFeedback();
        using var scene = new DialScene(feedback);
        scene.App.PointerDown(scene.Center);
        scene.Move(direction * 9); // Beyond the midpoint, still short of the line.
        scene.Move(direction * 15);
        Assert.Equal(0, feedback.Ticks);
        scene.Move(direction * 16); // Exactly at the half-degree line.
        Assert.Equal(1, feedback.Ticks);
        scene.Move(direction * 17); // Leaving the line must not tick again.
        Assert.Equal(1, feedback.Ticks);
        scene.App.PointerUp(scene.Center + new Vector(direction * 17, 0));
        scene.Advance(200);
        Assert.Equal(1, feedback.Ticks);
        scene.Advance(50);
        Assert.Equal(2, feedback.Ticks);
        scene.Advance(350);
        Assert.Equal(2, feedback.Ticks);

        scene.App.PointerDown(scene.Center); // A stationary tap has no settling motion.
        scene.App.PointerUp(scene.Center);
        scene.Advance(350);
        Assert.Equal(2, feedback.Ticks);
    }

    [Fact]
    public void InertiaContinuesTickingAndTouchStopsIt()
    {
        var feedback = new RecordingFeedback();
        using var scene = new DialScene(feedback);
        scene.App.PointerDown(scene.Center);
        scene.Advance(16);
        for (int i = 1; i <= 4; i++)
        {
            scene.Move(-16 * i);
            scene.Advance(16);
        }
        var released = scene.Dial.Temperature;
        int releaseTicks = feedback.Ticks;
        scene.App.PointerUp(scene.Center + new Vector(-64, 0));
        scene.Advance(120);
        Assert.True(scene.Dial.Temperature > released);
        Assert.True(feedback.Ticks > releaseTicks);
        Assert.Equal((int)nfloat.Floor(scene.Dial.Temperature * 2) - 42, feedback.Ticks);

        scene.App.PointerDown(scene.Center);
        var caught = scene.Dial.Temperature;
        int caughtTicks = feedback.Ticks;
        scene.Advance(200);
        Assert.Equal(caught, scene.Dial.Temperature);
        Assert.Equal(caughtTicks, feedback.Ticks);
        scene.App.PointerUp(scene.Center);
        scene.Advance(350); scene.Advance(350);
        Assert.Equal(caughtTicks + 1, feedback.Ticks);
    }

    [Fact]
    public void EmulatorProvidesSilentFeedback()
    {
        using var scene = new DialScene();
        var feedback = Assert.IsAssignableFrom<IHapticFeedback>(scene.Dial.GetService(typeof(IHapticFeedback)));
        foreach (var effect in Enum.GetValues<HapticEffect>())
        {
            feedback.Prepare(effect); feedback.Play(effect);
        }
        scene.App.PointerDown(scene.Center); scene.Move(-32);
        scene.App.PointerUp(scene.Center + new Vector(-32, 0));
        scene.Advance(350);
        Assert.Equal((nfloat)22, scene.Dial.Temperature);
    }

    [Fact]
    public void DesktopDoesNotRegisterFeedback()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>((800, 600),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        Assert.Null(app.Window.RootView.GetService(typeof(IHapticFeedback)));
    }

    private sealed class RecordingFeedback : IHapticFeedback
    {
        public int Preparations { get; private set; }
        public int Ticks { get; private set; }
        public void Prepare(HapticEffect effect) { Assert.Equal(HapticEffect.Tick, effect); Preparations++; }
        public void Play(HapticEffect effect) { Assert.Equal(HapticEffect.Tick, effect); Ticks++; }
    }

    private sealed class DialScene : IDisposable
    {
        public TestSinglePageApp<Application, MainWindow> App { get; }
        public TemperatureDial Dial { get; }
        public Point Center => Dial.Parent!.LocalToGlobal(Dial.Frame.Center);
        private long time;

        public DialScene(IHapticFeedback? feedback = null)
        {
            App = new((478, 996), configure: services =>
            {
                services.AddSingleton<ITestSettings>(new TestSettings());
                if (feedback is not null) services.AddSingleton(feedback);
            }, runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
            App.Render();
            var button = App.Window.RootView.FindViewById("Gestures")!;
            Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(button).Clicked!();
            Advance(350);
            var carousel = App.Window.RootView.FindViewById("InputCarousel")!;
            Assert.True(carousel.TryFindParent<GesturesExample>(out var gallery));
            gallery.SelectSection(3);
            Advance(350);
            Dial = Assert.IsType<TemperatureDial>(App.Window.RootView.FindViewById("TemperatureDial"));
        }

        public void Move(nfloat dx) => App.PointerMove(Center + new Vector(dx, 0));

        public void Advance(int milliseconds)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds;
            App.Render();
        }

        public void Dispose() => App.Dispose();
    }
}
