using System.Threading.Tasks;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas;

/// <summary>
/// Represents a decoded, GPU-resident image. Acts as a self-loading resource handle —
/// analogous to <c>HTMLImageElement</c> in the browser.
/// <para>
/// Acquire instances via <c>GetService(typeof(IImage))</c> on any <see cref="Xui.Core.UI.View"/>.
/// Then set the source with <see cref="Load"/> (sync-if-cached) or <see cref="LoadAsync"/>
/// for first-time remote or large assets.
/// </para>
/// </summary>
public interface IImage
{
    /// <summary>
    /// Intrinsic size of the image in points. Returns <see cref="Size.Empty"/> until loaded.
    /// </summary>
    Size Size { get; }

    /// <summary>
    /// Loads the image from <paramref name="uri"/>.
    /// Returns immediately if the image is already in the platform catalog.
    /// For local packaged assets this may block on first call; prefer <see cref="LoadAsync"/>
    /// for anything that could be slow.
    /// </summary>
    void Load(string uri);

    /// <summary>
    /// Loads the image from <paramref name="uri"/> on a background thread.
    /// The returned task completes once the image is decoded and GPU-resident.
    /// Subsequent <see cref="Load"/> calls with the same URI return instantly from cache.
    /// </summary>
    Task LoadAsync(string uri);
}
