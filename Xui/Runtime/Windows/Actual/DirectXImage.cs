using System.Threading.Tasks;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Windows.Actual;

/// <summary>
/// View-facing image handle backed by <see cref="DirectXImageFactory"/>.
/// Obtained via <c>window.GetService(typeof(IImage))</c>.
/// Call <see cref="Load"/> or <see cref="LoadAsync"/> to populate — analogous to
/// setting <c>img.src</c> in the browser.
/// </summary>
internal sealed class DirectXImage : IImage
{
    private readonly DirectXImageFactory factory;
    private DirectXImageResource? resource;

    internal DirectXImage(DirectXImageFactory factory) => this.factory = factory;

    /// <summary>Intrinsic size. Returns <see cref="Size.Empty"/> until loaded.</summary>
    public Size Size => resource?.Size ?? Size.Empty;

    /// <summary>D2D1 bitmap for the current frame, or null if not yet loaded.</summary>
    internal D2D1.Bitmap1? D2D1Bitmap => resource?.D2D1Bitmap;

    public void Load(string uri) =>
        resource = factory.GetOrLoad(uri);

    public Task LoadAsync(string uri) =>
        factory.GetOrLoadAsync(uri).ContinueWith(
            t => resource = t.Result,
            TaskScheduler.Default);
}
