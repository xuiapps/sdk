using System;
using System.Runtime.InteropServices;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Animation;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Middleware.Emulator.Devices;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;

namespace Xui.Middleware.Emulator.Actual;

/// <summary>
/// A middleware window that wraps a platform window to simulate a mobile device environment
/// when running on desktop platforms like Windows or macOS.
///
/// <para>
/// This class implements both <see cref="Xui.Core.Abstract.IWindow"/> and <see cref="Xui.Core.Actual.IWindow"/>,
/// acting as a bridge between the abstract UI and the actual system window.
/// It also implements <see cref="Xui.Core.Abstract.IWindow.IDesktopStyle"/> to provide emulator-specific
/// chrome styling and sizing.
/// </para>
///
/// <para>
/// The window draws a rounded-rectangle phone frame, overlays controls like battery and signal indicators,
/// and translates desktop mouse events into synthetic mobile touch input.
/// </para>
/// </summary>
public partial class EmulatorWindow : Xui.Core.Abstract.IWindow, Xui.Core.Actual.IWindow, Xui.Core.Abstract.IWindow.IDesktopStyle
{
    private Point? leftMouseButtonTouch = null;

    public EmulatorWindow()
    {
    }

    /// <summary>
    /// The abstract window instance from the application layer.
    /// </summary>
    public Xui.Core.Abstract.IWindow? Abstract { get; set; }

    /// <summary>
    /// The underlying platform window from the base runtime.
    /// </summary>
    public Xui.Core.Actual.IWindow? Platform { get; set; }

    /// <summary>
    /// Gets whether the emulator should appear without OS window chrome.
    /// </summary>
    WindowBackdrop Xui.Core.Abstract.IWindow.IDesktopStyle.Backdrop => WindowBackdrop.Chromeless;

    /// <summary>
    /// Controls the window stacking level (Z-order) relative to other windows.
    /// </summary>
    DesktopWindowLevel Xui.Core.Abstract.IWindow.IDesktopStyle.Level => DesktopWindowLevel.Floating;

    /// <summary>
    /// Gets the preferred size of the emulator window at startup.
    /// </summary>
    Size? Xui.Core.Abstract.IWindow.IDesktopStyle.StartupSize => new (330, 740);

#region Platform.IWindow
    string Xui.Core.Actual.IWindow.Title { get => Platform!.Title; set => Platform!.Title = value; }

    void Xui.Core.Actual.IWindow.Invalidate() => Platform!.Invalidate();

    void Xui.Core.Actual.IWindow.Show() => Platform!.Show();

    /// <summary>
    /// The display area available to the app inside the mobile frame (excluding chrome).
    /// </summary>
    public Rect DisplayArea { get; set; }

    /// <summary>
    /// The safe area that excludes OS bars, camera notches, etc.
    /// </summary>
    public Rect SafeArea { get; set; }

    /// <summary>
    /// The corner radius of the emulated device screen. Forwarded to the abstract window
    /// so views (e.g. ScrollView) can inset UI elements away from rounded screen edges.
    /// </summary>
    public NFloat ScreenCornerRadius { get; set; }

    /// <summary>
    /// The device profile currently displayed in the emulator. Defaults to the first entry in
    /// <see cref="DeviceCatalog.All"/> (iPhone 15 Pro).
    /// </summary>
    public DeviceProfile CurrentDevice { get; set; } = DeviceCatalog.All[0];
#endregion

#region Abstract.IWindow
    /// <summary>
    /// Whether the virtual keyboard is requested by the app.
    /// </summary>
    public bool RequireKeyboard { get; set; }

    void Xui.Core.Abstract.IWindow.Closed() => Abstract!.Closed();

    bool Xui.Core.Abstract.IWindow.Closing() => Abstract!.Closing();

    void Xui.Core.Abstract.IWindow.OnAnimationFrame(ref FrameEventRef animationFrame)
    {
        Abstract!.OnAnimationFrame(ref animationFrame);
    }

