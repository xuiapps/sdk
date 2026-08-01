using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.CoreAnimation;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;
using System.Collections.Generic;
using static Xui.Runtime.IOS.Foundation;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

public class IOSWindow : UIWindow, Xui.Core.Actual.IWindow, Xui.Core.Actual.ITextMagnifier, Xui.Core.Actual.ITextInputSession
{
    public static Sel AnimationFrameSel = new Sel("animationFrame:");

    public static new readonly Class Class = UIWindow.Class
        .Extend("XUIIOSWindow")
        .AddMethod("sendEvent:", SendEvent)
        .AddMethod("animationFrame:", AnimationFrame)
        .Register();

    public static void SendEvent(nint self, nint sel, nint uiEvent) =>
        Marshalling.Get<IOSWindow>(self).SendEvent(new UIEventRef(uiEvent));
    
    public static void AnimationFrame(nint self, nint sel, nint caDisplayLink) =>
        Marshalling.Get<IOSWindow>(self).AnimationFrame(caDisplayLink);
    
    // Keep native touch identities stable without growing a dictionary during an
    // interaction. Ten slots cover normal multi-touch hardware with room to spare.
    private readonly TouchIdentityMap touchIndexMap = new();
    private bool[] nativeTouches = new bool[10];
    private static readonly Sel SendEventSel = new("sendEvent:");
    private static readonly Sel TouchViewSel = new("view");

    private CADisplayLink displayLink;
    // The context owns cached CoreText fonts and paint state, while Bind captures
    // the UIKit CGContext that is valid only for the current drawRect callback.
    private readonly IOSDrawingContext drawingContext;
    private readonly IFontSource? fontSource;
    private IOSTextMeasureContext? textMeasureContext;
    private readonly IOSDeviceInfo deviceInfo;
    private IOSImageFactory? imageFactory;
    private Xui.GPU.Hardware.Metal.IOSGpuDevice? gpuDevice;
    private TimeSpan previousFrameTime;
    private TimeSpan nextFrameTime;
    private bool pendingInvalidate;
    private UITextLoupeSessionPtr textLoupe;

    bool Xui.Core.Actual.ITextMagnifier.IsSupported => UITextLoupeSessionPtr.IsSupported;

    void Xui.Core.Actual.ITextMagnifier.Show(Point position, Rect caret)
    {
        // The contract uses window coordinates; UIWindow is also a UIView and
        // supplies the correct coordinate space for all of its rendered subviews.
        if (!textLoupe.IsActive) textLoupe = UITextLoupeSessionPtr.Begin(position, this);
        textLoupe.Move(position, caret);
    }

    void Xui.Core.Actual.ITextMagnifier.Hide() => textLoupe.Dispose();

    public IServiceProvider NextServiceProvider { get; }

    public ITextMeasureContext TextMeasureContext =>
        this.textMeasureContext ??= new IOSTextMeasureContext(fontSource);

    public IOSWindow(Xui.Core.Abstract.IWindow @abstract, IServiceProvider nextServiceProvider) : base(Class.New())
    {
        this.Abstract = @abstract;
        this.NextServiceProvider = nextServiceProvider;
        this.fontSource = nextServiceProvider.GetService(typeof(IFontSource)) as IFontSource;
        this.drawingContext = new IOSDrawingContext(fontSource);
        this.Title = "";
        this.RootView = new IOSWindowRootView(this);
        this.deviceInfo = new IOSDeviceInfo(this.RootView);

        this.SoftKeyboard = new IOSSoftKeyboard(this);
        this.SoftKeyboard.NextResponder = this.RootView;

        this.DefaultResponder = new IOSDefaultResponder(this);
        this.DefaultResponder.NextResponder = this.RootView;

        this.RootViewController = new IOSRootViewController(this)
        {
            View = this.RootView
        };

        this.displayLink = CADisplayLink.DisplayLink(this, AnimationFrameSel);
        // The host's CADisableMinimumFrameDurationOnPhone plist key enables ProMotion.
        // Ask for this screen's maximum (typically 60 or 120 Hz); iOS still controls
        // the delivered rate according to system policy. CAFrameRateRange uses native floats.
        var maximumFramesPerSecond = (float)new UIScreenRef(this.Screen).MaximumFramesPerSecond;
        this.displayLink.PreferredFrameRateRange = new CADisplayLink.FrameRateRange(
            maximumFramesPerSecond, maximumFramesPerSecond, maximumFramesPerSecond);
        this.displayLink.AddToRunLoopForMode(NSRunLoop.MainRunLoop, NSRunLoop.Mode.Common);
    }

    private bool requireKeyboard;

