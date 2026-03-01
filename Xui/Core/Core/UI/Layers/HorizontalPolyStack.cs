// File: Xui/Core/UI/Layers/HorizontalPolyStack.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A horizontal stack of up to 8 heterogeneous layers. Children are laid out left-to-right
/// at their natural (desired) widths. Unused slots should be typed as <see cref="Empty"/>;
/// the JIT eliminates their bodies entirely via constrained-call devirtualisation of
/// <see cref="ILayer.IsEmpty"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="HorizontalMonoStack{TChild,TBuffer}"/>, each slot may be a different
/// struct type, enabling mixed compositions such as
/// <c>HorizontalPolyStack&lt;CheckBox, Label, Empty, …&gt;</c> without a homogeneous buffer.
/// </remarks>
public struct HorizontalPolyStack<T1, T2, T3, T4, T5, T6, T7, T8> : ILayer
    where T1 : struct, ILayer
    where T2 : struct, ILayer
    where T3 : struct, ILayer
    where T4 : struct, ILayer
    where T5 : struct, ILayer
    where T6 : struct, ILayer
    where T7 : struct, ILayer
    where T8 : struct, ILayer
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
    public T6 Item6;
    public T7 Item7;
    public T8 Item8;

    /// <summary>Pixel gap inserted between non-empty children.</summary>
    public nfloat Gap;

    public LayoutGuide Update(LayoutGuide guide)
    {
        if (guide.IsMeasure)
        {
            nfloat totalW = 0, maxH = 0;
            bool first = true;
            MeasureH(ref Item1, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item2, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item3, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item4, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item5, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item6, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item7, Gap, guide, ref totalW, ref maxH, ref first);
            MeasureH(ref Item8, Gap, guide, ref totalW, ref maxH, ref first);
            guide.DesiredSize = new Size(totalW, maxH);
        }

        if (guide.IsArrange)
        {
            nfloat x = guide.ArrangedRect.X;
            bool first = true;
            ArrangeH(ref Item1, Gap, guide, ref x, ref first);
            ArrangeH(ref Item2, Gap, guide, ref x, ref first);
            ArrangeH(ref Item3, Gap, guide, ref x, ref first);
            ArrangeH(ref Item4, Gap, guide, ref x, ref first);
            ArrangeH(ref Item5, Gap, guide, ref x, ref first);
            ArrangeH(ref Item6, Gap, guide, ref x, ref first);
            ArrangeH(ref Item7, Gap, guide, ref x, ref first);
            ArrangeH(ref Item8, Gap, guide, ref x, ref first);
        }

        if (guide.IsRender)
        {
            IMeasureContext? mc = guide.RenderContext;
            nfloat x = guide.ArrangedRect.X;
            bool first = true;
            RenderH(ref Item1, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item2, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item3, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item4, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item5, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item6, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item7, Gap, guide, mc, ref x, ref first);
            RenderH(ref Item8, Gap, guide, mc, ref x, ref first);
        }

        if (guide.IsAnimate && !guide.IsRender && !guide.IsArrange && !guide.IsMeasure)
        {
            AnimateH(ref Item1, guide);
            AnimateH(ref Item2, guide);
            AnimateH(ref Item3, guide);
            AnimateH(ref Item4, guide);
            AnimateH(ref Item5, guide);
            AnimateH(ref Item6, guide);
            AnimateH(ref Item7, guide);
            AnimateH(ref Item8, guide);
        }

        return guide;
    }

    // --- static helpers — one generic instantiation per concrete T, JIT-devirtualised ---

    private static void MeasureH<T>(ref T item, nfloat gap, LayoutGuide guide,
        ref nfloat totalW, ref nfloat maxH, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        if (!first) totalW += gap;
        first = false;
        var g = guide;
        g = item.Update(g);
        totalW += g.DesiredSize.Width;
        if (g.DesiredSize.Height > maxH) maxH = g.DesiredSize.Height;
    }

    private static void ArrangeH<T>(ref T item, nfloat gap, LayoutGuide guide,
        ref nfloat x, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        var rect = guide.ArrangedRect;
        if (!first) x += gap;
        first = false;
        var mg = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Measure,
            AvailableSize = rect.Size,
            MeasureContext = guide.MeasureContext,
        };
        mg = item.Update(mg);
        var g = guide;
        g.ArrangedRect = new Rect(x, rect.Y, mg.DesiredSize.Width, rect.Height);
        item.Update(g);
        x += mg.DesiredSize.Width;
    }

    private static void RenderH<T>(ref T item, nfloat gap, LayoutGuide guide,
        IMeasureContext? mc, ref nfloat x, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        var rect = guide.ArrangedRect;
        if (!first) x += gap;
        first = false;
        var mg = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Measure,
            AvailableSize = rect.Size,
            MeasureContext = mc,
        };
        mg = item.Update(mg);
        var g = guide;
        g.ArrangedRect = new Rect(x, rect.Y, mg.DesiredSize.Width, rect.Height);
        item.Update(g);
        x += mg.DesiredSize.Width;
    }

    private static void AnimateH<T>(ref T item, LayoutGuide guide)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        item.Update(guide);
    }
}
