using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Drawing;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp;

/// <summary>An expandable demo surface with the same shadow as the surrounding cards.</summary>
internal sealed class MobileDemoExpander : Expander
{
    private IDesignSystem? design;

    public override Rect CullingFrame => Frame.Expand(MobileDemoCard.SharedShadowExtent);

    protected override void OnActivate()
    {
        base.OnActivate();
        design = GetService(typeof(IDesignSystem)) as IDesignSystem;
    }

    protected override void OnDeactivate()
    {
        design = null;
        base.OnDeactivate();
    }

    protected override void RenderCore(IContext context)
    {
        using var group = context.Group(new GroupOptions
        {
            Key = this, ViewBacking = true, Bounds = Frame, CornerRadius = design?.Shape.Large.TopLeft ?? 0,
            Shadow = new OutlineShadow(MobileDemoCard.SharedShadowColor, MobileDemoCard.SharedShadowExtent / 2, (0, 2)),
            HasUpdates = RequiresRender(context), CacheWhenHidden = true,
        });
        if (group.NeedsRedraw) base.RenderCore(context);
    }
}
