using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Window = Xui.Core.Abstract.Window;
using static Xui.Core.Canvas.Colors;
using System.Runtime.InteropServices;

namespace Xui.Apps.ClockApp;

public enum MouseLocation
{
    Elsewhere,
    RunButton
}

public class MainWindow : Window, IWindow.IDesktopStyle
{
    private NFloat HeaderHeight = 30;
    private NFloat BorderSize = 5;
    private NFloat CornerSize = 10;
    private NFloat ButtonWidth = 50;
    private NFloat ButtonHeight = 20;
    private NFloat ButtonMargin = 5;
    private NFloat ButtonCornerRadius = 6;

    private Point mousePosition;
    private MouseLocation mouseLocation = MouseLocation.Elsewhere;
    private Rect runButtonRect;
    private bool running = false;

    public MainWindow(IServiceProvider context) : base(context) { }

    IWindow.IDesktopStyle.WindowBackdrop IWindow.IDesktopStyle.Backdrop => IWindow.IDesktopStyle.WindowBackdrop.Chromeless;

    Core.Math2D.Size? IWindow.IDesktopStyle.StartupSize => new Core.Math2D.Size(200, 300);

    public override void OnMouseMove(ref MouseMoveEventRef e)
    {
        mousePosition = e.Position;

        var newLocation = MouseLocation.Elsewhere;
        if (runButtonRect.Contains(mousePosition))
            newLocation = MouseLocation.RunButton;

        if (newLocation != mouseLocation)
        {
            mouseLocation = newLocation;
            this.Invalidate();
        }

        base.OnMouseMove(ref e);
    }

    public override void OnMouseDown(ref MouseDownEventRef e)
    {
        if (runButtonRect.Contains(e.Position))
        {
            running = !running;
            this.Invalidate();
        }

        base.OnMouseDown(ref e);
    }

    private Rect GetRunButtonRect(Rect windowRect)
    {
        var inset = windowRect - 2.5;
        var centerX = inset.Left + inset.Width / 2;
        return new Rect(
            centerX - ButtonWidth / 2,
            inset.Top + HeaderHeight + ButtonMargin,
            ButtonWidth,
            ButtonHeight);
    }

