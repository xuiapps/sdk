using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.GPU.Hardware;
using Xui.GPU.Backends;
using Xui.GPU.Backends.Glsl;

namespace Xui.Runtime.Browser.Actual;

public partial class BrowserWindow : Xui.Core.Actual.IWindow
{
    [JSImport("setTitle", "xui-browser")]
    internal static partial void SetTitle(string title);
    [JSImport("setKeyboard", "xui-browser")]
    internal static partial void SetKeyboard(bool required);

    public static BrowserWindow? Instance { get; internal set; }
    public Core.Abstract.IWindow Abstract { get; }
    public IServiceProvider NextServiceProvider { get; }
    private readonly BrowserDrawingContext drawingContext = new();
    private readonly BrowserGpuDevice? gpu;
    private readonly IShaderBackend shaderBackend = new GlslCodeGenerator();
    private readonly List<BrowserImage> images = new();
    private bool invalidated = true;
    private bool shown;
    private bool requireKeyboard;
    private string title = "";
    private double lastTimestamp = -1;
    private double lastPixelRatio;
    public ITextMeasureContext TextMeasureContext => drawingContext;

    public BrowserWindow(Core.Abstract.IWindow windowAbstract, IServiceProvider nextServiceProvider)
    {
        Abstract = windowAbstract;
        NextServiceProvider = nextServiceProvider;
        gpu = BrowserGpuDevice.TryCreate();
        drawingContext.GpuDevice = gpu;
    }

    public string Title { get => title; set { title = value; SetTitle(value); } }
    public bool RequireKeyboard
    {
        get => requireKeyboard;
        set { requireKeyboard = value; SetKeyboard(value); }
    }
    public void Invalidate() => invalidated = true;
    public void Show() { shown = true; Invalidate(); }
    public void Close()
    {
        shown = false;
        if (Instance == this) Instance = null;
        foreach (var image in images) image.Dispose();
        images.Clear();
        drawingContext.Dispose();
        gpu?.Dispose();
    }
    public object? GetService(Type serviceType) =>
        serviceType == typeof(IContext) || serviceType == typeof(ITextMeasureContext) ? drawingContext :
        serviceType == typeof(IGpuDevice) || serviceType == typeof(IGpuResourceDevice) ? gpu :
        serviceType == typeof(IShaderBackend) ? shaderBackend :
        serviceType == typeof(IImage) ? CreateImage() : NextServiceProvider.GetService(serviceType);
    private BrowserImage CreateImage()
    {
        var image = new BrowserImage(Invalidate);
        images.Add(image);
        return image;
    }

    [JSExport]
    internal static void InvalidateFrame() => Instance?.Invalidate();

    // Returns whether a frame was painted, so the host can distinguish rAF cadence from render FPS.
    [JSExport]
    internal static bool OnAnimationFrame(double width, double height, double timestamp, double pixelRatio)
    {
        var window = Instance;
        if (window is null || !window.shown) return false;
        var area = new Rect(0, 0, (NFloat)width, (NFloat)height);
        if (window.Abstract.DisplayArea != area || window.lastPixelRatio != pixelRatio)
        {
            window.Abstract.DisplayArea = area;
            window.Abstract.SafeArea = area;
            window.Abstract.ScreenCornerRadius = 0;
            window.lastPixelRatio = pixelRatio;
            window.Invalidate();
        }
        var previous = window.lastTimestamp < 0 ? timestamp : window.lastTimestamp;
        window.lastTimestamp = timestamp;
        var frame = new FrameEventRef(TimeSpan.FromMilliseconds(previous), TimeSpan.FromMilliseconds(timestamp));
        window.Abstract.OnAnimationFrame(ref frame);
        if (!window.invalidated) return false;
        window.invalidated = false;
        window.drawingContext.Reset(pixelRatio);
        var render = new RenderEventRef(area, frame, window.drawingContext);
        window.Abstract.Render(ref render);
        window.drawingContext.Flush();
        window.drawingContext.EndFrame();
        return true;
    }

    [JSExport]
    internal static void OnPointer(int phase, int pointerId, bool touch, int button, double x, double y)
    {
        var window = Instance;
        if (window is null) return;
        var position = new Point((NFloat)x, (NFloat)y);
        if (touch)
        {
            Span<Touch> touches = stackalloc Touch[1];
            touches[0] = new Touch { Index = pointerId, Position = position, Phase = (TouchPhase)phase };
            var e = new TouchEventRef(touches);
            window.Abstract.OnTouch(ref e);
        }
        else if (phase == 0)
        {
            var e = new MouseDownEventRef { Position = position, Button = MapButton(button), TextMeasure = window.drawingContext };
            window.Abstract.OnMouseDown(ref e);
        }
        else if (phase == 1)
        {
            var e = new MouseMoveEventRef { Position = position, TextMeasure = window.drawingContext };
            window.Abstract.OnMouseMove(ref e);
        }
        else
        {
            var e = new MouseUpEventRef { Position = position, Button = MapButton(button), TextMeasure = window.drawingContext };
            window.Abstract.OnMouseUp(ref e);
        }
    }
    private static MouseButton MapButton(int button) => button == 0 ? MouseButton.Left : button == 2 ? MouseButton.Right : MouseButton.Other;

    [JSExport]
    internal static void OnWheel(double x, double y, double deltaX, double deltaY)
    {
        var e = new ScrollWheelEventRef { Position = new Point((NFloat)x, (NFloat)y), HasPosition = true,
            Delta = new Vector((NFloat)deltaX, (NFloat)deltaY) };
        Instance?.Abstract.OnScrollWheel(ref e);
    }

    [JSExport]
    internal static bool OnKey(int key, bool shift, bool repeat)
    {
        var e = new KeyEventRef { Key = (VirtualKey)key, Shift = shift, IsRepeat = repeat };
        Instance?.Abstract.OnKeyDown(ref e);
        return e.Handled;
    }

    [JSExport]
    internal static void OnText(string text)
    {
        foreach (var character in text)
        {
            var e = new KeyEventRef { Character = character };
            Instance?.Abstract.OnChar(ref e);
        }
    }
}
