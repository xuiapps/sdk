using System.Diagnostics;
using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using static Xui.Core.Canvas.Colors;
using static Xui.Core.Animation.Easing;
using Xui.Core.Math2D;
using Xui.Core.Actual;

using Window = Xui.Core.Abstract.Window;
using Point = Xui.Core.Math2D.Point;
using Rect = Xui.Core.Math2D.Rect;
using Color = Xui.Core.Canvas.Color;
using Colors = Xui.Core.Canvas.Colors;
using Font = Xui.Core.Canvas.Font;
using CornerRadius = Xui.Core.Canvas.CornerRadius;
using Xui.Core.UI;
using Xui.Core.Canvas.SVG;

namespace Xui.Apps.BlankApp;

public class MainWindow : Window
{
    public MainWindow(IServiceProvider context) : base(context)
    {
        this.Title = "Xui BlankApp";
    }

    private Point mousePoint;
    private Point scrollPoint;

    public override void OnMouseMove(ref MouseMoveEventRef e)
    {
        this.mousePoint = e.Position;
        this.Invalidate();
        base.OnMouseMove(ref e);
    }

    public override void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        this.scrollPoint += e.Delta;

        this.Invalidate();
        base.OnScrollWheel(ref e);
    }

    private Xui.Core.Math2D.Point touchPoint;
    private Vector scrollInertia = Vector.Zero;
    private bool runScrollInertia = false;

    private nint fps = 0;

    private nint maxFps = 0;

    private nint didIStutter = 0;

    private string textContent = "";

    // // SmoothDamp test
    private NFloat target;
    private NFloat current;
    private NFloat velocity;

    private NFloat tab1Selected;
    private NFloat tab1SelectionVelocity;

    private NFloat tab2Selected;
    private NFloat tab2SelectionVelocity;

    private NFloat tab3Selected;
    private NFloat tab3SelectionVelocity;

    private uint SelectedTabIndex = 0;

    public override void OnTouch(ref TouchEventRef e)
    {
        foreach(var touch in e.Touches)
        {
            // TODO: Simulate tabview press...
            if (touch.Position.Y > this.SafeArea.Bottom - 40f)
            {
                uint index = (uint)int.Clamp((int)((touch.Position.X - this.SafeArea.Left) / this.SafeArea.Width * 4f), 0, 3);

                this.SelectedTabIndex = index;
                NFloat tabWidth = this.SafeArea.Width / 4f;
                this.target = this.SafeArea.Left + this.SelectedTabIndex * tabWidth + 0.5f * tabWidth;
            }

            // TODO: Browser indices are not 0 based...
            // if (touch.Index == 0)
            // {
                if (touch.Phase == TouchPhase.Start)
                {
                    // TODO: Show/Hide SoftKeyboard 
                    // this.RequireKeyboard = !this.RequireKeyboard;

                    this.runScrollInertia = false;
                    this.scrollInertia = Vector.Zero;
                    this.touchPoint = touch.Position;
                    this.Invalidate();

                    this.mousePoint = this.touchPoint;
                }
                else if (touch.Phase == TouchPhase.Move)
                {
                    this.runScrollInertia = false;

                    var delta = touch.Position - this.touchPoint;
                    this.touchPoint = touch.Position;

                    this.scrollInertia = delta;

                    this.scrollPoint += delta;
                    this.Invalidate();

                    this.mousePoint = this.touchPoint;
                }
                else if (touch.Phase == TouchPhase.End)
                {
                    this.scrollInertia *= 0.6f - 0.4f * NFloat.Cos(NFloat.Clamp(this.scrollInertia.Magnitude * 0.2f, 0, NFloat.Pi));

                    this.runScrollInertia = true;
                    this.Invalidate();

                    this.mousePoint = Point.Zero;
                }
            // }
        }

        base.OnTouch(ref e);
    }

    public override void OnAnimationFrame(ref FrameEventRef e)
    {
        NFloat tabWidth = this.SafeArea.Width / 4f;
        this.target = this.SafeArea.Left + this.SelectedTabIndex * tabWidth + 0.5f * tabWidth;

        NFloat newCurrent = SmoothDamp(
            from: current,
            to: target,
            velocity: ref velocity,
            smoothTime: 0.075f,
            maxSpeed: +NFloat.PositiveInfinity,
            deltaTime: (NFloat)e.Delta.TotalSeconds);

        this.tab1Selected = SmoothDamp(
            from: this.tab1Selected,
            to: this.SelectedTabIndex == 0 ? 1 : 0,
            velocity: ref this.tab1SelectionVelocity,
            smoothTime: 0.1f,
            maxSpeed: NFloat.PositiveInfinity,
            deltaTime: (NFloat)e.Delta.TotalSeconds);

        this.tab2Selected = SmoothDamp(
            from: this.tab2Selected,
            to: this.SelectedTabIndex == 1 ? 1 : 0,
            velocity: ref this.tab2SelectionVelocity,
            smoothTime: 0.1f,
            maxSpeed: NFloat.PositiveInfinity,
            deltaTime: (NFloat)e.Delta.TotalSeconds);

        this.tab3Selected = SmoothDamp(
            from: this.tab3Selected,
            to: this.SelectedTabIndex == 2 ? 1 : 0,
            velocity: ref this.tab3SelectionVelocity,
            smoothTime: 0.1f,
            maxSpeed: NFloat.PositiveInfinity,
            deltaTime: (NFloat)e.Delta.TotalSeconds);

        if (NFloat.Abs(newCurrent - target) > 0.0001f)
        {
            this.Invalidate();
            current = newCurrent;
        }

        if (this.runScrollInertia)
        {
            this.scrollPoint += this.scrollInertia;

            // Drag at high speed
            this.scrollInertia *= 0.985f;

            if (this.scrollInertia.Magnitude > 0.15f)
            {
                // Slow speed-down force
                this.scrollInertia -= this.scrollInertia.Normalized * 0.15f;
                this.runScrollInertia = true;
            }
            else
            {
                this.scrollInertia = Vector.Zero;
                this.runScrollInertia = false;
            }

            this.Invalidate();
        }

        var frameFPS = (nint)Math.Round(1f / e.Delta.TotalSeconds);

        if (frameFPS != this.fps)
        {
            this.fps = frameFPS;
            this.maxFps = this.fps;
            this.Invalidate();
        }

        if (frameFPS != this.maxFps)
        {
            this.didIStutter++;
            this.Invalidate();
        }

        base.OnAnimationFrame(ref e);
    }

    public override void Render(ref RenderEventRef render)
    {
        using var ctx = Xui.Core.Actual.Runtime.DrawingContext!;

        var centerGuide = render.Rect.Width / 2f;

        ctx.SetFill(new LinearGradient(
            start: (0, 0 + this.scrollPoint.Y * 1.75f),
            end: (0, 1500 + this.scrollPoint.Y * 1.75f),
            gradient: [
                new (0.45f, 0xFFFFFFFF),
                new (0.60f, 0xFFFFDDFF),
                new (0.75f, 0xFFFFEEFF),
                new (0.90f, 0xFFFFFFFF),
            ]
        ));
        ctx.FillRect(render.Rect);
        ctx.Fill();
        
        // Content
        ctx.SetFill(0x000000FF);
        ctx.TextAlign = TextAlign.Center;
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 18,
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 18
        });
        ctx.FillText("Delightful App Development", (centerGuide, 320 + this.scrollPoint.Y));
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = 400,
            LineHeight = 14
        });
        ctx.FillText("A dotnet UI application framework.", (centerGuide, 348 + this.scrollPoint.Y));

        // Content
        var left = render.Rect.Left;
        var right = render.Rect.Right;

        // Dummy text entry...
        // ctx.SetStroke(Colors.Black);
        // ctx.StrokeRect((left + 5, 470 + 12 + this.scrollPoint.Y, right - left - 5 - 5, 20));
        // ctx.SetFont(new Font() {
        //     FontFamily = ["Inter"],
        //     FontSize = 18,
        //     FontStyle = FontStyle.Normal,
        //     FontWeight = 700,
        //     LineHeight = 18
        // });
        // ctx.TextAlign = TextAlign.Left;
        // ctx.SetFill(Colors.Black);
        // ctx.FillText(this.textContent, (left + 32, 470 + 12 + this.scrollPoint.Y));

        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 18,
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 18
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.SetFill(Colors.Black);
        ctx.FillText("Technology", (left + 32, 495 + 12 + this.scrollPoint.Y));

        var runtimesFrame = new Rect(left + 16, 540 + this.scrollPoint.Y, right - left - 16 - 16, 100);
        ctx.BeginPath();
        ctx.RoundRect(runtimesFrame, 15f);
        ctx.SetFill(0xE8BEEDFF);
        ctx.Fill();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = 400,
            LineHeight = 14
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.SetFill(Colors.Black);
        ctx.FillText("Runtimes", runtimesFrame.TopLeft + (16, 12));

        var canvasFrame = new Rect(left + 16, 660 + this.scrollPoint.Y, right - left - 16 - 16, 200);
        ctx.BeginPath();
        ctx.RoundRect(canvasFrame, 15f);
        ctx.SetFill(0xE8BEEDFF);
        ctx.Fill();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = 400,
            LineHeight = 14
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.SetFill(Colors.Black);
        ctx.FillText("Canvas", canvasFrame.TopLeft + (16, 12));

        var widgetsFrame = new Rect(left + 16, 880 + this.scrollPoint.Y, right - left - 16 - 16, 100);
        ctx.BeginPath();
        ctx.RoundRect(widgetsFrame, 15f);
        ctx.SetFill(0xE8BEEDFF);
        ctx.Fill();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 14,
            FontStyle = FontStyle.Normal,
            FontWeight = 400,
            LineHeight = 14
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.SetFill(Colors.Black);
        ctx.FillText("Widgets", widgetsFrame.TopLeft + (16, 12));

        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 18,
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 18
        });
        ctx.TextAlign = TextAlign.Left;
        ctx.SetFill(Colors.Black);
        ctx.FillText("Applications", (left + 32, 1015 + 12 + this.scrollPoint.Y));

        // Header Overlay
        var headerHeight = this.SafeArea.Top + 44;
        var headerAppearanceProgress = Normalize(-this.scrollPoint.Y, 100, 150);
        var headerAppearanceEasedProgress = EaseInOutQuad(headerAppearanceProgress);
        ctx.Rect((0, 0, render.Rect.Width, headerHeight));
        ctx.SetFill(new Color(1, 0xEE/255f, 0xEE/255f, headerAppearanceEasedProgress));
        ctx.Fill();
        ctx.SetStroke(new Color(0, 0, 0, headerAppearanceEasedProgress));
        ctx.BeginPath();
        ctx.MoveTo((0, headerHeight));
        ctx.LineTo((render.Rect.Width, headerHeight));
        ctx.LineWidth = 0.5f;
        ctx.Stroke();

        // Hamburger
        ctx.Save();
        var hamburgerInContent = new Point(render.Rect.Width * 0.5f + 64f, 420 + this.scrollPoint.Y);
        var hamburgerInTitle = new Point(this.SafeArea.Right - 24f, this.SafeArea.Top + 21f);
        ctx.Translate(Point.Lerp(hamburgerInContent, hamburgerInTitle, EaseInOutQuad(Normalize(-this.scrollPoint.Y, 200, 270))));
        Hamburger.Instance.Render(ctx);
        ctx.Restore();

        // Search
        ctx.Save();
        var spyGlassInContent = new Point(render.Rect.Width * 0.5f, 420 + this.scrollPoint.Y);
        var spyGlassInTitle = new Point(this.SafeArea.Right - 58f, this.SafeArea.Top + 21f);
        ctx.Translate(Point.Lerp(spyGlassInContent, spyGlassInTitle, EaseInOutQuad(Normalize(-this.scrollPoint.Y, 220, 290))));
        SpyGlass.Instance.Render(ctx);
        ctx.Restore();

        // Star
        ctx.Save();
        var starInContent = new Point(render.Rect.Width * 0.5f - 64f, 420 + this.scrollPoint.Y);
        var starInTitle = new Point(this.SafeArea.Right - 92f, this.SafeArea.Top + 21f);
        ctx.Translate(Point.Lerp(starInContent, starInTitle, EaseInOutQuad(Normalize(-this.scrollPoint.Y, 240, 310))));
        Star.Instance.Render(ctx);
        ctx.Restore();

        // Logo
        var logoDisappearProgress = Normalize(-this.scrollPoint.Y, 0, 150);
        var logoScale = NFloat.Lerp(2f, 0.5f, EaseInOutSine(logoDisappearProgress));
        ctx.Save();
        var logoPositionInContent = new Point(centerGuide, 180 + this.scrollPoint.Y);
        var logoPositionInHeader = new Point(this.SafeArea.Left + 38, this.SafeArea.Top + 21);
        var logoHeaderShiftProgress = Normalize(-this.scrollPoint.Y, 70, 140);
        var logoPosition = Point.Lerp(logoPositionInContent, logoPositionInHeader, EaseInOutSine(logoHeaderShiftProgress));
        ctx.Translate(logoPosition);
        ctx.Scale((logoScale, logoScale));
        ctx.Translate((-32, -32));
        XuiLogo.Instance.Render(ctx);
        ctx.Restore();

        // Title
        var titleContentToHeaderProgress = Normalize(-this.scrollPoint.Y, 60, 190);
        var titlePositionInContent = new Point(centerGuide, 286 + this.scrollPoint.Y);
        var titlePositionInHeader = new Point(this.SafeArea.Left + 105, this.SafeArea.Top + 30);
        var titlePosition = Point.Lerp(
            titlePositionInContent,
            titlePositionInHeader,
            EaseInOutQuad(titleContentToHeaderProgress));
        ctx.Save();
        ctx.SetFill(0x000000FF);
        ctx.TextAlign = TextAlign.Center;
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 32f, // NFloat.Lerp(32f, 22f, EaseInOutSine(titleContentToHeaderProgress))
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 32
        });
        var titleScale = NFloat.Lerp(1f, 22f / 32f, EaseInOutSine(titleContentToHeaderProgress));
        ctx.Translate(titlePosition);
        ctx.Scale((titleScale, titleScale));
        ctx.TextBaseline = TextBaseline.Alphabetic;
        ctx.FillText("XuiApps", (0, 0));
        ctx.Restore();

        // Settings
        
        // ctx.BeginPath();
        // ctx.PathData()
        //     .M((10, 30))
        //     .A((20, 20), 0, ArcFlag.Small, Winding.ClockWise, (50, 30))
        //     .A((20, 20), 0, ArcFlag.Small, Winding.ClockWise, (90, 30))
        //     .Q((90, 60), (50, 90))
        //     .Q((10, 60), (10, 30))
        //     .Z();
        // ctx.SetFill(0xFF0000FF);
        // ctx.Fill();

        // Tab view...

        // Footer Overlay
        var tabViewTop = this.SafeArea.Bottom - 44;
        var tabViewHeight = this.DisplayArea.Bottom - tabViewTop;
        ctx.Rect((0, tabViewTop, render.Rect.Width, tabViewHeight));
        ctx.SetFill(new Color(1, 0xEE/255f, 0xEE/255f, 1));
        ctx.Fill();
        ctx.SetStroke(new Color(0, 0, 0, 1));
        ctx.BeginPath();
        ctx.MoveTo((0, tabViewTop));
        ctx.LineTo((render.Rect.Width, tabViewTop));
        ctx.LineWidth = 0.5f;
        ctx.Stroke();

        // Tab view selection
        // ctx.SetStroke(Colors.Black);
        // ctx.StrokeRect(new Rect(target - 22.5f, this.SafeArea.Bottom - 40f, 45f, 45f));
        ctx.SetFill(0xD642CDFF);
        ctx.BeginPath();
        ctx.RoundRect((current - 10f, this.SafeArea.Bottom - 44f, 20f, 3f), 1.5f);
        ctx.Fill();

        var width = this.SafeArea.Width;
        var tabWidth = width / 4;

        ctx.Save();
        ctx.Translate((this.SafeArea.Left + tabWidth / 2 + 0 * tabWidth, this.SafeArea.Bottom - 25));
        Home.Instance.Selected = this.tab1Selected;
        Home.Instance.Color = Colors.Black; // 0x8A05FFFF;
        Home.Instance.SelectedColor = 0x8A05FFFF; //0xD642CDFF;
        Home.Instance.Render(ctx);
        ctx.Restore();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 10f, // NFloat.Lerp(32f, 22f, EaseInOutSine(titleContentToHeaderProgress))
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 10
        });
        ctx.SetFill(Colors.Black);
        ctx.TextAlign = TextAlign.Center;
        ctx.FillText("Home", (this.SafeArea.Left + tabWidth / 2 + 0 * tabWidth, this.SafeArea.Bottom - 12));

        ctx.Save();
        ctx.Translate((1 * tabWidth + this.SafeArea.Left + tabWidth / 2, this.SafeArea.Bottom - 24));
        LocationPin.Instance.Selected = this.tab2Selected;
        LocationPin.Instance.Color = Colors.Black;
        LocationPin.Instance.SelectedColor = 0x8A05FFFF;
        LocationPin.Instance.Render(ctx);
        ctx.Restore();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 10f,
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 10
        });
        ctx.SetFill(Colors.Black);
        ctx.TextAlign = TextAlign.Center;
        ctx.FillText("Locations", (1 * tabWidth + this.SafeArea.Left + tabWidth / 2, this.SafeArea.Bottom - 12));

        ctx.Save();
        ctx.Translate((2 * tabWidth + this.SafeArea.Left + tabWidth / 2, this.SafeArea.Bottom - 24));
        Wallet.Instance.Selected = this.tab3Selected;
        Wallet.Instance.Color = Colors.Black;
        Wallet.Instance.SelectedColor = 0x8A05FFFF;
        Wallet.Instance.Render(ctx);
        ctx.Restore();
        ctx.SetFont(new Font() {
            FontFamily = ["Inter"],
            FontSize = 10f,
            FontStyle = FontStyle.Normal,
            FontWeight = 700,
            LineHeight = 10
        });
        ctx.SetFill(Colors.Black);
        ctx.TextAlign = TextAlign.Center;
        ctx.FillText("Card", (2 * tabWidth + this.SafeArea.Left + tabWidth / 2, this.SafeArea.Bottom - 12));

        base.Render(ref render);

        return;
    }

    public override void InsertText(ref InsertTextEventRef eventRef)
    {
        this.textContent += eventRef.Text;
        this.Invalidate();
        base.InsertText(ref eventRef);
    }

    public override void DeleteBackwards(ref DeleteBackwardsEventRef eventRef)
    {
        this.textContent = this.textContent.Substring(0, this.textContent.Length - 1);
        this.Invalidate();
        base.DeleteBackwards(ref eventRef);
    }
}