    public override void Render(ref RenderEventRef renderEventRef)
    {
        // Console.WriteLine("Render");
        using var ctx = Xui.Core.Actual.Runtime.DrawingContext!;

        var rect = renderEventRef.Rect;
        var insetRect = rect - 2.5;

        // Window background
        ctx.SetFill(White);
        ctx.RoundRect(insetRect, 25);
        ctx.Fill();

        ctx.Save();
        ctx.BeginPath();
        ctx.RoundRect(insetRect, 25);
        ctx.Clip();

        // Header background
        ctx.SetFill(new Color(0.95f, 0.95f, 0.95f, 1f));
        ctx.FillRect(new Rect(insetRect.Left, insetRect.Top, insetRect.Width, HeaderHeight));

        // Header title (centered)
        ctx.SetFill(Black);
        ctx.SetFont(new Font
        {
            FontFamily = ["Inter"],
            FontSize = 14,
            FontWeight = 600,
            LineHeight = 14
        });
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;
        ctx.FillText("Xui ClockApp", new Point(insetRect.Left + insetRect.Width / 2, insetRect.Top + HeaderHeight / 2));

        // Run/Stop button
        runButtonRect = GetRunButtonRect(rect);
        var btnHovered = runButtonRect.Contains(mousePosition);
        ctx.BeginPath();
        ctx.RoundRect(runButtonRect, ButtonCornerRadius);
        ctx.SetFill(btnHovered ? Orange : new Color(0.85f, 0.85f, 0.85f, 1f));
        ctx.Fill();
        ctx.SetFill(Black);
        ctx.SetFont(new Font
        {
            FontFamily = ["Segoe UI", "Inter"],
            FontSize = 11,
            FontWeight = 500,
            LineHeight = 11
        });
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;
        ctx.FillText(running ? "Stop" : "Run", runButtonRect.Center);

        // Header separator line
        ctx.SetStroke(new Color(0.8f, 0.8f, 0.8f, 1f));
        ctx.LineWidth = 1;
        ctx.BeginPath();
        ctx.MoveTo(new Point(insetRect.Left, insetRect.Top + HeaderHeight));
        ctx.LineTo(new Point(insetRect.Right, insetRect.Top + HeaderHeight));
        ctx.Stroke();

        // Clock area: below button to bottom of window
        var clockTop = runButtonRect.Bottom + ButtonMargin;
        var clockAreaWidth = insetRect.Width;
        var clockAreaHeight = insetRect.Bottom - clockTop;
        var clockCenterX = insetRect.Left + insetRect.Width / 2;
        var clockCenterY = clockTop + clockAreaHeight / 2;
        var clockCenter = new Point(clockCenterX, clockCenterY);
        var clockRadius = NFloat.Min(clockAreaWidth, clockAreaHeight) / 2 - 10;

        // Get current time
        var now = DateTime.Now;
        var hours = now.Hour % 12;
        var minutes = now.Minute;
        var seconds = now.Second;
        var milliseconds = now.Millisecond;

        // Clock face circle
        ctx.BeginPath();
        // ctx.Arc(clockCenter, clockRadius, 0, NFloat.Pi * 2);
        ctx.SetFill(new Color(0.98f, 0.98f, 0.98f, 1f));
        ctx.Fill();
        ctx.SetStroke(new Color(0.7f, 0.7f, 0.7f, 1f));
        ctx.LineWidth = 2;
        ctx.Stroke();

        // Draw hour markers
        ctx.SetFont(new Font
        {
            FontFamily = ["Inter"],
            FontSize = 12,
            FontWeight = 600,
            LineHeight = 12
        });
        ctx.TextAlign = TextAlign.Center;
        ctx.TextBaseline = TextBaseline.Middle;
        ctx.SetFill(Black);

        for (int i = 1; i <= 12; i++)
        {
            var angle = (i * 30 - 90) * NFloat.Pi / 180; // 30 degrees per hour, -90 to start at 12
            var markerRadius = clockRadius - 15;
            var markerX = clockCenterX + NFloat.Cos(angle) * markerRadius;
            var markerY = clockCenterY + NFloat.Sin(angle) * markerRadius;

            if (i == 12 || i == 3 || i == 6 || i == 9)
            {
                // Draw numbers at 12, 3, 6, 9
                ctx.FillText(i.ToString(), new Point(markerX, markerY));
            }
            else
            {
                // Draw dots at 1, 2, 4, 5, 7, 8, 10, 11
                ctx.SetFill(Black);
                ctx.FillRect(new Rect(markerX - (NFloat)1.5, markerY - (NFloat)1.5, 3, 3));
            }
        }

        // Hour hand
        var hourAngle = ((hours + minutes / 60.0) * 30 - 90.0) * NFloat.Pi / 180.0;
        var hourLength = clockRadius * 0.5;
        ctx.BeginPath();
        ctx.MoveTo(clockCenter);
        ctx.LineTo(new Point(
            clockCenterX + NFloat.Cos((NFloat)hourAngle) * (NFloat)hourLength,
            clockCenterY + NFloat.Sin((NFloat)hourAngle) * (NFloat)hourLength));
        ctx.SetStroke(Black);
        ctx.LineWidth = 4;
        ctx.Stroke();

        // Minute hand
        var minuteAngle = ((minutes + seconds / 60.0) * 6 - 90.0) * NFloat.Pi / 180.0;
        var minuteLength = clockRadius * 0.7;
        ctx.BeginPath();
        ctx.MoveTo(clockCenter);
        ctx.LineTo(new Point(
            clockCenterX + NFloat.Cos((NFloat)minuteAngle) * (NFloat)minuteLength,
            clockCenterY + NFloat.Sin((NFloat)minuteAngle) * (NFloat)minuteLength));
        ctx.SetStroke(Black);
        ctx.LineWidth = 3;
        ctx.Stroke();

        // Second hand
        var secondAngle = ((seconds + milliseconds / 1000.0) * 6 - 90.0) * NFloat.Pi / 180.0;
        var secondLength = clockRadius * 0.85;
        ctx.BeginPath();
        ctx.MoveTo(clockCenter);
        ctx.LineTo(new Point(
            clockCenterX + NFloat.Cos((NFloat)secondAngle) * (NFloat)secondLength,
            clockCenterY + NFloat.Sin((NFloat)secondAngle) * (NFloat)secondLength));
        ctx.SetStroke(new Color(0.8f, 0.2f, 0.2f, 1f));
        ctx.LineWidth = 2;
        ctx.Stroke();

        // Millisecond hand
        var msAngle = (milliseconds / 1000.0 * 360.0 - 90.0) * NFloat.Pi / 180.0;
        var msLength = clockRadius * 0.6;
        ctx.BeginPath();
        ctx.MoveTo(clockCenter);
        ctx.LineTo(new Point(
            clockCenterX + NFloat.Cos((NFloat)msAngle) * (NFloat)msLength,
            clockCenterY + NFloat.Sin((NFloat)msAngle) * (NFloat)msLength));
        ctx.SetStroke(new Color(0.2f, 0.5f, 0.8f, 1f));
        ctx.LineWidth = 1;
        ctx.Stroke();

        // Center dot
        ctx.SetFill(Black);
        ctx.FillRect(new Rect(clockCenterX - (NFloat)2.5, clockCenterY - (NFloat)2.5, 5, 5));

        ctx.Restore();

        // Window border
        ctx.BeginPath();
        ctx.LineWidth = 5;
        ctx.SetStroke(Gray);
        ctx.RoundRect(insetRect, 25);
        ctx.Stroke();

        base.Render(ref renderEventRef);

        // Continue animation loop if running
        if (running)
        {
            this.Invalidate();
        }
    }

