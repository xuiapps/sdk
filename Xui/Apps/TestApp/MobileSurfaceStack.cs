using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Independently retained sections, culled before creating native surfaces.</summary>
internal class MobileSurfaceStack : VerticalStack
{
    protected override void RenderCore(IContext context)
    {
        for (var i = 0; i < Count; i++)
        {
            var child = this[i];
            var bounds = child.CullingFrame.Expand(MobileDemoCard.SharedShadowExtent);
            if (RenderCullingFrame is { } viewport && !bounds.Intersects(viewport)) continue;
            using var group = context.Group(new GroupOptions
            {
                Key = child, ViewBacking = true, Bounds = bounds,
                HasUpdates = child.RequiresRender(context), CacheWhenHidden = true,
            });
            if (group.NeedsRedraw) RenderChild(context, child, bounds);
        }
    }
}
