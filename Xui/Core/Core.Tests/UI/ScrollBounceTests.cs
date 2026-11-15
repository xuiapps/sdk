using Xui.Core.Abstract.Events;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Tests;

public class ScrollBounceTests
{
    [Theory]
    [InlineData(DevicePlatform.iOS, true)]
    [InlineData(DevicePlatform.MacOS, true)]
    [InlineData(DevicePlatform.Windows, false)]
    [InlineData(DevicePlatform.Android, false)]
    [InlineData(DevicePlatform.Browser, false)]
    public void PullResistsAndReturnsOnlyOnApplePlatforms(DevicePlatform platform, bool bounces)
    {
        var view = Create(platform);
        Pointer(view, PointerEventType.Down, (100, 100));
        Pointer(view, PointerEventType.Move, (100, 300));
        var pulled = Translation(view).Y;
        if (bounces) Assert.InRange((double)pulled, 1, 199);
        else Assert.Equal((nfloat)0, pulled);
        // Layout during the gesture preserves its displacement.
        view.Arrange((0, 0, 300, 400), null!);
        Assert.Equal(pulled, Translation(view).Y);
        view.Clock.Milliseconds += 200; // Pause before releasing: no fling velocity.
        Pointer(view, PointerEventType.Up, (100, 300));
        view.Animate(TimeSpan.Zero, TimeSpan.Zero);
        view.Animate(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        if (bounces) Assert.InRange((double)Translation(view).Y, 0.01, (double)pulled - 0.01);
        view.Animate(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        Assert.Equal((nfloat)0, Translation(view).Y);
    }

    [Fact]
    public void ReducedMotionUsesClampedEdges()
    {
        var view = Create(DevicePlatform.iOS, reducedMotion: true);
        Pointer(view, PointerEventType.Down, (100, 100));
        Pointer(view, PointerEventType.Move, (100, 300));
        Assert.Equal((nfloat)0, Translation(view).Y);
    }

    [Fact]
    public void InterruptedReboundResumesWithoutJumpAndCancellationSettles()
    {
        var view = Create(DevicePlatform.iOS);
        Pointer(view, PointerEventType.Down, (100, 100));
        Pointer(view, PointerEventType.Move, (100, 300));
        Pointer(view, PointerEventType.Up, (100, 300));
        view.Animate(TimeSpan.Zero, TimeSpan.Zero);
        view.Animate(TimeSpan.Zero, TimeSpan.FromMilliseconds(60));
        var caught = Translation(view).Y;
        Pointer(view, PointerEventType.Down, (100, 100));
        view.Animate(TimeSpan.FromMilliseconds(60), TimeSpan.FromMilliseconds(160));
        Assert.Equal(caught, Translation(view).Y);
        Pointer(view, PointerEventType.Move, (100, 110));
        Assert.InRange((double)(Translation(view).Y - caught), 0, 10);
        Pointer(view, PointerEventType.Cancel, (100, 110));
        view.Animate(TimeSpan.FromMilliseconds(160), TimeSpan.FromMilliseconds(160));
        view.Animate(TimeSpan.FromMilliseconds(160), TimeSpan.FromSeconds(2));
        Assert.Equal((nfloat)0, Translation(view).Y);
    }

    [Fact]
    public void HorizontalPullAndReversalHaveNoAccumulatedResistance()
    {
        var view = Create(DevicePlatform.iOS);
        view.Direction = ScrollDirection.Horizontal;
        view.Content = new FixedView((800, 400));
        view.Arrange((0, 0, 300, 400), null!);
        Pointer(view, PointerEventType.Down, (100, 100));
        Pointer(view, PointerEventType.Move, (200, 100));
        Pointer(view, PointerEventType.Move, (300, 100));
        Assert.InRange((double)Translation(view).X, 1, 199);
        Pointer(view, PointerEventType.Move, (100, 100));
        Assert.InRange((double)Translation(view).X, -0.0001, 0.0001);
        Pointer(view, PointerEventType.Move, (-500, 100));
        Assert.True(Translation(view).X < -500); // Far edge overscroll, too.
        Pointer(view, PointerEventType.Cancel, (-500, 100));
        view.Animate(TimeSpan.Zero, TimeSpan.Zero);
        view.Animate(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        Assert.Equal((nfloat)(-500), Translation(view).X);
    }

    [Fact]
    public void FlingCrossesEdgeThenSettlesAtTheExactLimit()
    {
        var view = Create(DevicePlatform.iOS);
        Pointer(view, PointerEventType.Down, (100, 700));
        view.Clock.Milliseconds += 100;
        Pointer(view, PointerEventType.Move, (100, 200)); // 500px into a 600px range.
        Pointer(view, PointerEventType.Up, (100, 200));
        view.Animate(TimeSpan.Zero, TimeSpan.Zero);
        view.Animate(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        Assert.True(Translation(view).Y < -600);
        view.Animate(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2));
        Assert.Equal((nfloat)(-600), Translation(view).Y);
    }

    [Fact]
    public void MacWheelOverscrollWaitsForQuietThenReturns()
    {
        var view = Create(DevicePlatform.MacOS);
        var wheel = new ScrollWheelEventRef { Delta = (0, 120) };
        view.OnScrollWheel(ref wheel);
        Assert.True(wheel.Handled);
        var pulled = Translation(view).Y;
        Assert.InRange((double)pulled, 0.01, 80);
        view.Animate(TimeSpan.Zero, TimeSpan.Zero);
        view.Animate(TimeSpan.Zero, TimeSpan.FromMilliseconds(40));
        Assert.Equal(pulled, Translation(view).Y);
        view.Animate(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(100));
        view.Animate(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        Assert.Equal((nfloat)0, Translation(view).Y);
    }

    [Fact]
    public void SpringPreservesInitialVelocityAndDoesNotDependOnSamplingRate()
    {
        var start = TimeSpan.FromHours(3);
        var spring = new SpringMotionCurve(start, 0, 1200, 0);
        Assert.Equal((nfloat)0, spring[start]);
        Assert.Equal((nfloat)1200, spring.VelocityAt(start));
        Assert.True(spring[start + TimeSpan.FromMilliseconds(40)] > 0);
        for (var i = 0; i < 60; i++) _ = spring[start + TimeSpan.FromSeconds(i / 120.0)];
        var at120 = spring[start + TimeSpan.FromSeconds(0.5)];
        for (var i = 0; i < 30; i++) _ = spring[start + TimeSpan.FromSeconds(i / 60.0)];
        Assert.Equal(at120, spring[start + TimeSpan.FromSeconds(0.5)]);
        Assert.True(spring.IsComplete(start + TimeSpan.FromSeconds(1)));
    }

    private static TestScroll Create(DevicePlatform platform, bool reducedMotion = false)
    {
        var view = new TestScroll(new Device(platform, reducedMotion)) { Content = new FixedView((300, 1000)) };
        view.Arrange((0, 0, 300, 400), null!);
        return view;
    }
    private static Vector Translation(ScrollView view)
    {
        var transform = ((TransformView)view[0]).Transform;
        return (transform.Tx, transform.Ty);
    }
    private static void Pointer(ScrollView view, PointerEventType type, Point position)
    {
        var state = new PointerState(position, (1, 1), 1, 0, (0, 0), 0, 0, 0,
            PointerType.Touch, PointerButton.Left, PointerButtons.Left);
        var e = new PointerEventRef(type, 0, 0, true, state, [], []);
        view.OnPointerEvent(ref e, EventPhase.Bubble);
    }
    private sealed class TestScroll(IDeviceInfo device) : ScrollView
    {
        public ManualClock Clock { get; } = new();
        public override object? GetService(Type type) => type == typeof(IDeviceInfo) ? device
            : type == typeof(TimeProvider) ? Clock : base.GetService(type);
    }
    private sealed class ManualClock : TimeProvider
    {
        public long Milliseconds;
        public override long TimestampFrequency => 1000;
        public override long GetTimestamp() => Milliseconds;
    }
    private sealed class FixedView(Size size) : View
    {
        protected override Size MeasureCore(Size available, IMeasureContext context) => size;
    }
    private sealed class Device(DevicePlatform platform, bool reducedMotion) : IDeviceInfo
    {
        public event Action? Changed { add { } remove { } }
        public DevicePlatform Platform => platform;
        public DeviceFormFactor FormFactor => DeviceFormFactor.Mobile;
        public PointerModel PointerModel => PointerModel.Touch;
        public nfloat AccessibilityFontScale => 1;
        public bool PrefersReducedMotion => reducedMotion;
        public bool PrefersHighContrast => false;
        public ColorScheme ColorScheme => ColorScheme.Light;
    }
}