    public override void OnAnimationFrame(ref FrameEventRef e)
    {
        // Console.WriteLine("OnAnimationFrame");
        // this.Invalidate();
        base.OnAnimationFrame(ref e);
    }

    public override void WindowHitTest(ref WindowHitTestEventRef evRef)
    {
        base.WindowHitTest(ref evRef);

        var point = evRef.Point;
        var window = evRef.Window;
        var insetRect = window - 2.5;

        // Check if point is outside the window
        if (!window.Contains(point))
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.Transparent;
            return;
        }

        // Check resize borders first (corners take priority)
        bool nearLeft = point.X < window.Left + BorderSize;
        bool nearRight = point.X > window.Right - BorderSize;
        bool nearTop = point.Y < window.Top + BorderSize;
        bool nearBottom = point.Y > window.Bottom - BorderSize;

        bool nearCornerLeft = point.X < window.Left + CornerSize;
        bool nearCornerRight = point.X > window.Right - CornerSize;
        bool nearCornerTop = point.Y < window.Top + CornerSize;
        bool nearCornerBottom = point.Y > window.Bottom - CornerSize;

        // Corner hit tests
        if (nearCornerTop && nearCornerLeft)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderTopLeft;
            return;
        }
        if (nearCornerTop && nearCornerRight)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderTopRight;
            return;
        }
        if (nearCornerBottom && nearCornerLeft)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottomLeft;
            return;
        }
        if (nearCornerBottom && nearCornerRight)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottomRight;
            return;
        }

        // Edge hit tests
        if (nearTop)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderTop;
            return;
        }
        if (nearBottom)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottom;
            return;
        }
        if (nearLeft)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderLeft;
            return;
        }
        if (nearRight)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.BorderRight;
            return;
        }

        // Check button (client area for interaction)
        var buttonRect = GetRunButtonRect(window);

        if (buttonRect.Contains(point))
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.Client;
            return;
        }

        // Check title bar (header area, excluding buttons)
        if (point.Y < insetRect.Top + HeaderHeight)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.Title;
            return;
        }

        // Everything else is client area
        evRef.Area = WindowHitTestEventRef.WindowArea.Client;
    }
}
