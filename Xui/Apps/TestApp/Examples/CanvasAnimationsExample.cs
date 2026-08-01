using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

/// <summary>Time-driven vector animations presented in the shared canvas card carousel.</summary>
public sealed class CanvasAnimationsExample : Example, IExampleSectionNavigator
{
    private static readonly (string Name, Func<View> Create)[] Sections =
    [
        ("BeatingHeart", static () => new BeatingHeart()),
        ("MorphingShape", static () => new MorphingShape()),
        ("DownloadComplete", static () => new DownloadComplete()),
    ];
    private readonly ExampleCardCarousel carousel;

    public CanvasAnimationsExample()
    {
        Title = "Canvas Animations";
        Content = carousel = new ExampleCardCarousel(Sections, 0, CanvasCardDescriptions.Animation)
            { Id = "CanvasAnimationsCarousel" };
    }

    public bool ShowsMobileSectionControls => false;
    public int SectionCount => Sections.Length;
    public int SelectedSectionIndex => carousel.SelectedIndex;
    public string GetSectionName(int index) => Sections[index].Name;
    public void SelectSection(int index) => carousel.Select(index);
    public event Action? SectionChanged
    {
        add => carousel.SelectionChanged += value;
        remove => carousel.SelectionChanged -= value;
    }

    private abstract class AnimatedDrawing : View, ICanvasDrawingExample
    {
        private TimeSpan? start;
        protected NFloat Seconds { get; private set; }

        // A stable envelope for every phase: animation never changes layout or zoom.
        public Rect GetArtworkBounds(IMeasureContext context) => new(180, 30, 240, 240);

        protected override void OnActivate()
        {
            base.OnActivate();
            start = null;
            Seconds = 0;
            RequestAnimationFrame();
        }

        protected override void AnimateCore(TimeSpan previous, TimeSpan current)
        {
            start ??= current;
            Seconds = (NFloat)(current - start.Value).TotalSeconds;
            InvalidateRender();
            RequestAnimationFrame();
        }

        protected sealed override void RenderCore(IContext context)
        {
            context.Save();
            try
            {
                context.Translate(Frame.Center);
                Draw(context);
            }
            finally { context.Restore(); }
        }

        protected abstract void Draw(IContext context);

        protected static NFloat Smooth(NFloat t)
        {
            t = NFloat.Clamp(t, 0, 1);
            return t * t * (3 - 2 * t);
        }
    }

    private sealed class BeatingHeart : AnimatedDrawing
    {
        protected override void Draw(IContext context)
        {
            var phase = Seconds * 1.2f % 1; // 72 beats per minute, with a quieter echo.
            var beat = Pulse(phase, 0, 0.1f, 0.18f, 0.28f)
                + 0.7f * Pulse(phase, 0.32f, 0.42f, 0.50f, 0.60f);
            var scale = 1 + 0.13f * beat;
            context.Scale((scale, scale));
            context.SetFill(new LinearGradient((-70, -85), (70, 85),
                [new(0, new Color(0xEF4A9CFF)), new(1, new Color(0x8A05FFFF))]));
            context.SetStroke(new Color(0x9B288CFF));
            context.LineWidth = 1.5f;
            // Offset the original two-bezier heart so its visual bounds are centered.
            NFloat s = 120;
            context.BeginPath();
            context.MoveTo((0, -0.476f * s));
            context.CurveTo((0.5f * s, -1.176f * s), (1.2f * s, -0.276f * s), (0, 0.724f * s));
            context.CurveTo((-1.2f * s, -0.276f * s), (-0.5f * s, -1.176f * s), (0, -0.476f * s));
            context.ClosePath();
            context.Fill();
            context.Stroke();
        }

        private static NFloat Pulse(NFloat x, NFloat a, NFloat b, NFloat c, NFloat d) =>
            NFloat.Min(Smooth((x - a) / (b - a)), 1 - Smooth((x - c) / (d - c)));
    }

    private sealed class MorphingShape : AnimatedDrawing
    {
        protected override void Draw(IContext context)
        {
            var time = Seconds * 1.5f;
            var cycle = time / 2 % 3;
            var from = (int)cycle;
            var blend = Smooth((cycle - from) / 0.8f);
            context.Rotate(time * 0.22f);
            context.SetFill(new LinearGradient((-90, -90), (90, 90),
                [new(0, new Color(0x35D8CAFF)), new(1, new Color(0x4358EAFF))]));
            context.SetStroke(new Color(0x3874CFFF));
            context.LineWidth = 1.5f;
            context.LineJoin = LineJoin.Round;
            context.BeginPath();
            for (var i = 0; i < 96; i++)
            {
                var angle = i * NFloat.Pi * 2 / 96;
                var a = Radius(from, angle);
                var radius = a + (Radius((from + 1) % 3, angle) - a) * blend;
                Point point = (NFloat.Cos(angle) * radius, NFloat.Sin(angle) * radius);
                if (i == 0) context.MoveTo(point);
                else context.LineTo(point);
            }
            context.ClosePath();
            context.Fill();
            context.Stroke();
        }

        private static NFloat Radius(int shape, NFloat angle) => shape switch
        {
            0 => 84,
            1 => 76 / NFloat.Pow(NFloat.Pow(NFloat.Abs(NFloat.Cos(angle)), 6)
                + NFloat.Pow(NFloat.Abs(NFloat.Sin(angle)), 6), 1 / (NFloat)6),
            _ => 78 + 19 * NFloat.Cos(5 * angle),
        };
    }

    private sealed class DownloadComplete : AnimatedDrawing
    {
        protected override void Draw(IContext context)
        {
            var time = Seconds * 1.5f % 5.2f;
            var progress = NFloat.Clamp(time / 2.6f, 0, 1);
            var morph = Smooth((time - 2.6f) / 0.65f);
            context.GlobalAlpha = Smooth(time / 0.25f) * (1 - Smooth((time - 4.6f) / 0.6f));
            var color = new Color(0.22f - 0.12f * morph, 0.42f + 0.27f * morph, 0.92f - 0.48f * morph, 1f);
            context.LineWidth = 8;
            context.LineCap = LineCap.Round;
            context.LineJoin = LineJoin.Round;
            context.SetStroke(new Color(0xE0E4ECFF));
            context.BeginPath();
            context.Arc((0, 0), 94, 0, NFloat.Pi * 2);
            context.Stroke();

            context.SetStroke(color);
            if (progress > 0)
            {
                context.BeginPath();
                context.Arc((0, 0), 94, -NFloat.Pi / 2, -NFloat.Pi / 2 + progress * NFloat.Pi * 2);
                context.Stroke();
            }

            // Keep corresponding vertices: the arrow shaft and head become one check stroke.
            ReadOnlySpan<Point> arrow = [(0, -45), (0, 34), (-25, 8), (0, 34), (25, 8)];
            ReadOnlySpan<Point> check = [(-40, 0), (-10, 30), (8, 10), (26, -10), (48, -35)];
            var bounce = NFloat.Sin(time * NFloat.Pi * 3) * 5 * (1 - morph);
            context.BeginPath();
            for (var i = 0; i < arrow.Length; i++)
            {
                var point = Point.Lerp(arrow[i], check[i], morph);
                point.Y += bounce;
                if (i == 0) context.MoveTo(point);
                else context.LineTo(point);
            }
            context.Stroke();
        }
    }
}
