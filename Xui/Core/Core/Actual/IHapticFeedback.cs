namespace Xui.Core.Actual;

/// <summary>
/// Optional, window-scoped tactile feedback. Resolve through the view's service
/// provider and call on the UI thread. Desktop runtimes may leave this unregistered.
/// Requests respect native settings and hardware capabilities; playback is not guaranteed.
/// </summary>
public interface IHapticFeedback
{
    /// <summary>
    /// Prepares an effect without vibrating. This is an optional latency hint;
    /// <see cref="Play"/> must work without it. Platforms may ignore the hint.
    /// </summary>
    void Prepare(HapticEffect effect);

    /// <summary>
    /// Plays a preset using the platform's closest available effect. Native platforms
    /// may coalesce rapid requests; this does not queue delayed pulses.
    /// </summary>
    void Play(HapticEffect effect);
}
