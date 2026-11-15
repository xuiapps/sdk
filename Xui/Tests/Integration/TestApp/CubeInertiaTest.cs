using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.ThreeD.Tests;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class CubeInertiaTest
{
    [Fact]
    public void FlickCarriesBothAxesThenDecaysAndStopsRendering()
    {
        using var scene = new Scene();
        scene.Flick(12, 6);
        var pitch = scene.Cube.Pitch; var yaw = scene.Cube.Yaw;
        scene.Advance(16); // Start the release curves.
        scene.Advance(100);
        var firstTravel = yaw - scene.Cube.Yaw;
        Assert.True(firstTravel > 0);
        Assert.True(scene.Cube.Pitch < pitch);
        yaw = scene.Cube.Yaw;
        scene.Advance(100);
        Assert.InRange(yaw - scene.Cube.Yaw, (nfloat)0, firstTravel);
        scene.Advance(1500);
        var settled = (scene.Cube.Pitch, scene.Cube.Yaw, scene.Cube.Frames);
        scene.Advance(500);
        Assert.Equal(settled, (scene.Cube.Pitch, scene.Cube.Yaw, scene.Cube.Frames));
    }

    [Fact]
    public void TiltClampsInBothDirectionsAndReversesImmediatelyAtTheLimit()
    {
        using var scene = new Scene();
        var limit = nfloat.Pi * 70 / 180;
        scene.App.PointerDown(scene.Center);
        scene.Move(0, 500);
        Assert.Equal(-limit, scene.Cube.Pitch);
        scene.Move(0, 490);
        Assert.True(scene.Cube.Pitch > -limit);
        scene.Move(0, -500);
        Assert.Equal(limit, scene.Cube.Pitch);
        scene.App.PointerUp(scene.Center + new Vector(0, -500));
        scene.Advance(16); scene.Advance(1000);
        Assert.InRange(scene.Cube.Pitch, -limit, limit);
    }

    [Fact]
    public void ReachingTiltLimitDoesNotStopHorizontalInertia()
    {
        using var scene = new Scene();
        scene.App.PointerDown(scene.Center);
        scene.Move(0, -80);
        scene.Advance(16);
        for (var i = 1; i <= 3; i++)
        {
            scene.Move(i * 15, -80 - i * 7);
            scene.Advance(16);
        }
        scene.App.PointerUp(scene.Center + new Vector(45, -101));
        scene.Advance(16); scene.Advance(100);
        Assert.Equal(nfloat.Pi * 70 / 180, scene.Cube.Pitch);
        var yaw = scene.Cube.Yaw;
        scene.Advance(100);
        Assert.True(scene.Cube.Yaw < yaw);
    }

    [Fact]
    public void HorizontalRotationIsUnboundedAndVerticalDragDoesNotChangeYaw()
    {
        using var scene = new Scene();
        scene.App.PointerDown(scene.Center);
        var pitch = scene.Cube.Pitch;
        scene.Move(1000, 0);
        Assert.Equal(pitch, scene.Cube.Pitch);
        Assert.True(scene.Cube.Yaw < -2 * nfloat.Pi);
        var yaw = scene.Cube.Yaw;
        scene.Move(1000, 20);
        Assert.Equal(yaw, scene.Cube.Yaw);
        Assert.True(scene.Cube.Pitch < pitch);
    }

    [Fact]
    public void TouchHoldAndNavigationStopMomentumWithoutResumingIt()
    {
        using var scene = new Scene();
        scene.Flick(15, -5);
        scene.Advance(16); scene.Advance(100);
        scene.App.PointerDown(scene.Center);
        var caught = (scene.Cube.Pitch, scene.Cube.Yaw);
        scene.Advance(500);
        scene.App.PointerUp(scene.Center);
        scene.Advance(16); scene.Advance(500);
        Assert.Equal(caught, (scene.Cube.Pitch, scene.Cube.Yaw));

        scene.Flick(-15, 5);
        scene.Advance(16);
        scene.Cube.IsLive = false;
        var frozen = (scene.Cube.Pitch, scene.Cube.Yaw);
        scene.Advance(500);
        scene.Cube.IsLive = true;
        scene.Advance(500);
        Assert.Equal(frozen, (scene.Cube.Pitch, scene.Cube.Yaw));
    }

    [Fact]
    public void HoldingBeforeReleaseAndLosingCaptureDoNotFling()
    {
        using var scene = new Scene();
        scene.App.PointerDown(scene.Center);
        scene.Advance(16);
        scene.Move(50, 20);
        scene.Advance(16);
        scene.Advance(500); // Stationary finger loses its old velocity.
        scene.App.PointerUp(scene.Center + new Vector(50, 20));
        var held = (scene.Cube.Pitch, scene.Cube.Yaw);
        scene.Advance(16); scene.Advance(1000);
        Assert.Equal(held, (scene.Cube.Pitch, scene.Cube.Yaw));

        scene.App.PointerDown(scene.Center);
        scene.Advance(16);
        scene.Move(-50, -20);
        scene.Advance(16);
        scene.Cube.ReleasePointer(0);
        var cancelled = (scene.Cube.Pitch, scene.Cube.Yaw);
        scene.App.PointerUp(scene.Center + new Vector(-50, -20));
        scene.Advance(16); scene.Advance(1000);
        Assert.Equal(cancelled, (scene.Cube.Pitch, scene.Cube.Yaw));
    }

    [Fact]
    public void CoastingUsesElapsedTimeRatherThanFrameCount()
    {
        using var first = new Scene();
        using var second = new Scene();
        first.Flick(12, 4); second.Flick(12, 4);
        first.Advance(16); second.Advance(16);
        first.Advance(300);
        for (int i = 0; i < 30; i++) second.Advance(10);
        Assert.Equal(first.Cube.Yaw, second.Cube.Yaw);
        Assert.Equal(first.Cube.Pitch, second.Cube.Pitch);
    }

    public sealed class RotationExample : Example, INavigationAware
    {
        public ProbeCube Cube { get; } = new();
        public RotationExample() => Content = Cube;
        public void OnNavigationStateChanged(in NavigationStateChange change) =>
            Cube.IsLive = change.Phase == NavigationPhase.Appeared;
    }

    public sealed class ProbeCube : CubeSurface
    {
        public nfloat Pitch => rotationX;
        public nfloat Yaw => rotationY;
        public int Frames { get; private set; }
        protected override string BackendLabel => "Inertia test";
        protected override bool RenderPixels(byte[] pixels) { Frames++; return false; }
    }

    private sealed class Scene : IDisposable
    {
        public TestSinglePageApp<Application, MainWindow> App { get; }
        public ProbeCube Cube { get; }
        public Point Center => Cube.Parent!.LocalToGlobal(Cube.Frame.Center);
        private int time;
        public Scene()
        {
            App = new((441, 916), runtimeVariant: TestRuntimeVariant.IPhoneEmulator,
                fontSource: Xui.Core.Fonts.Inter.Source);
            App.Render();
            var button = App.Window.RootView.FindViewById("Settings")!;
            Assert.True(button.TryFindParent<NavigationView>(out var navigation));
            var page = new MobileExamplePage<RotationExample>();
            Cube = ((RotationExample)page[0]).Cube;
            navigation.Push(page);
            Advance(400);
            Assert.True(Cube.IsLive);
        }
        public void Move(nfloat x, nfloat y) => App.PointerMove(Center + new Vector(x, y));
        public void Flick(nfloat dx, nfloat dy)
        {
            App.PointerDown(Center);
            Advance(16);
            for (int i=1; i<=4; i++) { Move(dx*i, dy*i); Advance(16); }
            App.PointerUp(Center + new Vector(dx*4, dy*4));
        }
        public void Advance(int ms)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time+ms));
            time+=ms; App.Render();
        }
        public void Dispose() => App.Dispose();
    }
}
