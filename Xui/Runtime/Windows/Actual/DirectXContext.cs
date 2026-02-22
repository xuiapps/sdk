using System;
using Xui.Core.Abstract.Events;
using Xui.Core.Debug;
using CoreRuntime = Xui.Core.Actual.Runtime;
using Xui.Core.Math2D;
using Xui.Runtime.Windows.Win32;
using static Xui.Runtime.Windows.D2D1;
using static Xui.Runtime.Windows.DXGI;
using static Xui.Runtime.Windows.Win32.User32;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

public sealed class DirectXContext
{
    private readonly Win32Window win32Window;

    /** 1.0 @ 100%, 2.0 @ 200% */
    private float DpiScale = 1.0f;
    private float InverseDpiScale = 1.0f;

    public DirectXContext(Win32Window win32Window)
    {
        this.win32Window = win32Window;
    }

    // Composition SwapChain
    private D3D11.Device? D3D11Device { get; set; }
    private D3D11.FeatureLevel D3D11FeatureLevel { get; set; }
    private D3D11.DeviceContext? D3D11DeviceContext { get; set; }
    private DXGI.Device? DXGIDevice { get; set; }
    private DXGI.Factory2? DXGIFactory2 { get; set; }
    private DXGI.SwapChain1? SwapChain1 { get; set; }

    // Direct2D
    private D2D1.Factory3? D2D1Factory3 { get; set; }
    private D2D1.Device1? D2D1Device1 { get; set; }
    private D2D1.DeviceContext? D2D1DeviceContext { get; set; }

    private DXGI.Surface? DXGISurface { get; set; }
    private D2D1.Bitmap1? D2D1Bitmap1 { get; set; }

    // DComposition
    private DComp.Device? DCompDevice { get; set; }
    private DComp.Target? DCompTarget { get; set; }
    private DComp.Visual? DCompVisual { get; set; }

    internal DWrite.Factory? DWriteFactory { get; private set; }
    private Direct2DContext? Direct2DContext { get; set; }
    public DirectXBitmapContext? ImageContext { get; private set; }

    /// <summary>
    /// IDXGISwapChain2::GetFrameLatencyWaitableObject() handle.
    /// </summary>
    public nint FrameLatencyHandle { get; private set; }

    // Frame
    private TimeSpan LastFrameTime = TimeSpan.Zero;
    private TimeSpan LastNextEstimatedFrameTime = TimeSpan.Zero;
    private TimeSpan NextEstimatedFrameTime = TimeSpan.Zero;

    /// <summary>
    /// Initializes the swapchain + D2D target + DComp visual tree.
    /// Must not be done in WM_PAINT (we validate there only).
    /// </summary>
    public void EnsureInitialized(HWND hWnd)
    {
        if (this.SwapChain1 != null)
        {
            return;
        }

        unsafe
        {
            this.UpdateDPI(hWnd);

            // Composition SwapChain
            D3D11.CreateDevice(out var d3d11Device, out var d3d11FeatureLevel, out var d3d11DeviceContext);
            this.D3D11Device = d3d11Device;
            this.D3D11FeatureLevel = d3d11FeatureLevel;
            this.D3D11DeviceContext = d3d11DeviceContext;

            this.DXGIDevice = new DXGI.Device(this.D3D11Device.QueryInterface(in DXGI.Device.IID));

            using var adapter = this.DXGIDevice.GetAdapter();
            this.DXGIFactory2 = adapter.GetParentFactory2();

            hWnd.GetClientRect(out var rect);

            uint logicalW = (uint)Math.Max(1, rect.Right - rect.Left);
            uint logicalH = (uint)Math.Max(1, rect.Bottom - rect.Top);

            uint physicalW = (uint)Math.Max(1, logicalW * this.DpiScale);
            uint physicalH = (uint)Math.Max(1, logicalH * this.DpiScale);

            SwapChainDesc1 swapChainDesc1 = new SwapChainDesc1()
            {
                Width = physicalW,
                Height = physicalH,
                Format = Format.B8G8R8A8_UNORM,
                Stereo = false,
                SampleDesc = new SampleDesc()
                {
                    Count = 1,
                    Quality = 0,
                },
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = DXGI.AlphaMode.Premultiplied,
                Flags = (uint)DXGI.SwapChainFlags.FrameLatencyWaitableObject
            };

            this.SwapChain1 = this.DXGIFactory2.CreateSwapChainForComposition(this.DXGIDevice, swapChainDesc1);

            this.RefreshFrameLatencyHandle();

            // Direct2D
            this.D2D1Factory3 = new D2D1.Factory3();
            this.D2D1Device1 = this.D2D1Factory3.CreateDevice(this.DXGIDevice);
            this.D2D1DeviceContext = this.D2D1Device1.CreateDeviceContext(D2D1.DeviceContextOptions.None);

            // Bind backbuffer as D2D target
            this.RecreateSizeDependentTargets();

            this.DWriteFactory = new DWrite.Factory();
            this.Direct2DContext = new Direct2DContext(this.D2D1DeviceContext, D2D1Factory3, DWriteFactory);
            this.ImageContext = new DirectXBitmapContext(this.D3D11Device, this.D2D1DeviceContext);

            // DComposition
            this.DCompDevice = DComp.Device.Create(this.DXGIDevice);
            this.DCompTarget = this.DCompDevice.CreateTargetForHwnd(hWnd, false);
            this.DCompVisual = this.DCompDevice.CreateVisual();
            this.DCompVisual.SetContent(this.SwapChain1);
            this.DCompTarget.SetRoot(this.DCompVisual);
            this.DCompDevice.Commit();
        }
    }

