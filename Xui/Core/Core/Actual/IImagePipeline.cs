using Xui.Core.Canvas;

namespace Xui.Core.Actual;

/// <summary>
/// Platform image pipeline. Caches decoded GPU-resident images by URI and vends
/// <see cref="IImage"/> handles to views via the service-resolution chain.
/// Implement this on each platform window to participate in image loading.
/// </summary>
public interface IImagePipeline
{
    /// <summary>
    /// Creates a new <see cref="IImage"/> handle backed by this pipeline's catalog.
    /// The handle is initially empty; populate it with <see cref="IImage.Load"/>
    /// or <see cref="IImage.LoadAsync"/>.
    /// </summary>
    IImage CreateImage();
}
