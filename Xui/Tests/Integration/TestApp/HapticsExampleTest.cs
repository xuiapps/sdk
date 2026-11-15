using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Core.Actual;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class HapticsExampleTest
{
    [Theory]
    [InlineData(375)]
    [InlineData(430)]
    public void MenuOrderColumnsAndButtonsMatchEveryEffect(int width)
    {
        var feedback = new RecordingFeedback();
        var device = DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with
        { LogicalResolution = new Size(width, 800) };
        using var app = new TestSinglePageApp<Application, MainWindow>((width + 48, 864),
            configure: services =>
            {
                services.AddSingleton<ITestSettings>(new TestSettings());
                services.AddSingleton<IHapticFeedback>(feedback);
            }, runtimeVariant: TestRuntimeVariant.IPhoneEmulator, emulatorDevice: device,
            fontSource: Xui.Core.Fonts.Inter.Source);
        app.Render();
        var home = app.Window.RootView;
        var link = Assert.IsAssignableFrom<Button>(home.FindViewById("Haptics"));
        Assert.True(link.Frame.Top > home.FindViewById("TextBox")!.Frame.Top);
        Assert.True(link.Frame.Top < home.FindViewById("3D")!.Frame.Top);
        link.Clicked!();
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromMilliseconds(400));
        app.Render();

        foreach (var effect in Enum.GetValues<HapticEffect>())
        {
            var button = Assert.IsAssignableFrom<Button>(home.FindViewById($"Haptic.{effect}"));
            var row = button.Parent!;
            var description = home.FindViewById($"HapticDescription.{effect}")!;
            Assert.Equal(row.Frame.Width / 3, button.Frame.Width);
            Assert.True(button.Frame.Height >= 48);
            Assert.True(description.Frame.Left > button.Frame.Right);
            Assert.True(description.Frame.Right <= row.Frame.Right + 0.01f);
            Assert.Equal(2, description.Count);
            button.Clicked!();
        }
        Assert.Equal(Enum.GetValues<HapticEffect>(), feedback.Played);
        feedback.Played.Clear();

        var tick = home.FindViewById("Haptic.Tick")!;
        var point = tick.Parent!.LocalToGlobal(tick.Frame.Center);
        app.PointerDown(point);
        Assert.Equal([HapticEffect.Tick], feedback.Prepared);
        Assert.Empty(feedback.Played);
        app.PointerUp(point);
        Assert.Equal([HapticEffect.Tick], feedback.Played);
        feedback.Played.Clear();

        // A drag starting on a button must scroll without playing its effect.
        app.PointerDown(point);
        app.PointerMove(point + new Vector(0, -100));
        app.PointerUp(point + new Vector(0, -100));
        app.Render();
        Assert.Empty(feedback.Played);
        var error = home.FindViewById("Haptic.Error")!;
        var errorPoint = error.Parent!.LocalToGlobal(error.Frame.Center);
        var scroll = home.FindViewById("HapticsScroll")!;
        Assert.True(errorPoint.Y < scroll.Frame.Bottom);
        app.PointerDown(errorPoint);
        app.PointerUp(errorPoint);
        Assert.Equal([HapticEffect.Error], feedback.Played);
    }

    private sealed class RecordingFeedback : IHapticFeedback
    {
        public List<HapticEffect> Prepared { get; } = [];
        public List<HapticEffect> Played { get; } = [];
        public void Prepare(HapticEffect effect) => Prepared.Add(effect);
        public void Play(HapticEffect effect) => Played.Add(effect);
    }
}
