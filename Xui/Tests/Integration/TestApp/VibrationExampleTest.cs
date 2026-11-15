using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Examples;
using Xui.Core.Actual;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class VibrationExampleTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TouchSurfaceIsSquareAndCenteredOnBothPages(bool record)
    {
        using var scene = new Scene(record);
        var pad = scene.Pad(record);
        Assert.Equal(pad.Frame.Width, pad.Frame.Height);
        Assert.Equal(pad.ControlArea.Width, pad.ControlArea.Height);
        Assert.Equal(pad.Parent!.Frame.Center, pad.Frame.Center);
        Assert.True(pad.Frame.Width >= 200);
    }

    [Fact]
    public void FasterEndpointMotionProducesAFasterReturnAlongTheSameCurve()
    {
        var fast = Record(20);
        var slow = Record(100);
        var fastReturn = fast[^1].Time - fast[^33].Time;
        var slowReturn = slow[^1].Time - slow[^33].Time;
        Assert.True(fastReturn.TotalSeconds < slowReturn.TotalSeconds * 0.6);
        // Geometry stays the same; only traversal time changes with the gesture.
        for (var i = 1; i <= 32; i++)
        {
            Assert.InRange((double)(fast[^i].Strength - slow[^i].Strength), -0.00001, 0.00001);
            Assert.InRange((double)(fast[^i].Frequency - slow[^i].Frequency), -0.00001, 0.00001);
        }

        static VibrationPoint[] Record(int stepMilliseconds)
        {
            using var scene = new Scene(record: true);
            var pad = scene.Pad(true);
            scene.App.PointerDown(scene.Position(pad, 0.2f, 0.8f));
            for (var i = 1; i < 10; i++)
            {
                scene.Advance(stepMilliseconds);
                scene.App.PointerMove(scene.Position(pad, 0.2f + (nfloat)i * 0.06f, 0.8f - (nfloat)i * 0.06f));
            }
            scene.Advance(stepMilliseconds);
            scene.App.PointerUp(scene.Position(pad, 0.8f, 0.2f));
            VibrationPattern.Validate(scene.Feedback.Pattern!);
            return scene.Feedback.Pattern!;
        }
    }

    [Fact]
    public void LiveControlHasItsOwnPageAndStopsOnRelease()
    {
        using var scene = new Scene();
        var pad = scene.Pad(false);
        scene.App.PointerDown(scene.Position(pad, 0.25f, 0.75f));
        Assert.InRange((double)scene.Feedback.Strength, (double)0.75f - 0.00001, (double)0.75f + 0.00001);
        Assert.InRange((double)scene.Feedback.Frequency, (double)0.25f - 0.00001, (double)0.25f + 0.00001);
        scene.App.PointerMove(scene.Position(pad, 0.75f, 0.25f));
        Assert.InRange((double)scene.Feedback.Strength, (double)0.25f - 0.00001, (double)0.25f + 0.00001);
        Assert.InRange((double)scene.Feedback.Frequency, (double)0.75f - 0.00001, (double)0.75f + 0.00001);
        scene.App.PointerUp(scene.Position(pad, 0.75f, 0.25f));
        Assert.False(scene.Feedback.Playing);
        Assert.False(pad.IsLooping);

        Assert.Null(scene.App.Window.RootView.FindViewById("Vibration.Live.Stop"));
        Assert.Null(scene.App.Window.RootView.FindViewById("Vibration.Record.Playback"));
        Assert.Null(scene.App.Window.RootView.FindViewById("VibrationCarousel"));
        Assert.True(pad.TryFindParent<LiveVibrationExample>(out _));
    }

    [Fact]
    public void RecorderClosesTheLoopAndCanReplayAfterStopping()
    {
        using var scene = new Scene(record: true);
        var pad = scene.Pad(true);
        scene.App.PointerDown(scene.Position(pad, 0.25f, 0.75f));
        scene.Advance(100);
        scene.App.PointerMove(scene.Position(pad, 0.75f, 0.25f));
        scene.Advance(100);
        scene.App.PointerUp(scene.Position(pad, 0.75f, 0.25f));
        Assert.True(pad.IsLooping);
        Assert.True(scene.Feedback.Repeat);
        var points = scene.Feedback.Pattern!;
        VibrationPattern.Validate(points);
        Assert.Equal(TimeSpan.Zero, points[0].Time);
        Assert.InRange(points[^1].Time, TimeSpan.FromMilliseconds(280), TimeSpan.FromMilliseconds(2200));
        Assert.Contains(points, point => point.Time == TimeSpan.FromMilliseconds(200));
        Assert.InRange((double)points[0].Strength, (double)0.75f - 0.00001, (double)0.75f + 0.00001);
        Assert.InRange((double)points[^1].Strength, (double)0.75f - 0.00001, (double)0.75f + 0.00001);
        Assert.InRange((double)points[^1].Frequency, (double)0.25f - 0.00001, (double)0.25f + 0.00001);
        scene.Advance(16); // Activate and style the newly available playback button.
        var svg = System.Xml.Linq.XDocument.Parse(scene.App.Render());
        Assert.Contains(svg.Descendants(System.Xml.Linq.XName.Get("path", "http://www.w3.org/2000/svg")),
            path => (string?)path.Attribute("stroke") == "#5266DE" && ((string?)path.Attribute("d"))!.Contains("C "));
        Assert.Contains(svg.Descendants(System.Xml.Linq.XName.Get("text", "http://www.w3.org/2000/svg")),
            text => text.Value == "Stop" && (string?)text.Attribute("font-size") != "0");

        Assert.Equal("Stop", scene.PlaybackButton.Text);
        scene.Toggle();
        Assert.False(scene.Feedback.Playing);
        Assert.False(pad.IsLooping);
        Assert.True(pad.HasPattern);
        Assert.Equal("Start", scene.PlaybackButton.Text);
        scene.Toggle();
        Assert.Equal("Stop", scene.PlaybackButton.Text);
        Assert.True(pad.IsLooping);
        Assert.Equal(points, scene.Feedback.Pattern);
        // Recording a new tap replaces the old pattern and closes safely too.
        RecordTap();
        Assert.True(pad.TryFindParent<NavigationView>(out var navigation));
        navigation.Pop();
        Assert.False(scene.Feedback.Playing);
        Assert.False(pad.IsLooping);

        void RecordTap()
        {
            var position = scene.Position(pad, 0.5f, 0.5f);
            scene.App.PointerDown(position); scene.App.PointerUp(position);
            VibrationPattern.Validate(scene.Feedback.Pattern!);
            Assert.True(pad.IsLooping);
        }
    }

    [Fact]
    public void RecorderIgnoresOtherPointersAndBoundsLongRecording()
    {
        using var scene = new Scene(record: true);
        var pad = scene.Pad(true);
        var position = scene.Position(pad, 0.5f, 0.5f);
        scene.App.PointerDown(position);
        scene.App.PointerDown(scene.Position(pad, 1, 1), index: 1);
        scene.App.PointerUp(scene.Position(pad, 1, 1), index: 1);
        Assert.InRange((double)scene.Feedback.Strength, (double)0.5f - 0.00001, (double)0.5f + 0.00001);
        Assert.False(pad.IsLooping);
        for (int i = 0; i < 600; i++) scene.Advance(50, render: false);
        scene.Advance(1000, render: false);
        scene.App.PointerUp(position);
        Assert.InRange(pad.RecordedPointCount, 2, 512);
        VibrationPattern.Validate(scene.Feedback.Pattern!);
        Assert.Equal(VibrationPattern.MaximumDuration, scene.Feedback.Pattern![^1].Time);
    }

    [Fact]
    public void CancelledRecordingDoesNotStartALoopOnLaterRelease()
    {
        using var scene = new Scene(record: true);
        var pad = scene.Pad(true);
        var position = scene.Position(pad, 0.5f, 0.5f);
        scene.App.PointerDown(position);
        scene.Advance(100);
        var cancel = new PointerEventRef(PointerEventType.Cancel, 0, 0, true, default, [], []);
        pad.OnPointerEvent(ref cancel, EventPhase.Bubble);
        scene.App.PointerUp(position);
        Assert.False(scene.Feedback.Playing);
        Assert.False(pad.IsLooping);
        Assert.Null(scene.Feedback.Pattern);
    }

    [Fact]
    public void EmulatorProvidesSilentVibrationAndDesktopLeavesItOptional()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>((478, 996),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        var vibration = Assert.IsAssignableFrom<IVibration>(app.Window.RootView.GetService(typeof(IVibration)));
        Assert.False(vibration.IsSupported);
        vibration.Play(1, 1);
        vibration.Play([new(TimeSpan.Zero, 1, 1), new(TimeSpan.FromSeconds(1), 0, 0)], true);
        vibration.Stop(); vibration.Stop();
        using var desktop = new TestSinglePageApp<Application, MainWindow>((800, 600),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            fontSource: Xui.Core.Fonts.Inter.Source);
        desktop.Render();
        Assert.Null(desktop.Window.RootView.GetService(typeof(IVibration)));
    }

    private sealed class Feedback : IVibration
    {
        public bool IsSupported => true;
        public bool SupportsFrequency => true;
        public bool Playing, Repeat;
        public nfloat Strength, Frequency;
        public VibrationPoint[]? Pattern;
        public void Play(nfloat strength, nfloat frequency)
        { VibrationPattern.Validate(strength, frequency); Strength = strength; Frequency = frequency; Playing = strength > 0; }
        public void Play(scoped ReadOnlySpan<VibrationPoint> points, bool repeat = false)
        { VibrationPattern.Validate(points); Pattern = points.ToArray(); Repeat = repeat; Playing = true; }
        public void Stop() => Playing = false;
    }

    private sealed class Scene : IDisposable
    {
        public readonly Feedback Feedback = new();
        public TestSinglePageApp<Application, MainWindow> App { get; }
        private int time;
        public Scene(bool record = false)
        {
            App = new((478, 996), configure: services =>
            {
                services.AddSingleton<ITestSettings>(new TestSettings());
                services.AddSingleton<IVibration>(Feedback);
            }, runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
            App.Render();
            Assert.IsAssignableFrom<Button>(App.Window.RootView.FindViewById(record ? "VibrationRecord" : "VibrationLive")).Clicked!();
            Advance(400);
            Assert.Null(App.Window.RootView.FindViewById("VibrationCarousel"));
        }
        public VibrationPad Pad(bool record) => Assert.IsType<VibrationPad>(App.Window.RootView.FindViewById(record ? "Vibration.Record.Pad" : "Vibration.Live.Pad"));
        public Button PlaybackButton => Assert.IsAssignableFrom<Button>(App.Window.RootView.FindViewById("Vibration.Record.Playback"));
        public void Toggle()
        {
            App.Render();
            var point = PlaybackButton.Parent!.LocalToGlobal(PlaybackButton.Frame.Center);
            App.PointerDown(point); App.PointerUp(point); App.Render();
        }
        public Point Position(VibrationPad pad, nfloat frequency, nfloat strength) => pad.Parent!.LocalToGlobal(
            (pad.ControlArea.X + pad.ControlArea.Width * frequency, pad.ControlArea.Bottom - pad.ControlArea.Height * strength));
        public void Advance(int ms, bool render = true)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + ms));
            time += ms;
            if (render) App.Render();
        }
        public void Dispose() => App.Dispose();
    }
}
