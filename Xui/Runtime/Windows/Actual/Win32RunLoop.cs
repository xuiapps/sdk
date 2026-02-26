using System;
using System.Collections.Concurrent;
using System.Threading;
using Xui.Core.Abstract.Events;
using Xui.Core.Debug;
using CoreRuntime = Xui.Core.Actual.Runtime;
using static Xui.Runtime.Windows.Win32.User32;

namespace Xui.Runtime.Windows.Actual;

public class Win32RunLoop : Xui.Core.Actual.IRunLoop, Xui.Core.Actual.IDispatcher
{
    // From WinUser.h
    public const uint PM_REMOVE = 0x0001;

    private readonly SynchronizationContext synchronizationContext;
    private readonly ConcurrentQueue<Action> postQueue = new();

    protected Xui.Core.Abstract.Application Application { get; }

    // Devices (app lifetime)
    private DXGI.Device? dxgiDevice;
    private DComp.Device? dCompDevice;

    // Timing state (app lifetime)
    private TimeSpan lastCurrent;

    public Win32RunLoop(Xui.Core.Abstract.Application application)
    {
        this.synchronizationContext = new Win32SynchronizationContext(this);
        SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

        this.Application = application;
    }

    public void Post(Action callback)
    {
        if (callback == null)
        {
            return;
        }

        this.postQueue.Enqueue(callback);
    }

    public int Run()
    {
        var instruments = CoreRuntime.CurrentInstruments;
        instruments.Log(Scope.Application, LevelOfDetail.Essential, $"Win32RunLoop.Run starting");

        this.Application.Start();

        instruments.Log(Scope.Application, LevelOfDetail.Essential, $"Win32RunLoop.Run Application.Start completed");

        unsafe
        {
            // Create once
            D3D11.CreateDevice(out var d3d11Device, out var _, out var _);
            this.dxgiDevice = new DXGI.Device(d3d11Device.QueryInterface(in DXGI.Device.IID));
            this.dCompDevice = DComp.Device.Create(this.dxgiDevice);
        }

        instruments.Log(Scope.Application, LevelOfDetail.Essential, $"Win32RunLoop.Run entering message loop");

        MSG m = new MSG();

        while(m.message != WindowMessage.WM_QUIT)
        {
            HandleInput(ref m);
            ApplicationMessages();
            AnimationClockAndRender();
            WaitForNextFrame();
        }

        instruments.Log(Scope.Application, LevelOfDetail.Essential, $"Win32RunLoop.Run exiting");
        this.Application.OnExit();
        return 0;
    }

    public void Quit() => PostQuitMessage(0);

    private static unsafe void WaitForNextFrame()
    {
        DComp.DCompositionWaitForCompositorClock(0, 0, 32);

        // var windows = Win32Platform.Instance.Windows;
        // int count = 0;

        // Span<nint> handles = stackalloc nint[windows.Count];
        // foreach (var w in windows)
        // {
        //     if (w.Renderer is Win32Window.D2DComp d2dComp && d2dComp.FrameLatencyHandle != 0)
        //     {
        //         handles[count++] = d2dComp.FrameLatencyHandle;
        //     }
        // }

        // fixed (nint* ptr = handles)
        // {
        //     DComp.DCompositionWaitForCompositorClock((uint)count, (nint)ptr, 16);
        // }
    }

    private void AnimationClockAndRender()
    {
        var stats = this.dCompDevice!.GetFrameStatistics();

        var previous = stats.CurrentTimeSpan;
        var next = stats.NextEstimatedFrameTimeSpan;

        FrameEventRef @event = new FrameEventRef(previous, next);
        for (int i = 0; i < Win32Platform.Instance.Windows.Count; i++)
        {
            var w = Win32Platform.Instance.Windows[i];
            w.OnAnimationFrame(ref @event);
            w.Render();
        }
    }

    private void ApplicationMessages()
    {
        while (this.postQueue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }

    private static void HandleInput(ref MSG m)
    {
        while (PeekMessage(ref m, 0, 0, 0, PM_REMOVE) && m.message != WindowMessage.WM_QUIT)
        {
            TranslateMessage(ref m);
            DispatchMessage(ref m);
        }
    }
}
