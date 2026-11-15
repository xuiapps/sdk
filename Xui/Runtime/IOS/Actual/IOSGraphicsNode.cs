using Xui.Core.Canvas.Composition.Graphics;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Owns the UIKit presentation container for a retained graphics node.</summary>
internal sealed class IOSGraphicsNode : GraphicsNode
{
    internal IOSCompositionPresenter.Surface? Presentation { get; set; }

    protected override void DisposeCore()
    {
        Presentation?.Dispose();
        Presentation = null;
        base.DisposeCore();
    }
}