    public bool RequireKeyboard
    {
        get => this.requireKeyboard;

        set
        {
            if (this.requireKeyboard != value)
            {
                if (value)
                {
                    if (this.SoftKeyboard.BecomeFirstResponder()) this.SoftKeyboard.ActivateInteraction();
                }
                else
                {
                    textLoupe.Dispose();
                    this.DefaultResponder.BecomeFirstResponder();
                }

                this.requireKeyboard = value;
            }
            else if (value)
            {
                // The responder is shared; a different focused field may be secure.
                this.SoftKeyboard.ReloadInputViews();
            }
        }
    }

    void Xui.Core.Actual.ITextInputSession.SetClient(Xui.Core.UI.ITextEditingClient? client) => SoftKeyboard.SetClient(client);
    void Xui.Core.Actual.ITextInputSession.CancelSelectionGesture() => SoftKeyboard.CancelTouchSelection();

    private void SendEvent(UIEventRef uiEventRef)
    {
        var type = uiEventRef.Type;
        var subtype = uiEventRef.Subtype;
        
        if (type == UIEvent.EventType.Touches)
        {
            var touches = uiEventRef.AllTouches;
            var count = (int)touches.Count;

            ReadOnlySpan<nint> touchPtrs = stackalloc nint[count];
            Span<Touch> touchPoints = stackalloc Touch[count];
            var changedCount = 0;

            touches.GetValues(ref MemoryMarshal.GetReference(touchPtrs));
            var previousClient = SoftKeyboard.Client;
            bool started = false;
            for(var i = 0; i < count; i++)
            {
                // TRICKY:
                // We don't use 'using var touchRef = new ....' because...
                // 
                // The NSSet states the get values will follow creation rule and we should dispose the objects we got:
                // https://developer.apple.com/documentation/corefoundation/1520437-cfsetgetvalues?language=objc
                // 
                // But the touch count seems stable and with Release,
                // the app will crash as we will dispose the touches.
                var touchRef = new UITouchRef(touchPtrs[i]);
                if (touchRef.Phase == UITouch.Phase.Stationary) continue;
                var phase = MapPhase(touchRef.Phase);

                int index = 0;
                if (phase == TouchPhase.Start)
                {
                    bool firstContact = !touchIndexMap.HasContacts;
                    if (!touchIndexMap.TryBegin(touchRef.Self, out index)) continue;
                    if (firstContact) { SoftKeyboard.BeginTouches(); started = true; }
                    if (index >= nativeTouches.Length) Array.Resize(ref nativeTouches, index + 10);
                    nint target = objc_msgSend_retIntPtr(touchRef.Self, TouchViewSel);
                    SoftKeyboard.BeginHandleTouch(touchRef.Self, target, touchRef.LocationInView(null));
                    nativeTouches[index] = (target != 0 && target != (nint)RootView && target != (nint)this)
                        || SoftKeyboard.OwnsHandle(touchRef.LocationInView(null));
                }
                else
                {
                    if (!touchIndexMap.TryGet(touchRef.Self, out index)) continue;
                }

                if (nativeTouches[index]) continue;
                touchPoints[changedCount++] = new Touch()
                {
                    Index = index,
                    Position = touchRef.LocationInView(null),
                    Radius = touchRef.Radius,
                    Phase = MapPhase(touchRef.Phase),
                };

            }

            var touchEventRef = new TouchEventRef(touchPoints[..changedCount]);
            if (changedCount > 0) this.Abstract.OnTouch(ref touchEventRef);
            if (started && !ReferenceEquals(previousClient, SoftKeyboard.Client)) SoftKeyboard.FocusedByTouch();
            // UIKit must also receive events for UITextInteraction (handles, loupe and selection).
            var super = new Super(this, UIWindow.Class);
            objc_msgSendSuper(ref super, SendEventSel, uiEventRef);
            bool ended = false;
            // Keep ended IDs reserved until the whole batch has been routed. NSSet
            // order must not let a new contact reuse an ID that is still ending.
            for (var i = 0; i < count; i++)
            {
                var touch = new UITouchRef(touchPtrs[i]);
                if (touch.Phase is UITouch.Phase.Ended or UITouch.Phase.Cancelled)
                {
                    SoftKeyboard.EndHandleTouch(touch.Self);
                    if (touchIndexMap.TryGet(touch.Self, out int index)) nativeTouches[index] = false;
                    touchIndexMap.End(touch.Self);
                    ended = true;
                }
            }
            if (ended && !touchIndexMap.HasContacts) SoftKeyboard.EndTouches();
        }
        else
        {
            var super = new Super(this, UIWindow.Class);
            objc_msgSendSuper(ref super, SendEventSel, uiEventRef);
        }
    }

