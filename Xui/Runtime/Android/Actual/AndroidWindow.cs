using System.Buffers.Binary;
using Xui.Runtime.Android.GPU;
using Xui.GPU.Hardware;
using Xui.GPU.Backends;
using Xui.GPU.Backends.Hlsl;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Composition;
using Xui.Runtime.Android.Interop;

namespace Xui.Runtime.Android.Actual;

/// <summary>Binding-free Android window. The host supplies only a borrowed JNI Activity handle.</summary>
public sealed unsafe class AndroidWindow : IWindow, IDisposable
{
    private sealed class EmptyServices : IServiceProvider { internal static readonly EmptyServices Instance = new(); public object? GetService(Type type) => null; }
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private nint native;
    private GCHandle self;
    private AndroidResources? resources;
    private AndroidTextContext? text;
    private AndroidTextInput? input;
    private AndroidCompositionPresenter? presenter;
    private VulkanDevice? gpu;
    private readonly HlslCodeGenerator shaderBackend = new();
    private RetainedCanvasContext? context;
    private readonly AndroidDeviceInfo device = new();
    private bool dirty = true, disposed;
    private readonly WireBuffer request = new();
    private string title = "";
    internal static AndroidWindow? Current { get; private set; }
    internal static readonly ConcurrentQueue<Action> Pending = new();
    public Xui.Core.Abstract.IWindow? Abstract { get; private set; }
    public IServiceProvider NextServiceProvider { get; private set; } = EmptyServices.Instance;
    /// <summary>Optional host services; the renderer itself has no managed Android dependencies.</summary>
    public IServiceProvider? HostServices { get; set; }
    public string Title { get => title; set => title = value; }
    private bool requireKeyboard;
    public bool RequireKeyboard
    {
        get=>requireKeyboard;
        set {requireKeyboard=value;request.Clear();request.Int(value?1:0);AndroidBridge.Invoke(native,8,request);}
    }