    /// <summary>
    /// Translates desktop mouse down events into synthetic mobile touch + forwards to the app.
    /// </summary>
    void Xui.Core.Abstract.IWindow.OnMouseDown(ref MouseDownEventRef evRef)
    {
        MouseDownEventRef evMobile = new MouseDownEventRef()
        {
            Position = evRef.Position + (-8, -52 - 8 - 8),
            Button = evRef.Button
        };
        this.Abstract!.OnMouseDown(ref evMobile);

        // Synthetic touch
        this.leftMouseButtonTouch = evMobile.Position;
        var touchEventRef = new TouchEventRef([new()
        {
            Index = 0,
            Phase = TouchPhase.Start,
            Position = evMobile.Position,
            Radius = 0.5f
        }]);
        this.Abstract!.OnTouch(ref touchEventRef);
    }

    /// <summary>
    /// Translates desktop mouse move events into synthetic mobile touch move.
    /// </summary>
    void Xui.Core.Abstract.IWindow.OnMouseMove(ref MouseMoveEventRef evRef)
    {
        MouseMoveEventRef evMobile = new MouseMoveEventRef()
        {
            Position = evRef.Position + (-8, -52 - 8 - 8)
        };
        Abstract!.OnMouseMove(ref evMobile);

        // Synthetic touch
        if (this.leftMouseButtonTouch.HasValue)
        {
            this.leftMouseButtonTouch = evMobile.Position;
            var touchEventRef = new TouchEventRef([new()
            {
                Index = 0,
                Phase = TouchPhase.Move,
                Position = evMobile.Position,
                Radius = 0.5f
            }]);
            this.Abstract!.OnTouch(ref touchEventRef);
        }
    }

    /// <summary>
    /// Translates mouse up events to mobile-style touch end.
    /// </summary>
    void Xui.Core.Abstract.IWindow.OnMouseUp(ref MouseUpEventRef evRef)
    {
        MouseUpEventRef evMobile = new MouseUpEventRef()
        {
            Position = evRef.Position + (-8, -52 - 8 - 8),
            Button = evRef.Button
        };
        Abstract!.OnMouseUp(ref evMobile);

        // Synthetic touch
        if (this.leftMouseButtonTouch.HasValue)
        {
            this.leftMouseButtonTouch = null;
            var touchEventRef = new TouchEventRef([new()
            {
                Index = 0,
                Phase = TouchPhase.End,
                Position = evMobile.Position,
                Radius = 0.5f
            }]);
            this.Abstract!.OnTouch(ref touchEventRef);
        }
    }

    void Xui.Core.Abstract.IWindow.OnScrollWheel(ref ScrollWheelEventRef evRef)
    {
        Abstract!.OnScrollWheel(ref evRef);
    }

    void Xui.Core.Abstract.IWindow.OnTouch(ref TouchEventRef touchEventRef)
    {
        Abstract!.OnTouch(ref touchEventRef);
    }

