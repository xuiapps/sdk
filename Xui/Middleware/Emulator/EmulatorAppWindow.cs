using System;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Middleware.Emulator.Devices;
using Xui.Middleware.Emulator.UI;

namespace Xui.Middleware.Emulator;

/// <summary>
/// The ordinary desktop window that will host the emulator application UI.
/// </summary>
public sealed class EmulatorAppWindow : Window, IMacOSWindowStyle
{
    MacOSWindowTitle IMacOSWindowStyle.Title => MacOSWindowTitle.Hidden;
    MacOSWindowTitleHeight IMacOSWindowStyle.TitleHeight => MacOSWindowTitleHeight.Large;
    MacOSWindowBackground IMacOSWindowStyle.Background => MacOSWindowBackground.Glass;

    internal DeviceProfile CurrentDevice { get; } = DeviceProfiles.IPhone15Pro;
    private readonly AppView appView;

    /// <summary>Initializes the desktop host window.</summary>
    public EmulatorAppWindow(IServiceProvider context)
        : base(context)
    {
        appView = new AppView(CurrentDevice);
        Content = appView;
    }

    private EmulatedWindow? emulatedWindow;

    internal void Attach(EmulatedWindow emulatedWindow)
    {
        this.emulatedWindow = emulatedWindow;
        appView.Attach(emulatedWindow);
    }

    /// <summary>
    /// The desktop runtime owns the frame clock. Forward it to the virtual mobile
    /// window as well, so animation requests made by the emulated app keep running.
    /// </summary>
    public override void OnAnimationFrame(ref FrameEventRef e)
    {
        base.OnAnimationFrame(ref e);
        emulatedWindow?.OnAnimationFrame(ref e);
    }

    // Keyboard focus belongs to the emulated mobile surface, not the desktop
    // emulator controls. The host window is only the transport for these events.
    /// <inheritdoc/>
    public override void OnKeyDown(ref KeyEventRef e) => emulatedWindow?.OnKeyDown(ref e);

    /// <inheritdoc/>
    public override void OnChar(ref KeyEventRef e) => emulatedWindow?.OnChar(ref e);
}
