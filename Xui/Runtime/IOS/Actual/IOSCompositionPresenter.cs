using Xui.Core.Canvas.Composition;
using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Canvas.Composition.GPU;
using Xui.Core.Math2D;
using static Xui.Runtime.IOS.ObjC;
using static Xui.Runtime.IOS.UIKit;
using static Xui.Runtime.IOS.UIKit.CompositionNative;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Retained, non-interactive UIKit surfaces with explicitly supplied shadow outlines.</summary>
internal sealed partial class IOSCompositionPresenter(IOSDrawingContext drawing,
    Func<Xui.GPU.Hardware.Metal.IOSGpuDevice?> gpuDevice, Action invalidate) : IGpuSurfaceFactory
{
    public bool IsSupported => gpuDevice() is not null;
    public ICompositionGpuSurface CreateSurface() => new IOSGpuSurface(
        gpuDevice() ?? throw new NotSupportedException("Metal presentation is unavailable."), invalidate);

    private static readonly Sel New = new("new"), Alloc = new("alloc"), Release = new("release"), Layer = new("layer"),
        AddSubview = new("addSubview:"), InsertSubview = new("insertSubview:atIndex:"), Remove = new("removeFromSuperview"),
        Bounds = new("setBounds:"), Position = new("setPosition:"), Anchor = new("setAnchorPoint:"), Transform = new("setAffineTransform:"),
        Opaque = new("setOpaque:"),
        Interaction = new("setUserInteractionEnabled:"), Clips = new("setClipsToBounds:"), Radius = new("setCornerRadius:"),
        Background = new("setBackgroundColor:"), ShadowPath = new("setShadowPath:"), ShadowColor = new("setShadowColor:"),
        BorderColor = new("setBorderColor:"), BorderWidth = new("setBorderWidth:"),
        ShadowRadius = new("setShadowRadius:"), ShadowOpacity = new("setShadowOpacity:"), ShadowOffset = new("setShadowOffset:"),
        EffectWithStyle = new("effectWithStyle:"), InitEffect = new("initWithEffect:"), ContentView = new("contentView"),
        Begin = new("begin"), Commit = new("commit"), DisableActions = new("setDisableActions:"),
        CornerConfig = new("setCornerConfiguration:"), Capsule = new("capsuleConfigurationWithMaximumRadius:");

    public void Present(GraphicsNode root, UIView host, NFloat scale)
    {
        var transaction = objc_getClass("CATransaction");
        objc_msgSend(transaction, Begin); objc_msgSend(transaction, DisableActions, true);
        try { PresentNode(root, host, scale, root: true); }
        finally { objc_msgSend(transaction, Commit); }
    }
    private void PresentNode(Node node, nint parent, NFloat scale, bool root = false, int index = 0)
    {
        var native = node switch
        {
            IOSGraphicsNode graphics => graphics.Presentation ??= new Surface(graphics, drawing),
            IOSGpuNode gpuContainer => gpuContainer.Presentation ??= new Surface(gpuContainer, drawing),
            _ => throw new InvalidOperationException("Expected an iOS composition node."),
        };
        // Keep the composition root behind UIKit's text handles and edit menu overlays.
        if (native.Parent != parent || native.Index != index)
        {
            objc_msgSend(parent, InsertSubview, native.Container, (nint)index);
            native.Parent=parent; native.Index=index;
        }
        var options = node.Options;
        var bounds = node.RasterBounds;
        var radius = NFloat.Min(options.CornerRadius,NFloat.Min(bounds.Width,bounds.Height)/2);
        ConfigureGeometry(native.Container,bounds,node.Placement);
        ConfigureGeometry(native.Body,bounds,AffineTransform.Identity);
        objc_msgSend(native.Body,Clips,options.ClipToBounds);
        objc_msgSend(objc_msgSend_retIntPtr(native.Body,Layer),Radius,radius);
        var background=options.Background ?? Colors.Transparent;
        if (!native.HasBackground || !background.Equals(native.BackgroundColor))
        { SetColor(objc_msgSend_retIntPtr(native.Body,Layer),Background,background); native.BackgroundColor=background; native.HasBackground=true; }
        ConfigureShadow(native,node,bounds,radius);
        var border = options.Border ?? default;
        if (!native.HasBorder || native.Border != border)
        {
            var bodyLayer = objc_msgSend_retIntPtr(native.Body,Layer);
            SetColor(bodyLayer,BorderColor,border.Color);
            objc_msgSend(bodyLayer,BorderWidth,border.Width);
            native.Border=border; native.HasBorder=true;
        }
        if(native.Material != options.Backdrop)
        {
            // Move the foreground out before replacing its effect host.
            if(native.Drawing is { } foreground) objc_msgSend(native.Body,AddSubview,foreground);
            if(native.Effect!=0) { objc_msgSend(native.Effect,Remove); objc_msgSend(native.Effect,Release); native.Effect=0; }
            native.Material=options.Backdrop;
            if(options.Backdrop!=BackdropMaterial.None)
            {
                nint cls = options.Backdrop==BackdropMaterial.LiquidGlass ? objc_getClass("UIGlassEffect") : 0;
                var effect=objc_msgSend_retIntPtr(cls!=0 ? cls : objc_getClass("UIBlurEffect"),EffectWithStyle,cls!=0 ? (nint)0 : (nint)8);
                native.Effect=objc_msgSend_retIntPtr(objc_msgSend_retIntPtr(objc_getClass("UIVisualEffectView"),Alloc),InitEffect,effect);
                objc_msgSend(native.Effect,Interaction,false);
                objc_msgSend(native.Body,InsertSubview,native.Effect,(nint)0);
                if(native.Drawing is { } content) objc_msgSend(objc_msgSend_retIntPtr(native.Effect,ContentView),AddSubview,content);
            }
        }
        if(native.Effect!=0)
        {
            // Effect views use conventional zero-origin bounds; their content lives in that local space.
            SetRect(native.Effect, UIView.SetFrameSel, bounds);
            var configClass=objc_getClass("UICornerConfiguration");
            if(configClass!=0)
            {
                var config=SendObjectFloat(configClass,Capsule,radius);
                objc_msgSend(native.Effect,CornerConfig,config);
            }
            else { objc_msgSend(objc_msgSend_retIntPtr(native.Effect,Layer),Radius,radius); objc_msgSend(native.Effect,Clips,true); }
            if(native.Drawing is { } foreground) SetRect(foreground, UIView.SetFrameSel, new Rect(0, 0, bounds.Width, bounds.Height));
        }
        else if(native.Drawing is { } foreground) SetRect(foreground, UIView.SetFrameSel, bounds);
        native.Drawing?.Update(scale);
        if (node is GpuNode { Surface: IOSGpuSurface gpu })
        {
            // The GPU leaf replaces canvas drawing, while its container supplies clipping,
            // transforms, borders and shadows exactly like an ordinary composition group.
            if (native.Gpu != gpu)
            {
                if (native.Gpu is not null) objc_msgSend(native.Gpu, Remove);
                native.Gpu = gpu;
                objc_msgSend(native.Body, InsertSubview, gpu, native.Effect != 0 ? (nint)1 : (nint)0);
            }
            gpu.Present(bounds, scale);
        }
        if(node is GraphicsNode graphicsNode)
            foreach(var hidden in graphicsNode.HiddenChildren)
            {
                var surface = hidden switch
                {
                    IOSGraphicsNode graphics => graphics.Presentation,
                    IOSGpuNode gpuNode => gpuNode.Presentation,
                    _ => null,
                };
                surface?.Detach();
            }
        for(int i=0;i<node.Count;i++) PresentNode(node[i],native.Body,scale,index:i+1);
    }
    private static void ConfigureGeometry(nint view, Rect bounds, AffineTransform matrix)
    {
        var layer=objc_msgSend_retIntPtr(view,Layer);
        SetPoint(layer,Anchor,new CGPoint(0,0));
        SetRect(view,Bounds,bounds);
        SetPoint(layer,Position,matrix*bounds.TopLeft);
        SetMatrix(layer,Transform,new CGAffineTransform(matrix.A,matrix.B,matrix.C,matrix.D,0,0));
    }
    private static void ConfigureShadow(Surface surface,Node node,Rect bounds,NFloat radius)
    {
        var layer=objc_msgSend_retIntPtr(surface.Container,Layer);
        if(node.Options.Shadow is not { } shadow || shadow.Color.Alpha==0)
        { SendFloat(layer,ShadowOpacity,0); return; }
        if(surface.ShadowBounds!=bounds || surface.ShadowCorner!=radius || !surface.HasShadow)
        {
            using var path=CGPathRef.CreateWithRoundedRect(bounds,radius);
            objc_msgSend(layer,ShadowPath,path); surface.ShadowBounds=bounds; surface.ShadowCorner=radius; surface.HasShadow=true;
        }
        if (!surface.HasShadowColor || !surface.ShadowPaint.Equals(shadow.Color))
        { SetColor(layer,ShadowColor,shadow.Color); surface.ShadowPaint=shadow.Color; surface.HasShadowColor=true; }
        SendFloat(layer,ShadowOpacity,1);
        objc_msgSend(layer,ShadowRadius,shadow.Radius);
        SetSize(layer,ShadowOffset,new CGSize(shadow.Offset.X,shadow.Offset.Y));
    }
    private static void SetColor(nint layer,Sel selector,Color color)
    {
        using var cg=CGColorRef.RGBA(color.Red,color.Green,color.Blue,color.Alpha);
        objc_msgSend(layer,selector,cg);
    }
    internal sealed class Surface : IDisposable
    {
        public nint Container=Create(),Body=Create(),Effect;
        public readonly IOSCompositionDrawingView? Drawing;
        public IOSGpuSurface? Gpu;
        public BackdropMaterial Material;
        public bool HasShadow, HasBackground, HasShadowColor;
        public bool HasBorder;
        public OutlineBorder Border;
        public Color BackgroundColor, ShadowPaint;
        public nint Parent;
        public int Index=-1;
        public Rect ShadowBounds;
        public NFloat ShadowCorner;
        public Surface(Node node, IOSDrawingContext drawing)
        {
            objc_msgSend(Container,AddSubview,Body);
            if(node is GraphicsNode graphics)
            {
                Drawing = new IOSCompositionDrawingView(graphics, drawing);
                objc_msgSend(Body,AddSubview,Drawing);
            }
        }
        private static nint Create()
        {
            var view=objc_msgSend_retIntPtr(objc_getClass("UIView"),New);
            objc_msgSend(view,Interaction,false); objc_msgSend(view,Opaque,false); return view;
        }
        public void Detach()
        {
            if(Parent==0) return;
            objc_msgSend(Container,Remove); Parent=0; Index=-1;
        }
        public void Dispose()
        {
            if(Container==0) return;
            objc_msgSend(Container,Remove);
            if(Effect!=0) { objc_msgSend(Effect,Release); Effect=0; }
            if(Drawing is not null) { objc_msgSend(Drawing,Remove); Drawing.Dispose(); }
            objc_msgSend(Body,Release); objc_msgSend(Container,Release);
            Container=Body=0;
        }
    }
}
