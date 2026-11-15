using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.Middleware;
using Xui.Core.UI;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;
using static Xui.Runtime.MacOS.AppKit;
using static Xui.Runtime.MacOS.AppKit.NSEventRef;
using static Xui.Runtime.MacOS.CoreAnimation;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.Foundation;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS.Actual;

public partial class MacOSWindow : NSWindow, Xui.Core.Actual.IWindow
{
    public static readonly Sel CloseSel = new Sel("close");

    public static Sel AnimationFrameSel = new Sel("animationFrame:");

    public bool RequireKeyboard { get; set; }

    protected static unsafe new readonly Class Class = NSWindow.Class
        .Extend("XUIMacOSWindow")
        .AddMethod("sendEvent:", SendEvent)
        .AddMethod("animationFrame:", AnimationFrame)
        .AddMethod("close", Close)
        .Register();

    public static void AnimationFrame(nint self, nint sel, nint caDisplayLink) =>
        Marshalling.Get<MacOSWindow>(self).AnimationFrame(caDisplayLink);

    protected static void Close(nint self, nint sel) =>
        Marshalling.Get<MacOSWindow>(self)
            .Close();

    private CADisplayLink displayLink;
    private TimeSpan previousFrameTime;
    private TimeSpan nextFrameTime;
    private bool pendingInvalidate;

    // Custom resize mechanism
    private WindowHitTestEventRef.WindowArea _activeResizeEdge = WindowHitTestEventRef.WindowArea.Default;
    private Point _resizeStartPoint;
    private NSRect _resizeStartFrame;

    // Keyboard modifier tracking (for FlagsChanged)
    private nuint _lastModifierFlags;

    // A borderless Xui frame can opt out of AppKit's native title-bar handling
    // for an otherwise transparent pointer region.
    private bool _suppressNativeEventDispatch;

    // The root view — always the flipped drawing view, regardless of content view hierarchy.
    private MacOSWindowRootView rootView = null!;
    private readonly Xui.Core.Abstract.IWindow.IDesktopStyle? desktopStyle;
    private readonly Style style;

    internal bool UsesGlassCornerConfiguration => this.style is GlassStyle;

    internal void OnWindowDidResize() => this.style.OnWindowDidResize(this);

    // Per-window drawing context (owns Path2D, text measure, paint state)
    private readonly MacOSDrawingContext drawingContext;
    private readonly IFontSource? fontSource;

    // Text measurement context (used for hit-testing cursor position on mouse click)
    private MacOSTextMeasureContext? _textMeasureContext;

    public ITextMeasureContext? TextMeasureContext =>
        _textMeasureContext ??= new MacOSTextMeasureContext(fontSource);

    public IServiceProvider NextServiceProvider { get; }

    // Image pipeline (decodes via ImageIO, caches CGImageRef by URI)
    private MacOSImageFactory? _imageFactory;

    private MacOSImageFactory ImageFactory =>
        _imageFactory ??= new MacOSImageFactory();

    // GPU device pipeline (Metal hardware-accelerated 3D rendering)
    private Xui.GPU.Hardware.Metal.MacOSGpuDevice? _gpuDevice;

    private Xui.GPU.Hardware.IGpuDevice? GpuDevice
    {
        get
        {
            if (_gpuDevice == null || _gpuDevice.IsDisposed)
            {
                _gpuDevice = null;
                try { _gpuDevice = new Xui.GPU.Hardware.Metal.MacOSGpuDevice(); }
                catch { /* Metal not available; return null */ }
            }
            return _gpuDevice;
        }
    }

