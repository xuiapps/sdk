using System.Runtime.InteropServices;

namespace Xui.Middleware.DevTools;

/// <summary>Optional bridge to the startup-loaded native test profiler. Never unload a live CLR profiler.</summary>
internal sealed unsafe class NativeAllocationProfiler
{
    private readonly delegate* unmanaged<uint, void> begin;
    private readonly delegate* unmanaged<void> end;
    private readonly delegate* unmanaged<void> reset;
    private readonly delegate* unmanaged<nint> read;

    private NativeAllocationProfiler(string path)
    {
        var library = NativeLibrary.Load(path);
        var ready = (delegate* unmanaged<int>)NativeLibrary.GetExport(library, "XuiProfiler_Ready");
        if (ready() != 1)
            throw new InvalidOperationException("Allocation profiler was not initialized by the CLR. Set CORECLR_ENABLE_PROFILING, CORECLR_PROFILER and CORECLR_PROFILER_PATH before starting the app.");
        begin = (delegate* unmanaged<uint, void>)NativeLibrary.GetExport(library, "XuiProfiler_Begin");
        end = (delegate* unmanaged<void>)NativeLibrary.GetExport(library, "XuiProfiler_End");
        reset = (delegate* unmanaged<void>)NativeLibrary.GetExport(library, "XuiProfiler_Reset");
        read = (delegate* unmanaged<nint>)NativeLibrary.GetExport(library, "XuiProfiler_Read");
    }

    public static NativeAllocationProfiler? FromEnvironment() =>
        Environment.GetEnvironmentVariable("XUI_ALLOCATION_PROFILER") is { Length: > 0 } path ? new(path) : null;

    public void Begin(uint phase) => begin(phase);
    public void End() => end();
    public void Reset() => reset();
    public string Read() => Marshal.PtrToStringUTF8(read()) ?? throw new IOException("Native profiler returned no trace.");
}
