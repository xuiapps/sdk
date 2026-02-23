using System;
using System.Diagnostics;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using static Xui.Runtime.Windows.D2D1;
using static Xui.Runtime.Windows.DXGI;
using static Xui.Runtime.Windows.Win32.Types;
using static Xui.Runtime.Windows.Win32.User32;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

public partial class Win32Window
{
    public partial class D2D : RenderTarget, IDisposable
    {
        static readonly D2D1.Factory3 D2D1Factory3;
        static readonly DWrite.Factory DWriteFactory;

        static D2D()
        {
            D2D1Factory3 = new D2D1.Factory3();
            DWriteFactory = new DWrite.Factory();
        }

        public D2D(Win32Window win32Window) : base(win32Window)
        {
        }

        protected D3D11.DeviceAndSwapChain DeviceAndSwapChain { get; private set; }

        protected Surface? DxgiSurface { get; private set; }
        protected D2D1.Device2? D2D1Device { get; private set; }
        protected D2D1.DeviceContext? D2D1DeviceContext { get; private set; }

        protected D2D1.RenderTarget? RenderTarget => this.D2D1DeviceContext;

        protected Direct2DContext Direct2DContext { get; private set; }

        protected D2D1.Bitmap1? D2DTargetBitmap { get; private set; }

        public DComp.Device? DCompDevice { get; private set; }

        public Win32ImageFactory? ImageFactory { get; private set; }

        private TimeSpan LastFrameTime = TimeSpan.Zero;
        private TimeSpan LastNextEstimatedFrameTime = TimeSpan.Zero;
        private TimeSpan NextEstimatedFrameTime = TimeSpan.Zero;

        public override bool HandleOnMessage(HWND hWnd, WindowMessage uMsg, WPARAM wParam, LPARAM lParam, out int result)
        {
            if (uMsg == WindowMessage.WM_ERASEBKGND)
            {
                result = 1;
                return true;
            }
            else if (uMsg == WindowMessage.WM_SIZE)
            {
                hWnd.GetClientRect(out var rc);
                SizeU size = new()
                {
                    Width = (uint)(rc.Right - rc.Left),
                    Height = (uint)(rc.Bottom - rc.Top)
                };

                if (this.D2D1DeviceContext != null)
                {
                    this.D2D1DeviceContext.SetTarget(null);
                    this.D2DTargetBitmap?.Dispose();
                    this.D2DTargetBitmap = null;
                    this.DxgiSurface?.Dispose();
                    this.DxgiSurface = null;
                    this.DeviceAndSwapChain.D3D11ImmediateContext?.ClearState();
                    this.DeviceAndSwapChain.DxgiSwapChain?.ResizeBuffers(0, size.Width, size.Height, Format.UNKNOWN, 0);
                }
            }
            else if (uMsg == WindowMessage.WM_PAINT)
            {
                this.Render();

                this.CreateWindowResources();
                this.CreateRenderTarget();
                this.CreateWindowSizeDependentResources();

                this.Render();
                result = 1;
                return true;
            }

            result = 0;
            return false;
        }

        private void CreateWindowResources()
        {
            if (this.DeviceAndSwapChain.DxgiSwapChain == null ||
                this.DeviceAndSwapChain.D3D11Device == null ||
                this.DeviceAndSwapChain.D3D11ImmediateContext == null)
            {
                this.DeviceAndSwapChain.Dispose();

                var hWnd = this.Win32Window.Hwnd;

                hWnd.GetClientRect(out var rc);
                D2D1.SizeU size = new SizeU()
                {
                    Width = (uint)(rc.Right - rc.Left),
                    Height = (uint)(rc.Bottom - rc.Top)
                };

                this.DeviceAndSwapChain = D3D11.CreateDeviceAndSwapChain(hWnd, size.Width, size.Height);
            }

            if (this.DeviceAndSwapChain.DxgiDevice != null && this.DCompDevice == null)
            {
                this.DCompDevice = DComp.Device.Create(this.DeviceAndSwapChain.DxgiDevice);
            }
        }

