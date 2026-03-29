using Xui.Core.Abstract.Events;
using Xui.Core.Debug;
using static Xui.Runtime.Windows.Win32.User32.Types;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// Abstracts the native Win32 host that <see cref="DirectXContext"/> renders into,
/// such as a regular <see cref="Win32Window"/>.
/// </summary>
internal interface IDirectXHost
{
    HWND Hwnd { get; }
    NFloat ExtendedFrameTopOffset { get; }
    InstrumentsAccessor Instruments { get; }
    void Render(RenderEventRef render);
}
