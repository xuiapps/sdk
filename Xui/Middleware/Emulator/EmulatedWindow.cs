using System;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Middleware.Emulator;

/// <summary>
/// The actual window surface exposed to an app inside the emulator.
/// Its state is mobile state; host operations are limited to showing, closing, and repainting.
/// </summary>
internal sealed class EmulatedWindow(
    Xui.Core.Abstract.IWindow appWindow,
    EmulatedPlatform platform) : Xui.Core.Actual.IWindow
{
    private string title = string.Empty;
    private bool requireKeyboard;

    public Xui.Core.Abstract.IWindow? Abstract => appWindow;

    public IServiceProvider NextServiceProvider => platform.Services;

    public string Title
    {
        get => title;
        set => title = value;
    }

    public bool RequireKeyboard
    {
        get => requireKeyboard;
        set => requireKeyboard = value;
    }

    public ITextMeasureContext? TextMeasureContext => null;

    public void Show() => platform.Show();

    public void Close() => platform.Close();

    public void Invalidate() => platform.RequestRender();

    public object? GetService(Type serviceType) => platform.Services.GetService(serviceType);

    internal void Render(ref RenderEventRef render)
    {
        var device = platform.Device;
        appWindow.DisplayArea = render.Rect;
        appWindow.SafeArea = render.Rect - device.SafeAreaInsetsPortrait;
        appWindow.ScreenCornerRadius = device.ScreenCornerRadius;
        appWindow.Render(ref render);
    }

    internal void DispatchTouch(ref TouchEventRef touch) => appWindow.OnTouch(ref touch);

    /// <summary>Delivers a desktop scroll gesture to the emulated display.</summary>
    internal void DispatchScrollWheel(ref ScrollWheelEventRef scroll, Point position)
    {
        scroll.Position = position;
        scroll.HasPosition = true;
        appWindow.OnScrollWheel(ref scroll);
    }

    internal void OnAnimationFrame(ref FrameEventRef frame) => appWindow.OnAnimationFrame(ref frame);

    internal void OnKeyDown(ref KeyEventRef key) => appWindow.OnKeyDown(ref key);

    internal void OnChar(ref KeyEventRef character) => appWindow.OnChar(ref character);
}