        private void CreateRenderTarget()
        {
            if (this.D2D1Device == null && this.DeviceAndSwapChain.DxgiDevice != null)
            {
                this.D2D1Device = D2D1Factory3.CreateDevice(this.DeviceAndSwapChain.DxgiDevice);
            }

            if (this.D2D1DeviceContext == null && this.D2D1Device != null)
            {
                this.D2D1DeviceContext = this.D2D1Device.CreateDeviceContext(DeviceContextOptions.None);
                this.Direct2DContext = new Direct2DContext(D2D1DeviceContext, D2D1Factory3, DWriteFactory, this.DeviceAndSwapChain.D3D11Device);

                if (this.DeviceAndSwapChain.D3D11Device != null)
                {
                    this.ImageFactory?.InvalidateAll();
                    this.ImageFactory = new Win32ImageFactory(this.DeviceAndSwapChain.D3D11Device, this.D2D1DeviceContext);
                }
            }
        }

        private void CreateWindowSizeDependentResources()
        {
            if (this.DxgiSurface == null && this.DeviceAndSwapChain.DxgiSwapChain != null)
            {
                this.DxgiSurface = this.DeviceAndSwapChain.DxgiSwapChain.GetBufferAsSurface();
            }

            if (this.D2DTargetBitmap == null && this.D2D1DeviceContext != null && this.DxgiSurface != null)
            {
                D2D1Factory3.GetDesktopDpi(out var dpiX, out var dpiY);
                BitmapProperties1 bitmapProperties1 = new BitmapProperties1()
                {
                    PixelFormat = new PixelFormat()
                    {
                        Format = Format.B8G8R8A8_UNORM,
                        AlphaMode = D2D1.AlphaMode.Premultiplied,
                    },
                    BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
                    DpiX = dpiX,
                    DpiY = dpiY
                };

                // https://learn.microsoft.com/en-us/windows/win32/direct2d/devices-and-device-contexts
                this.D2DTargetBitmap = this.D2D1DeviceContext.CreateBitmapFromDxgiSurface(this.DxgiSurface, in bitmapProperties1);

                if (this.D2DTargetBitmap != null)
                {
                    this.D2D1DeviceContext.SetTarget(this.D2DTargetBitmap);
                }
            }
        }

        private void Render()
        {
            if (this.RenderTarget != null && this.DeviceAndSwapChain.DxgiSwapChain != null)
            {
                var hWnd = this.Win32Window.Hwnd;

                hWnd.GetClientRect(out var rc);
                D2D1.SizeU size = new SizeU()
                {
                    Width = (uint)(rc.Right - rc.Left),
                    Height = (uint)(rc.Bottom - rc.Top)
                };

                PAINTSTRUCT ps = new();
                hWnd.BeginPaint(ref ps);

                FrameEventRef f = new FrameEventRef(this.LastNextEstimatedFrameTime, this.NextEstimatedFrameTime);
                RenderEventRef e = new RenderEventRef(new Rect(0, 0, size.Width, size.Height), f);

                this.RenderTarget.BeginDraw();
                // this.RenderTarget.Clear(new ColorF() { A = 0f, B = 0, G = 0, R = 0 });

                this.Direct2DContext.BeginDraw();
                Win32Platform.DisplayContextStack.Push(this.Direct2DContext);
                try
                {
                    this.Win32Window.Render(e);
                    this.Direct2DContext.EndDraw();
                }
                finally
                {
                    Win32Platform.DisplayContextStack.Pop();
                }

                this.RenderTarget.EndDraw();

                this.DeviceAndSwapChain.DxgiSwapChain.Present(0, 0);

                hWnd.EndPaint(ref ps);
            }
        }

        public void Dispose()
        {
            this.ImageFactory?.Dispose();
            this.ImageFactory = null;

            this.DeviceAndSwapChain.Dispose();
            if (this.DCompDevice != null)
            {
                this.DCompDevice.Dispose();
                this.DCompDevice = null;
            }

            GC.SuppressFinalize(this);
        }

        ~D2D()
        {
            this.DeviceAndSwapChain.Dispose();
            Debug.WriteLine($"Reached to finalizer for {this.GetType().FullName}. Treat as resource and call Dispose.");
        }
    }
}

