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
internal sealed class DevToolsWindow : Xui.Core.Middleware.IWindow
{
    private readonly DevToolsPlatform platform;
    private readonly FrameAllocationProfiler profiler = new();
    private TaskCompletionSource<FrameResult>? pendingFrame;
    private TimeSpan controlledTime;
    private TimeSpan controlledPrevious;
    private long frameAnimationBytes;

    // Screenshot state — written and read exclusively on the UI thread.
    private TaskCompletionSource<string>? pendingScreenshot;
    private MemoryStream? svgStream;
    private SvgDrawingContext? pendingSvgContext;
    private Rect pendingRect;

    // Overlay context captured from GetService so Render() can dispose it after Abstract.Render.
    private IContext? pendingOverlayCtx;

    // Overlay state: last interaction point and identity label, shown on every frame until replaced.
    private (Point pos, bool isTouch)? lastInputOverlay;
    private string? overlayLabel;

    /// <summary>The abstract application window (app layer).</summary>
    public Xui.Core.Abstract.IWindow Abstract { get; }

    /// <summary>The actual window immediately downstream of this middleware layer.</summary>
    public Xui.Core.Actual.IWindow? Actual { get; private set; }

    /// <summary>Application services reached after the complete actual-window chain.</summary>
    public IServiceProvider NextServiceProvider { get; }

    public DevToolsWindow(
        DevToolsPlatform platform,
        Xui.Core.Abstract.IWindow @abstract,
        IServiceProvider nextServiceProvider)
    {
        this.platform = platform;
        this.Abstract = @abstract;
        this.NextServiceProvider = nextServiceProvider;
    }

    internal void AttachActual(Xui.Core.Actual.IWindow actual)
    {
        if (this.Actual is not null)
            throw new InvalidOperationException("The DevTools window is already attached to an actual window.");

        this.Actual = actual;
    }

    string Xui.Core.Actual.IWindow.Title
    {
        get => Actual!.Title;
        set => Actual!.Title = value;
    }

    void Xui.Core.Actual.IWindow.Show() => Actual!.Show();
    void Xui.Core.Actual.IWindow.Invalidate() => Actual!.Invalidate();
    void Xui.Core.Actual.IWindow.Close() => Actual!.Close();

    bool Xui.Core.Actual.IWindow.RequireKeyboard
    {
        get => Actual!.RequireKeyboard;
        set => Actual!.RequireKeyboard = value;
    }

    Xui.Core.Canvas.ITextMeasureContext? Xui.Core.Actual.IWindow.TextMeasureContext
        => Actual!.TextMeasureContext;

    /// <summary>
    /// Returns a <see cref="SplicingContext"/> when a screenshot is pending so the next
    /// render pass writes simultaneously to the real display and an SVG capture stream.
    /// </summary>
    private IContext WrapContext(IContext realCtx)
    {
        IContext ctx;
        if (pendingScreenshot != null && svgStream != null)
        {
            var svgCtx = new SvgDrawingContext(
                new Size(pendingRect.Width, pendingRect.Height),
                svgStream,
                keepOpen: true);
            pendingSvgContext = svgCtx;
            ctx = new SplicingContext(realCtx, svgCtx);
        }
        else
        {
            ctx = realCtx;
        }

        // Wrap with OverlayContext when there's an interaction point or a client label to show.
        // Store it so Render() can call Dispose() after Abstract.Render to trigger the drawing.
        if (!platform.StdioEnabled && lastInputOverlay is { } overlay)
        {
            pendingOverlayCtx = new OverlayContext(ctx, overlay.pos, overlay.isTouch, overlayLabel);
            return pendingOverlayCtx;
        }

        if (!platform.StdioEnabled && overlayLabel != null)
        {
            pendingOverlayCtx = new OverlayContext(ctx, null, false, overlayLabel);
            return pendingOverlayCtx;
        }

        return ctx;
    }

    object? IServiceProvider.GetService(Type t) =>
        Actual?.GetService(t) ?? NextServiceProvider.GetService(t);

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