    /// <summary>
    /// Renders the emulator UI frame, status bar elements, and mobile display area.
    /// Delegates the actual app content rendering to the abstract window inside a clipped canvas.
    /// </summary>
    void Xui.Core.Abstract.IWindow.Render(ref RenderEventRef render)
    {
        NFloat titleHeight = 52f;
        NFloat gap = 8f;

        NFloat borderWidth = 8f;
        NFloat borderOutline = 2.5f; // Included in borderWidth
        NFloat screenCornerRadius = (NFloat)CurrentDevice.ScreenCornerRadius;

        Rect titleRect = new(0, 0, render.Rect.Width, titleHeight);
        Rect emulatorRect = new(
            x: borderWidth,
            y: titleHeight + gap + borderWidth,
            width: render.Rect.Width - borderWidth - borderWidth,
            height: render.Rect.Height - titleHeight - gap - borderWidth - borderWidth);

        using (var ctx = Xui.Core.Actual.Runtime.DrawingContext!)
        {
            ctx.Save();

            ctx.BeginPath();
            ctx.RoundRect(emulatorRect, screenCornerRadius);
            ctx.Clip();

            ctx.BeginPath();
            ctx.Translate((borderWidth, titleHeight + gap + borderWidth));

            RenderEventRef emulatorRender = new RenderEventRef(
                rect: new Rect(0, 0, emulatorRect.Width, emulatorRect.Height),
                frame: render.Frame
            );

            ctx.SetFill(Colors.White);
            ctx.FillRect(emulatorRender.Rect);

            Abstract!.DisplayArea = emulatorRender.Rect;
            Abstract!.SafeArea = emulatorRender.Rect - new Frame(40, 0, 20, 0);
            Abstract!.ScreenCornerRadius = CurrentDevice.ScreenCornerRadius;

            Abstract!.Render(ref emulatorRender);
        }

        using (var ctx = Xui.Core.Actual.Runtime.DrawingContext!)
        {
            ctx.Restore();

            if (this.leftMouseButtonTouch.HasValue)
            {
                ctx.Save();
                ctx.BeginPath();
                ctx.RoundRect(emulatorRect, screenCornerRadius);
                ctx.Clip();

                ctx.Translate((borderWidth, titleHeight + gap + borderWidth));

                ctx.BeginPath();
                ctx.Ellipse(this.leftMouseButtonTouch.Value, 15f, 15f, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
                ctx.SetFill(0x66888888);
                ctx.Fill();

                ctx.BeginPath();
                ctx.Ellipse(this.leftMouseButtonTouch.Value, 15f, 15f, 0, 0, NFloat.Pi * 2, Winding.ClockWise);
                ctx.LineWidth = 3f;
                ctx.SetStroke(0x88AAAAAA);
                ctx.Stroke();

                ctx.Restore();
            }

            // Outer device frame stroke
            ctx.BeginPath();
            ctx.RoundRect(
                rect: emulatorRect.Expand(borderWidth * 0.5f),
                radius: screenCornerRadius + borderWidth * 0.5f);
            ctx.SetStroke(new Color(0x111111FF));
            ctx.LineWidth = borderWidth;
            ctx.Stroke();

            ctx.BeginPath();
            ctx.RoundRect(
                rect: emulatorRect.Expand(borderWidth - borderOutline * 0.5f),
                radius: screenCornerRadius + borderWidth * 0.5f + 0.25f * borderOutline);
            ctx.SetStroke(new Color(0x444444FF));
            ctx.LineWidth = borderOutline;
            ctx.Stroke();

            NFloat phoneToTabletT = Easing.Normalize(render.Rect.Width, 500, 575);

            // Icons (refactored)
            PinholeCutout.Instance.Render(ctx, (
                render.Rect.Width / 2f - 45f,
                titleHeight + gap + borderWidth + 12f
            ));

            NFloat clockX = NFloat.Lerp(
                (render.Rect.Width / 2f - 22f) / 2,
                (300 / 2f - 22f) / 2,
                Easing.EaseInOutSine(phoneToTabletT));
            ClockIcon.Instance.Render(ctx, (clockX, titleHeight + gap + borderWidth + 18));

            NFloat instrumentsX = NFloat.Lerp(
                render.Rect.Width / 2f + 45f + (render.Rect.Width / 2f - 45f - 22f) / 2f,
                render.Rect.Width - 80f,
                Easing.EaseInOutSine(phoneToTabletT));

            SignalStrengthIcon.Instance.Render(ctx, (instrumentsX - 12f - 8f, titleHeight + gap + borderWidth + 18 + 13.5f));
            BatteryIcon.Instance.Render(ctx, (instrumentsX - 8f, titleHeight + gap + borderWidth + 18 + 2.5f));
            FiveGIcon.Instance.Render(ctx, (instrumentsX + 36f - 8f, titleHeight + gap + borderWidth + 18));

            // Menu Handle
            MenuHandle.Instance.Render(ctx, (
                render.Rect.Width / 2f,
                render.Rect.Height - borderWidth - 3f
            ));

            // Title background
            ctx.BeginPath();
            ctx.RoundRect(titleRect, 10f);
            ctx.SetFill(new Color(0x333333FF));
            ctx.Fill();
        }
    }

    void Xui.Core.Abstract.IWindow.WindowHitTest(ref WindowHitTestEventRef evRef)
    {
        NFloat titleHeight = 52f;
        NFloat gap = 8f;
        NFloat borderWidth = 8f;
        NFloat screenCornerRadius = (NFloat)CurrentDevice.ScreenCornerRadius;

        var point = evRef.Point;

        if (point.Y < titleHeight)
        {
            evRef.Area = WindowHitTestEventRef.WindowArea.Title;
        }

        Rect emulatorRect = new Rect(
            evRef.Window.Left,
            evRef.Window.Top + titleHeight + gap,
            evRef.Window.Width,
            evRef.Window.Height - titleHeight - gap);

        if (emulatorRect.Contains(point))
        {
            var topLeftCenter = emulatorRect.TopLeft + (screenCornerRadius, screenCornerRadius);
            var topRightCenter = emulatorRect.TopRight + (-screenCornerRadius, screenCornerRadius);
            var bottomLeftCenter = emulatorRect.BottomLeft + (screenCornerRadius, -screenCornerRadius);
            var bottomRightCenter = emulatorRect.BottomRight + (-screenCornerRadius, -screenCornerRadius);

            NFloat signedBorderDistance;

            if (point.X < topLeftCenter.X && point.Y < topLeftCenter.Y)
            {
                signedBorderDistance = (point - topLeftCenter).Magnitude - screenCornerRadius;
            }
            else if (point.X > topRightCenter.X && point.Y < topRightCenter.Y)
            {
                signedBorderDistance = (point - topRightCenter).Magnitude - screenCornerRadius;
            }
            else if (point.X < bottomLeftCenter.X && point.Y > bottomLeftCenter.Y)
            {
                signedBorderDistance = (point - bottomLeftCenter).Magnitude - screenCornerRadius;
            }
            else if (point.X > bottomRightCenter.X && point.Y > bottomRightCenter.Y)
            {
                signedBorderDistance = (point - bottomRightCenter).Magnitude - screenCornerRadius;
            }
            else
            {
                signedBorderDistance = NFloat.Max(
                    NFloat.Max(emulatorRect.Left - point.X, point.X - emulatorRect.Right),
                    NFloat.Max(emulatorRect.Top - point.Y, point.Y - emulatorRect.Bottom)
                );
            }

            if (-borderWidth <= signedBorderDistance && signedBorderDistance <= 0)
            {
                NFloat resizeRect = 48f;

                if (point.X <= emulatorRect.Left + resizeRect && point.Y <= emulatorRect.Top + resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderTopLeft;
                else if (point.X >= emulatorRect.Right - resizeRect && point.Y <= emulatorRect.Top + resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderTopRight;
                else if (point.X >= emulatorRect.Right - resizeRect && point.Y >= emulatorRect.Bottom - resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottomRight;
                else if (point.X <= emulatorRect.Left + resizeRect && point.Y >= emulatorRect.Bottom - resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottomLeft;
                else if (point.Y <= emulatorRect.Top + resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderTop;
                else if (point.Y >= emulatorRect.Bottom - resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderBottom;
                else if (point.X <= emulatorRect.Left + resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderLeft;
                else if (point.X >= emulatorRect.Right - resizeRect)
                    evRef.Area = WindowHitTestEventRef.WindowArea.BorderRight;
                else
                    evRef.Area = WindowHitTestEventRef.WindowArea.Client;
            }
            else if (signedBorderDistance <= 0)
            {
                evRef.Area = WindowHitTestEventRef.WindowArea.Client;
            }
            else
            {
                evRef.Area = WindowHitTestEventRef.WindowArea.Transparent;
            }
        }
    }

    public void OnKeyDown(ref KeyEventRef e) =>
        this.Abstract?.OnKeyDown(ref e);

    public void OnChar(ref KeyEventRef e) =>
        this.Abstract?.OnChar(ref e);

    #endregion
}