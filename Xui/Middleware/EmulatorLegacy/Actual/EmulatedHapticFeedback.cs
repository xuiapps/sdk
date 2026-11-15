using Xui.Core.Actual;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>Silent emulator feedback until a visual representation is available.</summary>
internal sealed class EmulatedHapticFeedback : IHapticFeedback
{
    public void Prepare(HapticEffect effect) { }
    public void Play(HapticEffect effect) { }
}