    void Xui.Core.Abstract.IWindow.Closed()
    {
        pendingFrame?.TrySetException(new IOException("Window closed before rendering."));
        pendingFrame = null;
        pendingScreenshot?.TrySetException(new IOException("Window closed before screenshot."));
        pendingScreenshot = null;
        Abstract.Closed();
    }
    bool Xui.Core.Abstract.IWindow.Closing() => Abstract!.Closing();

    void Xui.Core.Abstract.IWindow.OnAnimationFrame(ref FrameEventRef e)
    {
        if (!platform.StdioEnabled) Abstract.OnAnimationFrame(ref e);
    }
    void Xui.Core.Abstract.IWindow.OnMouseDown(ref MouseDownEventRef e) => Abstract!.OnMouseDown(ref e);
    void Xui.Core.Abstract.IWindow.OnMouseMove(ref MouseMoveEventRef e) => Abstract!.OnMouseMove(ref e);
    void Xui.Core.Abstract.IWindow.OnMouseUp(ref MouseUpEventRef e) => Abstract!.OnMouseUp(ref e);
    void Xui.Core.Abstract.IWindow.OnScrollWheel(ref ScrollWheelEventRef e) => Abstract!.OnScrollWheel(ref e);
    void Xui.Core.Abstract.IWindow.OnTouch(ref TouchEventRef e) => Abstract!.OnTouch(ref e);
    void Xui.Core.Abstract.IWindow.WindowHitTest(ref WindowHitTestEventRef e) => Abstract!.WindowHitTest(ref e);
    void Xui.Core.Abstract.IWindow.OnKeyDown(ref KeyEventRef e) => Abstract!.OnKeyDown(ref e);
    void Xui.Core.Abstract.IWindow.OnChar(ref KeyEventRef e) => Abstract!.OnChar(ref e);

    /// <summary>
    /// Intercepts the render call: if a screenshot is pending, allocates the capture stream
    /// and rect before delegating to the abstract window, then reads the completed SVG after.
    /// </summary>
    void Xui.Core.Abstract.IWindow.Render(ref RenderEventRef render)
    {
        if (pendingScreenshot != null)
        {
            svgStream = new MemoryStream();
            pendingRect = render.Rect;
        }

        pendingOverlayCtx = null;
        render.Context = WrapContext(render.Context);
        var frame = pendingFrame;
        if (platform.StdioEnabled)
            render.Frame = new FrameEventRef(controlledPrevious, controlledTime);
        if (frame is not null) profiler.Begin(3);
        var before = frame is null ? 0 : GC.GetAllocatedBytesForCurrentThread();
        long renderBytes;
        try { Abstract.Render(ref render); }
        catch (Exception ex) when (frame is not null)
        {
            pendingFrame = null;
            frame.TrySetException(ex);
            return;
        }
        finally
        {
            renderBytes = frame is null ? 0 : GC.GetAllocatedBytesForCurrentThread() - before;
            if (frame is not null) profiler.End();
        }
        if (frame is not null)
        {
            pendingFrame = null;
            profiler.Frames++;
            profiler.AnimationBytes += frameAnimationBytes;
            profiler.RenderBytes += renderBytes;
            frame.TrySetResult(new FrameResult(profiler.Frames, frameAnimationBytes, renderBytes));
        }

        // Draw the overlay on top of the app content (and into the SVG stream if screenshotting).
        // Dispose() triggers the drawing; inner.Dispose() is NOT called by OverlayContext so the
        // underlying context (real or SplicingContext) remains valid for the screenshot read below.
        pendingOverlayCtx?.Dispose();
        pendingOverlayCtx = null;

        // Window.Render does not dispose the IContext, so we must dispose the
        // SvgDrawingContext ourselves to flush the closing SVG tags to the stream.
        if (pendingSvgContext != null)
        {
            pendingSvgContext.Dispose();
            pendingSvgContext = null;
        }

        if (pendingScreenshot != null && svgStream != null)
        {
            var tcs = pendingScreenshot;
            pendingScreenshot = null;
            var svg = System.Text.Encoding.UTF8.GetString(svgStream.ToArray());
            svgStream.Dispose();
            svgStream = null;
            tcs.SetResult(svg);
        }
    }

