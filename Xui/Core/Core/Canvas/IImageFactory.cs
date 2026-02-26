using System.Threading.Tasks;

namespace Xui.Core.Canvas;

/// <summary>
/// Provides access to platform image resources.
/// Obtain an instance from <see cref="Xui.Core.Actual.IWindow.ImageFactory"/> at view attach time.
/// <para>
/// The factory caches images by URI. On device-lost, the platform implementation rehydrates
/// all cached images transparently — callers may hold <see cref="IImage"/> references across
/// device reconnects without reloading.
/// </para>
/// </summary>
public interface IImageFactory
{
    /// <summary>
    /// Returns the cached image if already loaded; otherwise decodes and uploads it synchronously.
    /// Prefer <see cref="LoadAsync"/> for first-time loads of large or remote assets.
    /// </summary>
    IImage? Load(string uri);

    /// <summary>
    /// Decodes and uploads the image asynchronously on a background thread.
    /// Subsequent calls to <see cref="Load"/> with the same URI return instantly from cache.
    /// </summary>
    Task<IImage?> LoadAsync(string uri);
}
