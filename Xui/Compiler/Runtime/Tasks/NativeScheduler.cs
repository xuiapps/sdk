namespace Xui.Compiler.Runtime.Tasks;

/// <summary>Queues managed continuations on the host's single event-loop agent.</summary>
public static class NativeScheduler
{
    /// <summary>The WASM emitter replaces this boundary with a deferred host callback.</summary>
    public static void Post(Action continuation) => throw new NotSupportedException("A browser task scheduler is required.");

    /// <summary>Typed entry point used by the host when a queued callback is ready.</summary>
    public static void Invoke(Action continuation) => continuation();
}