    internal Task<InspectResult> HandleInspect()
    {
        var tcs = new TaskCompletionSource<InspectResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        platform.MainDispatcher.Post(() =>
        {
            try
            {
                var root = Abstract is Xui.Core.Abstract.Window w
                    ? WalkView(w.RootView)
                    : new ViewNode("window", 0, 0, 0, 0, 0, 0, true, null, null, []);
                tcs.SetResult(new InspectResult(root));
            }
            catch (Exception ex) { tcs.SetException(ex); }
        });
        return tcs.Task;
    }

    internal Task<ScreenshotResult> HandleScreenshot()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        platform.MainDispatcher.Post(() =>
        {
            if (pendingScreenshot is not null || pendingFrame is not null)
            {
                tcs.SetException(new InvalidOperationException("A render request is already pending."));
                return;
            }
            pendingScreenshot = tcs;
            Actual!.Invalidate();
        });
        return tcs.Task.ContinueWith(
            t => new ScreenshotResult(t.Result),
            TaskContinuationOptions.ExecuteSynchronously);
    }

    internal Task HandleClick(ClickParams p)
    {
        return DispatchAsync(() =>
        {
            var pos = new Point(p.X, p.Y);
            lastInputOverlay = (pos, isTouch: false);
            var down = new MouseDownEventRef { Position = pos, Button = MouseButton.Left };
            profiler.Begin(1);
            var before = GC.GetAllocatedBytesForCurrentThread();
            try
            {
                Abstract!.OnMouseDown(ref down);
                var up = new MouseUpEventRef { Position = pos, Button = MouseButton.Left };
                Abstract!.OnMouseUp(ref up);
            }
            finally
            {
                profiler.InputBytes += GC.GetAllocatedBytesForCurrentThread() - before;
                profiler.End();
            }
            Actual!.Invalidate();
        });
    }

    internal Task HandleTap(TapParams p)
    {
        return DispatchAsync(() =>
        {
            var pos = new Point(p.X, p.Y);
            lastInputOverlay = (pos, isTouch: true);
            var touch = new Touch { Index = 0, Phase = TouchPhase.Start, Position = pos, Radius = 0.5f };
            var te = new TouchEventRef([touch]) { TextMeasure = Actual!.TextMeasureContext };
            profiler.Begin(1);
            var before = GC.GetAllocatedBytesForCurrentThread();
            try
            {
                Abstract!.OnTouch(ref te);

                touch.Phase = TouchPhase.End;
                var endEvent = new TouchEventRef([touch]) { TextMeasure = te.TextMeasure };
                Abstract!.OnTouch(ref endEvent);
            }
            finally
            {
                profiler.InputBytes += GC.GetAllocatedBytesForCurrentThread() - before;
                profiler.End();
            }
            Actual!.Invalidate();
        });
    }

    internal Task HandlePointer(PointerParams p)
    {
        return DispatchAsync(() =>
        {
            if (p.Index < 0) throw new ArgumentOutOfRangeException(nameof(p.Index));
            var phase = p.Phase switch
            {
                "start" => TouchPhase.Start,
                "move" => TouchPhase.Move,
                "end" => TouchPhase.End,
                "cancel" => TouchPhase.Cancel,
                _ => throw new ArgumentException("Unknown pointer phase."),
            };
            var pos = new Point(p.X, p.Y);
            lastInputOverlay = (pos, isTouch: true);
            var touch = new Touch { Index = p.Index, Phase = phase, Position = pos, Radius = 0.5f };
            var te = new TouchEventRef([touch]) { TextMeasure = Actual!.TextMeasureContext };
            profiler.Begin(1);
            var before = GC.GetAllocatedBytesForCurrentThread();
            try
            {
                Abstract!.OnTouch(ref te);
            }
            finally
            {
                profiler.InputBytes += GC.GetAllocatedBytesForCurrentThread() - before;
                profiler.End();
            }
            Actual!.Invalidate();
        });
    }

    internal Task HandleMouseMove(MouseMoveParams p)
    {
        return DispatchAsync(() =>
        {
            var pos = new Point(p.X, p.Y);
            lastInputOverlay = (pos, isTouch: false);
            var move = new MouseMoveEventRef { Position = pos };
            profiler.Begin(1);
            var before = GC.GetAllocatedBytesForCurrentThread();
            try
            {
                Abstract!.OnMouseMove(ref move);
            }
            finally
            {
                profiler.InputBytes += GC.GetAllocatedBytesForCurrentThread() - before;
                profiler.End();
            }
            Actual!.Invalidate();
        });
    }

    internal Task HandleInvalidate()
    {
        return DispatchAsync(() => Actual!.Invalidate());
    }

    internal Task HandleIdentify(IdentifyParams p)
    {
        return DispatchAsync(() =>
        {
            overlayLabel = string.IsNullOrWhiteSpace(p.Label) ? null : p.Label;
            Actual!.Invalidate();
        });
    }

    private Task DispatchAsync(Action action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        platform.MainDispatcher.Post(() =>
        {
            try { action(); completion.SetResult(); }
            catch (Exception ex) { completion.SetException(ex); }
        });
        return completion.Task;
    }

    internal async Task<ProfileResult> HandleProfileReset()
    {
        await DispatchAsync(() =>
        {
            if (pendingFrame is not null) throw new InvalidOperationException("A frame is in progress.");
            profiler.Reset();
        });
        return new(0, 0, 0, 0);
    }

    internal async Task<ProfileResult> HandleProfileRead()
    {
        ProfileResult? result = null;
        await DispatchAsync(() => result = profiler.Read());
        return result!;
    }

    internal Task<FrameResult> HandleFrame(FrameParams p)
    {
        if (!platform.StdioEnabled)
            throw new InvalidOperationException("Controlled frames require --enable-stdio-jsonrpc.");
        if (p.ElapsedMilliseconds is < 0 or > 60000)
            throw new ArgumentOutOfRangeException(nameof(p), "Elapsed milliseconds must be between 0 and 60000.");
        var completion = new TaskCompletionSource<FrameResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        platform.MainDispatcher.Post(() =>
        {
            try
            {
                if (pendingFrame is not null || pendingScreenshot is not null)
                    throw new InvalidOperationException("A render request is already pending.");
                var next = controlledTime + TimeSpan.FromMilliseconds(p.ElapsedMilliseconds);
                var frame = new FrameEventRef(controlledTime, next);
                controlledPrevious = controlledTime;
                controlledTime = next;
                profiler.Begin(2);
                var before = GC.GetAllocatedBytesForCurrentThread();
                try { Abstract.OnAnimationFrame(ref frame); }
                finally
                {
                    frameAnimationBytes = GC.GetAllocatedBytesForCurrentThread() - before;
                    profiler.End();
                }
                pendingFrame = completion;
                Actual!.Invalidate();
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(pendingFrame, completion)) pendingFrame = null;
                completion.TrySetException(ex);
            }
        });
        return completion.Task;
    }

    private static ViewNode WalkView(Xui.Core.UI.View view)
    {
        var f = view.Frame;
        var children = new ViewNode[view.Count];
        for (int i = 0; i < view.Count; i++)
            children[i] = WalkView(view[i]);

        // Calculate center point for clicking
        var center = view.Parent?.LocalToGlobal(f.Center) ?? f.Center;
        var centerX = (float)center.X;
        var centerY = (float)center.Y;

        // Get ClassName as comma-separated string or null
        var className = view.ClassName.Count > 0
            ? string.Join(" ", view.ClassName)
            : null;

        // Strip generic arity suffix (e.g. "ViewCollection`1" -> "ViewCollection") so the
        // type name can be used as a valid XML element name by the MCP InspectUi tool.
        var typeName = view.GetType().Name;
        var backtickIdx = typeName.IndexOf('`');
        if (backtickIdx >= 0)
            typeName = typeName[..backtickIdx];

        return new ViewNode(
            typeName,
            (float)f.X, (float)f.Y,
            (float)f.Width, (float)f.Height,
            centerX, centerY,
            true,
            view.Id,
            className,
            children);
    }
}
