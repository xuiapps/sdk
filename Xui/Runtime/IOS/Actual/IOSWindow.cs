using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.CoreAnimation;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;
using System.Collections.Generic;
using static Xui.Runtime.IOS.Foundation;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

public class IOSWindow : UIWindow, Xui.Core.Actual.IWindow
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
    
    private Dictionary<nint, int> touchIndexMap = new Dictionary<nint, int>();

    private CADisplayLink displayLink;
    private TimeSpan previousFrameTime;
    private TimeSpan nextFrameTime;

    public IOSWindow(Xui.Core.Abstract.IWindow @abstract) : base(Class.New())
    {
        this.Abstract = @abstract;
        this.Title = "";
        this.RootView = new IOSWindowRootView(this);

        this.SoftKeyboard = new IOSSoftKeyboard(this);
        this.SoftKeyboard.NextResponder = this.RootView;

        this.DefaultResponder = new IOSDefaultResponder(this);
        this.DefaultResponder.NextResponder = this.RootView;

        this.RootViewController = new IOSRootViewController(this)
        {
            View = this.RootView
        };

        this.displayLink = CADisplayLink.DisplayLink(this, AnimationFrameSel);
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
                    this.SoftKeyboard.BecomeFirstResponder();
                }
                else
                {
                    this.DefaultResponder.BecomeFirstResponder();
                }

                this.requireKeyboard = value;
            }
        }
    }

    private void SendEvent(UIEventRef uiEventRef)
    {
        var type = uiEventRef.Type;
        var subtype = uiEventRef.Subtype;
        
        if (type == UIEvent.EventType.Touches)
        {
            var touches = uiEventRef.AllTouches;
            var count = (int)touches.Count;

            Debug.WriteLine($"UIEvent {type} {subtype} {touches.Count}");

            ReadOnlySpan<nint> touchPtrs = stackalloc nint[count];
            Span<Touch> touchPoints = stackalloc Touch[count];

            touches.GetValues(ref MemoryMarshal.GetReference(touchPtrs));
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
                var phase = MapPhase(touchRef.Phase);

                int index = 0;
                if (phase == TouchPhase.Start)
                {
                    while(touchIndexMap.ContainsKey(index))
                    {
                        index++;
                    }

                    touchIndexMap[touchRef.Self] = index;
                }
                else
                {
                    index = touchIndexMap[touchRef.Self];
                }

                touchPoints[i] = new Touch()
                {
                    Index = index,
                    Position = touchRef.LocationInView(null),
                    Radius = touchRef.Radius,
                    Phase = MapPhase(touchRef.Phase),
                };

                if (phase == TouchPhase.End)
                {
                    touchIndexMap.Remove(touchRef.Self);
                }
            }

            var touchEventRef = new TouchEventRef(touchPoints);
            this.Abstract.OnTouch(ref touchEventRef);
        }
        else
        {
            Debug.WriteLine($"UIEvent {type} {subtype}");
        }
    }

    private void AnimationFrame(nint caDisplayLink)
    {
        this.previousFrameTime = this.displayLink.Timestamp;
        this.nextFrameTime = this.displayLink.TargetTimestamp;
        var animationFrame = new FrameEventRef(this.previousFrameTime, this.nextFrameTime);
        this.Abstract.OnAnimationFrame(ref animationFrame);
    }

    private TouchPhase MapPhase(UITouch.Phase phase)
    {
        switch(phase)
        {
            case UITouch.Phase.Began: return TouchPhase.Start;
            case UITouch.Phase.Ended: return TouchPhase.End;
            default: return TouchPhase.Move;
        }
    }

    public string Title { get; set; }
    
    protected internal Xui.Core.Abstract.IWindow Abstract { get; }

    protected IOSWindowRootView RootView { get; }

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

    public void Invalidate() => this.RootView.SetNeedsDisplay();

    void Xui.Core.Actual.IWindow.Show() => this.MakeKeyAndVisible();

    object? IServiceProvider.GetService(Type serviceType) => null;

    internal void Render(CGRect rect)
    {
        FrameEventRef frame = new (this.previousFrameTime, this.nextFrameTime);
        RenderEventRef render = new (rect, frame);

        this.Abstract.Render(ref render);
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