    /// <summary>Creates a Java composition host on the Android UI thread.</summary>
    public AndroidWindow(nint javaVm, nint activity)
    {
        if (Current is not null) throw new InvalidOperationException("Only one Android window is supported.");
        self = GCHandle.Alloc(this);
        try { native = AndroidBridge.Create(javaVm, activity, GCHandle.ToIntPtr(self), &Callback); }
        catch { self.Free(); throw; }
        if (native == 0) { self.Free(); throw new InvalidOperationException("Cannot create Android composition host."); }
        Current = this;
    }
    internal void Attach(Xui.Core.Abstract.IWindow window, IServiceProvider services)
    {
        if (Abstract is not null) throw new InvalidOperationException("Android window already attached.");
        NextServiceProvider = services; Abstract = window;
        resources = new(native, services.GetService(typeof(IFontSource)) as IFontSource);
        text = new(native, resources);input=new(native); gpu = new(native);presenter = new(native, resources,gpu,Invalidate);
        context = new(text, presenter, presenter.CreateNode,presenter.CreateGpuNode);
    }
    public void Show() => dirty = true;
    public void Invalidate() => dirty = true;
    object? IServiceProvider.GetService(Type type) => type == typeof(IContext) || type == typeof(IMeasureContext) ? context :
        type == typeof(ITextMeasureContext) ? text : type == typeof(ITextInputSession) ? input : type == typeof(IGpuDevice) ? gpu?.IsSupported==true?gpu:null : type == typeof(IShaderBackend) ? shaderBackend : type == typeof(IDeviceInfo) ? device : type == typeof(IImagePipeline) ? resources : type == typeof(IImage) ? resources?.CreateImage() : HostServices?.GetService(type) ?? NextServiceProvider.GetService(type);
    public void Close() => Dispose();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Callback(nint user, int kind, long time, byte* data, int length)
    {
        try
        {
            var window = (AndroidWindow)GCHandle.FromIntPtr(user).Target!;
            if (!window.disposed) window.Event(kind, time, new ReadOnlySpan<byte>(data, length));
            return 0;
        }
        catch (Exception exception)
        {
            // Exceptions cannot cross a reverse P/Invoke boundary. JNI turns this into
            // a Java failure; the managed stack is preserved in logcat/stderr.
            Console.Error.WriteLine(exception); return -1;
        }
    }
    private void Event(int kind, long time, ReadOnlySpan<byte> data)
    {
        if (Abstract is null || context is null) return;
        if (kind == 1)
        {
            while (Pending.TryDequeue(out var action)) action();
            if (data.Length != 48) throw new InvalidOperationException("Invalid Android frame packet.");
            device.Update(F(data, 8), F(data, 9) != 0, F(data, 10) != 0, F(data, 11) != 0);
            float scale = F(data, 2);
            var area = new Rect(0, 0, F(data, 0) / scale, F(data, 1) / scale);
            var safe = new Rect(F(data, 4) / scale, F(data, 5) / scale,
                area.Width - (F(data, 4) + F(data, 6)) / scale, area.Height - (F(data, 5) + F(data, 7)) / scale);
            if (Abstract.DisplayArea != area || Abstract.SafeArea != safe) dirty = true;
            Abstract.DisplayArea = area; Abstract.SafeArea = safe;
            var now = TimeSpan.FromTicks(time / 100);
            var frame = new FrameEventRef(now, now + TimeSpan.FromSeconds(1 / Math.Max(1, F(data, 3))));
            Abstract.OnAnimationFrame(ref frame);
            if (!dirty || area.Width <= 0 || area.Height <= 0) return;
            dirty = false;
            context.BeginFrame(area, scale);
            try
            {
                var render = new RenderEventRef { Context = context, Frame = frame, Rect = area };
                Abstract.Render(ref render);
                context.EndFrame(); presenter!.Present(context);
            }
            catch { context.AbortFrame(); dirty = true; throw; }
        }
        else if(kind == 4)input!.Edit(data);
        else if(kind == 3)
        {
            if(data.Length!=24)throw new InvalidOperationException("Invalid GPU surface packet.");
            presenter!.BindGpu(BinaryPrimitives.ReadInt32LittleEndian(data),
                (nint)BinaryPrimitives.ReadInt64LittleEndian(data[16..]),
                BinaryPrimitives.ReadInt32LittleEndian(data[4..]),BinaryPrimitives.ReadInt32LittleEndian(data[8..]));
        }
        else if (kind == 2)
        {
            if (data.Length % 20 != 0 || data.Length > 32 * 20) throw new InvalidOperationException("Invalid pointer packet.");
            Span<Touch> touches = stackalloc Touch[data.Length / 20];
            for (int i = 0; i < touches.Length; i++)
            {
                var p = data[(i * 20)..];
                touches[i] = new Touch { Index = BinaryPrimitives.ReadInt32LittleEndian(p), Phase = (TouchPhase)BinaryPrimitives.ReadInt32LittleEndian(p[4..]), Position = (F(p, 2), F(p, 3)), Radius = F(p, 4) };
            }
            var input = new TouchEventRef(touches); Abstract.OnTouch(ref input);
        }
    }
    private static float F(ReadOnlySpan<byte> data, int index) => BinaryPrimitives.ReadSingleLittleEndian(data[(index * 4)..]);
    /// <summary>Pauses frame callbacks while the Activity is not visible.</summary>
    public void SetActive(bool active)
    { if(active)presenter?.ResumeGpu(); request.Clear(); request.Int(active ? 1 : 0); AndroidBridge.Invoke(native, 6, request); dirty = true; }
    public void Dispose()
    {
        if (disposed) return;
        if (Environment.CurrentManagedThreadId != ownerThread) throw new InvalidOperationException("Dispose Android windows on the UI thread.");
        disposed = true;
        context?.Dispose();input?.Dispose(); presenter?.Dispose(); gpu?.Dispose(); text?.Dispose(); resources?.Dispose(); request.Dispose();
        AndroidBridge.Destroy(native); native = 0;
        if (self.IsAllocated) self.Free();
        if (Current == this) Current = null;
        while (Pending.TryDequeue(out _)) { }
        Abstract?.Closed();
    }
}
