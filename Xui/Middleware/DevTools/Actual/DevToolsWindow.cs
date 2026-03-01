using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Middleware.DevTools.IO;
using Xui.Runtime.Software.Actual;

namespace Xui.Middleware.DevTools.Actual;

/// <summary>
/// A middleware window inserted between the abstract application window and the real platform window.
/// Implements both <see cref="Xui.Core.Abstract.IWindow"/> and <see cref="Xui.Core.Actual.IWindow"/>,
/// delegating in both directions while adding DevTools capabilities:
/// SVG screenshot capture, UI-tree inspection, and synthetic input injection.
/// </summary>
internal sealed class DevToolsWindow : Xui.Core.Abstract.IWindow, Xui.Core.Actual.IWindow
{
    private readonly DevToolsPlatform _platform;

    // Screenshot state — written and read exclusively on the UI thread.
    private TaskCompletionSource<string>? _pendingScreenshot;
    private MemoryStream? _svgStream;
    private Rect _pendingRect;

    // Overlay state: last interaction point and identity label, shown on every frame until replaced.
    private (Point pos, bool isTouch)? _lastInputOverlay;
    private string? _overlayLabel;

    /// <summary>The abstract application window (app layer).</summary>
    public Xui.Core.Abstract.IWindow? Abstract { get; set; }

    /// <summary>The underlying real platform window.</summary>
    public Xui.Core.Actual.IWindow? Platform { get; set; }

    public DevToolsWindow(DevToolsPlatform platform) => _platform = platform;

    // ── Actual.IWindow ────────────────────────────────────────────────────────

    string Xui.Core.Actual.IWindow.Title
    {
        get => Platform!.Title;
        set => Platform!.Title = value;
    }

    void Xui.Core.Actual.IWindow.Show()       => Platform!.Show();
    void Xui.Core.Actual.IWindow.Invalidate() => Platform!.Invalidate();
    void Xui.Core.Actual.IWindow.Close()      => Platform!.Close();

    bool Xui.Core.Actual.IWindow.RequireKeyboard
    {
        get => Platform!.RequireKeyboard;
        set => Platform!.RequireKeyboard = value;
    }

    Xui.Core.Canvas.ITextMeasureContext? Xui.Core.Actual.IWindow.TextMeasureContext
        => Platform!.TextMeasureContext;

    /// <summary>
    /// Returns a <see cref="SplicingContext"/> when a screenshot is pending so the next
    /// render pass writes simultaneously to the real display and an SVG capture stream.
    /// </summary>
    object? Xui.Core.Actual.IWindow.GetService(Type t)
    {
        if (t != typeof(IContext))
            return Platform!.GetService(t);

        IContext ctx;
        if (_pendingScreenshot != null && _svgStream != null)
        {
            var realCtx = _platform.Base.DrawingContext;
            var svgCtx  = new SvgDrawingContext(
                new Size(_pendingRect.Width, _pendingRect.Height),
                _svgStream,
                keepOpen: true);
            ctx = new SplicingContext(realCtx, svgCtx);
        }
        else
        {
            ctx = (Platform!.GetService(t) as IContext) ?? _platform.Base.DrawingContext;
        }

        // Wrap with OverlayContext to draw the last interaction point on every frame.
        if (_lastInputOverlay is { } overlay)
            return new OverlayContext(ctx, overlay.pos, overlay.isTouch, _overlayLabel);

        return ctx;
    }

    // ── Abstract.IWindow ──────────────────────────────────────────────────────

    Rect Xui.Core.Abstract.IWindow.DisplayArea
    {
        get => Abstract!.DisplayArea;
        set => Abstract!.DisplayArea = value;
    }

    Rect Xui.Core.Abstract.IWindow.SafeArea
    {
        get => Abstract!.SafeArea;
        set => Abstract!.SafeArea = value;
    }

    nfloat Xui.Core.Abstract.IWindow.ScreenCornerRadius
    {
        get => Abstract!.ScreenCornerRadius;
        set => Abstract!.ScreenCornerRadius = value;
    }

    void Xui.Core.Abstract.IWindow.Closed()  => Abstract!.Closed();
    bool Xui.Core.Abstract.IWindow.Closing() => Abstract!.Closing();

    void Xui.Core.Abstract.IWindow.OnAnimationFrame(ref FrameEventRef e)   => Abstract!.OnAnimationFrame(ref e);
    void Xui.Core.Abstract.IWindow.OnMouseDown(ref MouseDownEventRef e)    => Abstract!.OnMouseDown(ref e);
    void Xui.Core.Abstract.IWindow.OnMouseMove(ref MouseMoveEventRef e)    => Abstract!.OnMouseMove(ref e);
    void Xui.Core.Abstract.IWindow.OnMouseUp(ref MouseUpEventRef e)        => Abstract!.OnMouseUp(ref e);
    void Xui.Core.Abstract.IWindow.OnScrollWheel(ref ScrollWheelEventRef e) => Abstract!.OnScrollWheel(ref e);
    void Xui.Core.Abstract.IWindow.OnTouch(ref TouchEventRef e)            => Abstract!.OnTouch(ref e);
    void Xui.Core.Abstract.IWindow.WindowHitTest(ref WindowHitTestEventRef e) => Abstract!.WindowHitTest(ref e);
    void Xui.Core.Abstract.IWindow.OnKeyDown(ref KeyEventRef e) => Abstract!.OnKeyDown(ref e);
    void Xui.Core.Abstract.IWindow.OnChar(ref KeyEventRef e)    => Abstract!.OnChar(ref e);

