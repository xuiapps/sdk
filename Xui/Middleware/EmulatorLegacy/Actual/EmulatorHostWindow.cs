using System;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Math2D;
using Xui.Middleware.EmulatorLegacy.UI;
using static Xui.Core.Abstract.IWindow.IDesktopStyle;

namespace Xui.Middleware.EmulatorLegacy.Actual;

/// <summary>The ordinary desktop window that contains the emulator composition.</summary>
public sealed class EmulatorHostWindow : Window, IWindow.IDesktopStyle, IMacOSWindowStyle
{
    private EmulatorWindow? emulator;

    public EmulatorWindow Emulator => emulator ?? ((EmulatorHostServiceProvider)this.Context).Emulator;

    public EmulatorHostWindow(IServiceProvider context, EmulatorWindow emulator)
        : base(context)
    {
        this.emulator = emulator;
        this.Content = new EmulatorWindowView(emulator);
    }

    WindowBackdrop IWindow.IDesktopStyle.Backdrop => WindowBackdrop.Chromeless;
    DesktopWindowLevel IWindow.IDesktopStyle.Level => DesktopWindowLevel.Floating;
    Size? IWindow.IDesktopStyle.StartupSize =>
        (Emulator.CurrentDevice.LogicalResolution.Width + 48, Emulator.CurrentDevice.LogicalResolution.Height + 64);

    MacOSWindowBackground IMacOSWindowStyle.Background => MacOSWindowBackground.Glass;
    MacOSWindowTitleHeight IMacOSWindowStyle.TitleHeight => MacOSWindowTitleHeight.Large;
    MacOSWindowTitle IMacOSWindowStyle.Title => MacOSWindowTitle.Hidden;

    public override bool Closing() => Emulator.Closing();

    public override void Closed()
    {
        Emulator.Closed();
        base.Closed();
    }

    public override void OnAnimationFrame(ref FrameEventRef e)
    {
        base.OnAnimationFrame(ref e);
        Emulator.OnAnimationFrame(ref e);
    }

    public override void OnKeyDown(ref KeyEventRef e) => Emulator.OnKeyDown(ref e);

    public override void OnChar(ref KeyEventRef e) => Emulator.OnChar(ref e);
}