    // Active popups owned by this window
    private readonly List<MacOSPopup> activePopups = new();

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(ITextMeasureContext)) return TextMeasureContext;
        if (serviceType == typeof(IImage)) return ImageFactory.CreateImage();
        if (serviceType == typeof(IDeviceInfo)) return MacOSDeviceInfo.Instance;
        if (serviceType == typeof(Xui.Core.UI.IPopup)) return CreatePopup();
        if (serviceType == typeof(Xui.GPU.Hardware.IGpuDevice)) return GpuDevice;
        if (serviceType == typeof(Xui.GPU.Backends.IShaderBackend)) return new Xui.GPU.Backends.Metal.MslCodeGenerator();
        return NextServiceProvider.GetService(serviceType);
    }

    private MacOSPopup CreatePopup()
    {
        var popup = new MacOSPopup(this);
        activePopups.Add(popup);
        popup.Closed += () => activePopups.Remove(popup);
        return popup;
    }

    /// <summary>
    /// Dismisses all active popups owned by this window.
    /// </summary>
    internal void DismissPopups()
    {
        // Copy to avoid modifying during iteration
        var popups = activePopups.ToArray();
        foreach (var p in popups)
            p.Close();
    }

    /// <summary>
    /// Checks if a mouse-down at the given screen point should dismiss any popups.
    /// Returns true if any popup was dismissed.
    /// </summary>
    private bool TryDismissPopupsOnMouseDown(NSPoint screenPoint)
    {
        if (activePopups.Count == 0) return false;

        bool dismissed = false;
        var popups = activePopups.ToArray();
        foreach (var p in popups)
        {
            if (p.TryDismissOnMouseDown(screenPoint))
                dismissed = true;
        }
        return dismissed;
    }

    public static nint InitWithAbstract(Xui.Core.Abstract.IWindow @abstract)
    {
        NSWindowStyleMask mask =
            NSWindowStyleMask.Titled |
            NSWindowStyleMask.Closable |
            NSWindowStyleMask.Miniaturizable |
            NSWindowStyleMask.Resizable;

        Rect rect = new Rect(200, 200, 600, 400);

        GetStyle(@abstract).ConfigureInitialWindow(@abstract, ref mask, ref rect);

        nint windowIntPtr = InitWithContentRectStyleMaskBackingDefer(
            Class.Alloc(),
            rect: rect,
            nswindowstylemask: mask,
            nsbackingstoretype: NSBackingStoreType.Buffered,
            defer: false
        );

        return windowIntPtr;
    }

    public MacOSWindow(Xui.Core.Abstract.IWindow @abstract, IServiceProvider nextServiceProvider) : base(InitWithAbstract(@abstract))
    {
        rootView = new MacOSWindowRootView(this);

        this.Abstract = @abstract;
        this.desktopStyle = @abstract.GetDesktopStyle();
        this.style = GetStyle(@abstract);
        this.NextServiceProvider = nextServiceProvider;
        this.fontSource = nextServiceProvider.GetService(typeof(IFontSource)) as IFontSource;
        this.drawingContext = new MacOSDrawingContext(fontSource);
        this.Title = "";
        this.Delegate = new MacOSWindowDelegate(this);

        this.IsReleasedWhenClosed = false;
        this.AcceptsMouseMovedEvents = true;

        // AppKit creates an initial content view whose frame reflects the window
        // style mask. Keep that exact frame when replacing it with our drawing
        // view: normal windows exclude the title bar; full-size windows include it.
        var initialContentView = this.ContentViewHandle;
        if (initialContentView == 0)
            throw new ObjCException("AppKit did not provide an initial NSWindow content view.");

        var initialContentFrame = NSView.GetFrame(initialContentView);
        NSView.SetFrame(rootView, initialContentFrame);
        rootView.AutoresizingMask =
            NSAutoresizingMaskOptions.WidthSizable |
            NSAutoresizingMaskOptions.HeightSizable;

        if (this.desktopStyle is { } desktopStyle)
        {
            this.Level = desktopStyle.Level switch
            {
                DesktopWindowLevel.Normal => NSWindowLevel.Normal,
                DesktopWindowLevel.Floating => NSWindowLevel.Floating,
                DesktopWindowLevel.StatusBar => NSWindowLevel.StatusBar,
                DesktopWindowLevel.Modal => NSWindowLevel.ModalPanel,
                _ => NSWindowLevel.Normal,
            };
        }

        this.style.ConfigureContentView(this, initialContentFrame);

        this.displayLink = CADisplayLink.DisplayLink(this, AnimationFrameSel);
        this.displayLink.AddToRunLoopForMode(NSRunLoop.MainRunLoop, NSRunLoop.Mode.Common);
    }

    public Xui.Core.Abstract.IWindow Abstract { get; }

    string Xui.Core.Actual.IWindow.Title
    {
        get => this.Title!;
        set => this.Title = value;
    }

    void Xui.Core.Actual.IWindow.Show()
    {
        this.MakeKeyAndOrderFront();
    }

    protected void Close()
    {
        this.DismissPopups();
        this.Abstract.Closed();
        Super super = new Super(this, NSWindow.Class);
        objc_msgSendSuper(ref super, CloseSel);
    }

    internal bool Closing() => this.Abstract.Closing();

    private void AnimationFrame(nint caDisplayLink)
    {
        this.previousFrameTime = this.displayLink.Timestamp;
        this.nextFrameTime = this.displayLink.TargetTimestamp;
        var animationFrame = new FrameEventRef(this.previousFrameTime, this.nextFrameTime);
        this.Abstract.OnAnimationFrame(ref animationFrame);

        if (this.pendingInvalidate)
        {
            this.pendingInvalidate = false;
            rootView.NeedsDisplay = true;
        }
    }

    public void Invalidate()
    {
        this.pendingInvalidate = true;
        rootView.NeedsDisplay = true;
    }

    /// <summary>
    /// Gets the Xui layout area in the flipped root-view coordinate system.
    /// Normal AppKit windows reserve their title bar; custom and extended chrome
    /// intentionally exposes the complete root view to Xui.
    /// </summary>
    internal Rect LayoutArea
    {
        get
        {
            var rootFrame = rootView.Frame;
            var rootArea = new Rect(0, 0, rootFrame.Size.width, rootFrame.Size.height);

            return this.style.GetLayoutArea(this, rootArea);
        }
    }

    internal void Render(NSRect rect)
    {
        var area = this.LayoutArea;
        this.Abstract.DisplayArea = area;
        this.Abstract.SafeArea = area;

        FrameEventRef frame = new(this.previousFrameTime, this.nextFrameTime);
        var ctx = this.drawingContext.Bind();
        RenderEventRef render = new(area, frame, ctx);

        this.pendingInvalidate = false;
        this.Abstract.Render(ref render);
    }
}