    /// <summary>
    /// Intercepts the render call: if a screenshot is pending, allocates the capture stream
    /// and rect before delegating to the abstract window, then reads the completed SVG after.
    /// </summary>
    void Xui.Core.Abstract.IWindow.Render(ref RenderEventRef render)
    {
        if (_pendingScreenshot != null)
        {
            _svgStream   = new MemoryStream();
            _pendingRect = render.Rect;
        }

        Abstract!.Render(ref render);

        // At this point the render is done and the SplicingContext (if any) has been
        // disposed — SvgDrawingContext.Dispose() writes the closing SVG tags.
        if (_pendingScreenshot != null && _svgStream != null)
        {
            var tcs = _pendingScreenshot;
            _pendingScreenshot = null;
            var svg = System.Text.Encoding.UTF8.GetString(_svgStream.ToArray());
            _svgStream = null;
            tcs.SetResult(svg);
        }
    }

    // ── DevTools handler methods (called via DevToolsHandler in DevToolsPlatform) ──

    internal Task<InspectResult> HandleInspect()
    {
        var tcs = new TaskCompletionSource<InspectResult>();
        _platform.MainDispatcher.Post(() =>
        {
            try
            {
                var root = Abstract is Xui.Core.Abstract.Window w
                    ? WalkView(w.RootView)
                    : new ViewNode("window", 0, 0, 0, 0, true, []);
                tcs.SetResult(new InspectResult(root));
            }
            catch (Exception ex) { tcs.SetException(ex); }
        });
        return tcs.Task;
    }

    internal Task<ScreenshotResult> HandleScreenshot()
    {
        var tcs = new TaskCompletionSource<string>();
        _platform.MainDispatcher.Post(() =>
        {
            _pendingScreenshot = tcs;
            Platform!.Invalidate();
        });
        return tcs.Task.ContinueWith(
            t => new ScreenshotResult(t.Result),
            TaskContinuationOptions.ExecuteSynchronously);
    }

    internal Task HandleClick(ClickParams p)
    {
        _platform.MainDispatcher.Post(() =>
        {
            var pos = new Point(p.X, p.Y);
            _lastInputOverlay = (pos, isTouch: false);
            var down = new MouseDownEventRef { Position = pos, Button = MouseButton.Left };
            Abstract!.OnMouseDown(ref down);
            var up = new MouseUpEventRef { Position = pos, Button = MouseButton.Left };
            Abstract!.OnMouseUp(ref up);
            Platform!.Invalidate();
        });
        return Task.CompletedTask;
    }

    internal Task HandleTap(TapParams p)
    {
        _platform.MainDispatcher.Post(() =>
        {
            var pos = new Point(p.X, p.Y);
            _lastInputOverlay = (pos, isTouch: true);
            var touch = new Touch { Index = 0, Phase = TouchPhase.Start, Position = pos, Radius = 0.5f };
            var te = new TouchEventRef([touch]);
            Abstract!.OnTouch(ref te);

            touch.Phase = TouchPhase.End;
            te = new TouchEventRef([touch]);
            Abstract!.OnTouch(ref te);
            Platform!.Invalidate();
        });
        return Task.CompletedTask;
    }

    internal Task HandlePointer(PointerParams p)
    {
        _platform.MainDispatcher.Post(() =>
        {
            var phase = p.Phase switch
            {
                "start"  => TouchPhase.Start,
                "move"   => TouchPhase.Move,
                "end"    => TouchPhase.End,
                _        => TouchPhase.End,
            };
            var pos = new Point(p.X, p.Y);
            _lastInputOverlay = (pos, isTouch: true);
            var touch = new Touch { Index = p.Index, Phase = phase, Position = pos, Radius = 0.5f };
            var te = new TouchEventRef([touch]);
            Abstract!.OnTouch(ref te);
            Platform!.Invalidate();
        });
        return Task.CompletedTask;
    }

    internal Task HandleInvalidate()
    {
        _platform.MainDispatcher.Post(() => Platform!.Invalidate());
        return Task.CompletedTask;
    }

    internal Task HandleIdentify(IdentifyParams p)
    {
        _platform.MainDispatcher.Post(() =>
        {
            _overlayLabel = string.IsNullOrWhiteSpace(p.Label) ? null : p.Label;
            if (_lastInputOverlay.HasValue)
                Platform!.Invalidate();
        });
        return Task.CompletedTask;
    }

    // ── View tree walk ────────────────────────────────────────────────────────

    private static ViewNode WalkView(Xui.Core.UI.View view)
    {
        var f = view.Frame;
        var children = new ViewNode[view.Count];
        for (int i = 0; i < view.Count; i++)
            children[i] = WalkView(view[i]);
        return new ViewNode(
            view.GetType().Name,
            (float)f.X, (float)f.Y,
            (float)f.Width, (float)f.Height,
            true,
            children);
    }
}