    public void ResizeBuffers(HWND hWnd, uint physicalW, uint physicalH)
    {
        if (this.SwapChain1 != null && this.D2D1DeviceContext != null)
        {
            // Detach target before resizing
            this.D2D1DeviceContext.SetTarget(null);

            // Release size-dependent resources
            this.D2D1Bitmap1?.Dispose();
            this.D2D1Bitmap1 = null;

            this.DXGISurface?.Dispose();
            this.DXGISurface = null;

            // Resize swapchain buffers
            this.SwapChain1.ResizeBuffers(
                bufferCount: 2,
                width: physicalW,
                height: physicalH,
                newFormat: Format.B8G8R8A8_UNORM,
                swapChainFlags: DXGI.SwapChainFlags.FrameLatencyWaitableObject);

            // Rebind backbuffer as D2D target
            this.RecreateSizeDependentTargets();

            this.RefreshFrameLatencyHandle();
        }
    }

    public void Render()
    {
        if (this.DCompDevice != null)
        {
            var frameStats = this.DCompDevice.GetFrameStatistics();
            var lastFrameTime = TimeSpan.FromSeconds(frameStats.LastFrameTime / (double)frameStats.TimeFrequency);
            var nextEstimatedFrameTime = TimeSpan.FromSeconds(frameStats.NextEstimatedFrameTime / (double)frameStats.TimeFrequency);

            var animationFrame = new FrameEventRef(this.LastNextEstimatedFrameTime, nextEstimatedFrameTime);

            this.LastFrameTime = lastFrameTime;
            this.LastNextEstimatedFrameTime = this.NextEstimatedFrameTime;
            this.NextEstimatedFrameTime = nextEstimatedFrameTime;

            if (this.D2D1DeviceContext == null || this.Direct2DContext == null || this.SwapChain1 == null)
            {
                return;
            }

            var hWnd = this.win32Window.Hwnd;
            hWnd.GetClientRect(out var rc);

            uint logicalW = (uint)Math.Max(1, rc.Right - rc.Left);
            uint logicalH = (uint)Math.Max(1, rc.Bottom - rc.Top);

            var topOffset = this.win32Window.ExtendedFrameTopOffset;
            var dipW = logicalW / this.DpiScale;
            var dipH = (logicalH - topOffset) / this.DpiScale;

            CoreRuntime.CurrentInstruments.Log(Scope.Rendering, LevelOfDetail.Diagnostic,
                $"DirectXContext.Render client=({logicalW}, {logicalH}) dpi={this.DpiScale:F2} dip=({dipW:F1}, {dipH:F1}) topOffset={topOffset}");

            var frame = new FrameEventRef(this.LastNextEstimatedFrameTime, this.NextEstimatedFrameTime);
            var render = new RenderEventRef(
                new Rect(0, 0, dipW, dipH),
                frame);

            this.D2D1DeviceContext.BeginDraw();
            this.D2D1DeviceContext.Clear(new ColorF { A = 0f });

            this.Direct2DContext.BeginDraw();

            this.D2D1DeviceContext.SetTransform(new D2D1.Matrix3X2F
            {
                _11 = this.DpiScale,
                _22 = this.DpiScale,
                _12 = 0,
                _21 = 0,
                _31 = 0,
                _32 = (float)topOffset
            });

            Win32Platform.DisplayContextStack.Push(this.Direct2DContext);
            try
            {
                this.win32Window.Render(render);
                this.Direct2DContext.EndDraw();
            }
            finally
            {
                Win32Platform.DisplayContextStack.Pop();
            }

            this.D2D1DeviceContext.EndDraw();

            this.SwapChain1.Present(0, Present.DoNotWait);
            this.DCompDevice.Commit();
        }
    }

    private void RecreateSizeDependentTargets()
    {
        if (this.SwapChain1 == null || this.D2D1DeviceContext == null)
        {
            return;
        }

        // Release old first
        this.D2D1Bitmap1?.Dispose();
        this.D2D1Bitmap1 = null;

        this.DXGISurface?.Dispose();
        this.DXGISurface = null;

        // Bind new backbuffer
        this.DXGISurface = this.SwapChain1.GetBufferAsSurface(0);

        D2D1.BitmapProperties1 bitmapProperties1 = new D2D1.BitmapProperties1()
        {
            PixelFormat = new D2D1.PixelFormat()
            {
                AlphaMode = D2D1.AlphaMode.Premultiplied,
                Format = Format.B8G8R8A8_UNORM
            },
            BitmapOptions = D2D1.BitmapOptions.Target | D2D1.BitmapOptions.CannotDraw,
        };

        this.D2D1Bitmap1 =
            this.D2D1DeviceContext.CreateBitmapFromDxgiSurface(
                this.DXGISurface,
                bitmapProperties1);

        this.D2D1DeviceContext.SetTarget(this.D2D1Bitmap1);
    }

    private void RefreshFrameLatencyHandle()
    {
        if (this.SwapChain1 == null)
        {
            this.FrameLatencyHandle = 0;
            return;
        }

        unsafe
        {
            void* sc2Ptr = COM.Unknown.QueryInterface(this.SwapChain1, in DXGI.SwapChain2.IID);
            if (sc2Ptr == null)
            {
                this.FrameLatencyHandle = 0;
                return;
            }

            using var sc2 = new DXGI.SwapChain2(sc2Ptr);
            sc2.SetMaximumFrameLatency(1);

            this.FrameLatencyHandle = sc2.GetFrameLatencyWaitableObject();
        }
    }

    private void UpdateDPI(User32.Types.HWND hWnd)
    {
        var dpi = hWnd.DPI;
        this.DpiScale = dpi / 96f;
        this.InverseDpiScale = 1.0f / this.DpiScale;
    }
}
