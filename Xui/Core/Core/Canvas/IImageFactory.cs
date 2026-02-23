namespace Xui.Core.Canvas;

/// <summary>
/// Provides access to platform image resources.
/// Obtain an instance from <see cref="Xui.Core.Actual.IWindow.ImageFactory"/> at view attach time.
/// </summary>
public interface IImageFactory
{
    /// <summary>
    /// Decodes the image at <paramref name="path"/>, uploads it to the GPU once,
    /// and returns an <see cref="IImage"/> handle.
    /// Results are cached by path — repeated calls with the same path return the same object.
    /// </summary>
    IImage? Load(string path);

    /// <summary>
    /// Fired when the factory invalidates all cached images (e.g. on device-lost).
    /// Subscribers should discard any held <see cref="IImage"/> references and reload.
    /// </summary>
    event Action? Invalidated;
}
