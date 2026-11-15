using Xui.Core.Canvas;
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
        if (design is not null)
            RoundedRectShadow.Draw(context, Frame, design.Shape.Large,
                MobileDemoCard.SharedShadowExtent, MobileDemoCard.SharedShadowColor);
        base.RenderCore(context);
    }
}
