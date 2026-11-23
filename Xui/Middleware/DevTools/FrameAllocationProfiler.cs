using Xui.Middleware.DevTools.IO;

namespace Xui.Middleware.DevTools;

/// <summary>
/// UI-thread managed allocation counters. Call boundaries exclude transport, dispatch,
/// screenshots and result construction. Native allocations and other threads are not included.
/// </summary>
internal sealed class FrameAllocationProfiler
{
    private readonly NativeAllocationProfiler? native = NativeAllocationProfiler.FromEnvironment();
    public void Begin(uint phase) => native?.Begin(phase);
    public void End() => native?.End();

    public long Frames;
    public long AnimationBytes;
    public long RenderBytes;
    public long InputBytes;

    public void Reset()
    {
        native?.Reset();
        (Frames, AnimationBytes, RenderBytes, InputBytes) = (0, 0, 0, 0);
    }
    public ProfileResult Read() => new(Frames, AnimationBytes, RenderBytes, InputBytes, native?.Read());
}
