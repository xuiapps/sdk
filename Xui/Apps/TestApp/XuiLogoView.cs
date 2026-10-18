using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Sizes and positions the fixed-size canvas logo within the mobile home header.</summary>
internal sealed class XuiLogoView : View
{
    protected override Size MeasureCore(Size available, IMeasureContext context) => (64, 64);

    protected override void RenderCore(IContext context)
    {
        context.Save();
        try
        {
            context.Translate(this.Frame.TopLeft);
            XuiLogo.Instance.Render(context);
        }
        finally
        {
            context.Restore();
        }
    }
}
