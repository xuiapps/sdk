using Xui.Core.Canvas.Composition.GPU;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Owns the UIKit presentation container for a retained gpu node.</summary>
internal sealed class IOSGpuNode : GpuNode
{
    internal IOSCompositionPresenter.Surface? Presentation { get; set; }

    protected override void DisposeCore()
    {
        Presentation?.Dispose();
        Presentation = null;
        base.DisposeCore();
    }
}