    private void AnimationFrame(nint caDisplayLink)
    {
        this.previousFrameTime = this.displayLink.Timestamp;
        this.nextFrameTime = this.displayLink.TargetTimestamp;
        var animationFrame = new FrameEventRef(this.previousFrameTime, this.nextFrameTime);
        this.Abstract.OnAnimationFrame(ref animationFrame);

        // Animation callbacks commonly update view state and invalidate while UIKit
        // is processing the display-link turn. Schedule the UIKit redraw afterwards,
        // matching the macOS render lifecycle and avoiding a lost invalidation.
        if (this.pendingInvalidate)
        {
            this.pendingInvalidate = false;
            this.RootView.SetNeedsDisplay();
        }
    }

    private TouchPhase MapPhase(UITouch.Phase phase)
    {
        switch(phase)
        {
            case UITouch.Phase.Began: return TouchPhase.Start;
            case UITouch.Phase.Ended: return TouchPhase.End;
            case UITouch.Phase.Cancelled: return TouchPhase.Cancel;
            default: return TouchPhase.Move;
        }
    }

    public string Title { get; set; }
    
    public Xui.Core.Abstract.IWindow Abstract { get; }

    internal IOSWindowRootView RootView { get; }

    internal nint HitTestSelectionHandle(CGPoint point) => SoftKeyboard?.HandleAtInputPoint(point) ?? 0;

    internal IOSDeviceInfo DeviceInfo => this.deviceInfo;

    private IOSImageFactory ImageFactory => this.imageFactory ??= new IOSImageFactory();

    private Xui.GPU.Hardware.IGpuDevice? GpuDevice
    {
        get
        {
            if (this.gpuDevice is null || this.gpuDevice.IsDisposed)
            {
                this.gpuDevice = null;
                try
                {
                    this.gpuDevice = new Xui.GPU.Hardware.Metal.IOSGpuDevice();
                }
                catch
                {
                    // Metal is not available on every simulator/device configuration.
                }
            }

            return this.gpuDevice;
        }
    }

    protected IOSSoftKeyboard SoftKeyboard { get; }

    protected IOSDefaultResponder DefaultResponder { get; }

    internal Rect DisplayArea
    {
        get => this.Abstract.DisplayArea;
        set => this.Abstract.DisplayArea = value;
    }

    internal Rect SafeArea
    {
        get => this.Abstract.SafeArea;
        set => this.Abstract.SafeArea = value;
    }

    internal NFloat ScreenCornerRadius
    {
        set => this.Abstract.ScreenCornerRadius = value;
    }

    public void Invalidate()
    {
        this.pendingInvalidate = true;
        this.RootView.SetNeedsDisplay();
    }

    void Xui.Core.Actual.IWindow.Show() => this.MakeKeyAndVisible();

    void Xui.Core.Actual.IWindow.Close()
    {
        RequireKeyboard = false;
        SoftKeyboard.Close();
        textLoupe.Dispose();
        ObjC.objc_msgSend(displayLink, new Sel("invalidate"));
        ObjC.objc_msgSend(this, new Sel("setHidden:"), true);
    }

    object? IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(Xui.Core.Actual.ITextInputSession))
            return this;
        if (serviceType == typeof(Xui.Core.Actual.ITextMagnifier))
            return this;
        if (serviceType == typeof(ITextMeasureContext))
            return TextMeasureContext;
        if (serviceType == typeof(IDeviceInfo))
            return deviceInfo;
        if (serviceType == typeof(IImage))
            return ImageFactory.CreateImage();
        if (serviceType == typeof(Xui.GPU.Hardware.IGpuDevice))
            return GpuDevice;
        if (serviceType == typeof(Xui.GPU.Backends.IShaderBackend))
            return new Xui.GPU.Backends.Metal.MslCodeGenerator();

        return NextServiceProvider.GetService(serviceType);
    }

    internal void Render(CGRect rect)
    {
        FrameEventRef frame = new (this.previousFrameTime, this.nextFrameTime);
        var context = this.drawingContext.Bind();
        RenderEventRef render = new (rect, frame, context);

        this.pendingInvalidate = false;
        this.Abstract.Render(ref render);
        this.SoftKeyboard.RefreshSelectionGeometry();
    }

    internal void InsertText(string text)
    {
        var insertTextEventRef = new InsertTextEventRef(text);
        (this.Abstract as Xui.Core.Abstract.IWindow.ISoftKeyboard)?.InsertText(ref insertTextEventRef);
    }

    internal void DeleteBackwards()
    {
        var deleteTextEventRef = new DeleteBackwardsEventRef();
        (this.Abstract as Xui.Core.Abstract.IWindow.ISoftKeyboard)?.DeleteBackwards(ref deleteTextEventRef);
    }
}
