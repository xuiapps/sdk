using System;
using System.Threading.Tasks;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.IOS.Actual;

/// <summary>View-facing iOS image handle backed by the window image catalog.</summary>
internal sealed class IOSImage : IImage
{
    private readonly IOSImageFactory factory;
    private IOSImageResource? resource;

    internal IOSImageResource? Resource => resource;

    internal IOSImage(IOSImageFactory factory) => this.factory = factory;

    public Size Size => resource is null ? Size.Empty : new Size(resource.Width, resource.Height);

    public void Load(string uri) => resource = factory.GetOrLoad(uri);

    public Task LoadAsync(string uri) => factory.GetOrLoadAsync(uri).ContinueWith(t => resource = t.Result);

    public void LoadPixels(int width, int height, ReadOnlySpan<byte> bgra32Data)
    {
        resource?.Dispose();
        resource = factory.UploadFromPixels(width, height, bgra32Data);
    }
}
