// File: Xui/Core/UI/Layers/VerticalPolyStack.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A vertical stack of up to 8 heterogeneous layers. Children are laid out top-to-bottom
/// at their natural (desired) heights. Unused slots should be typed as <see cref="Empty"/>;
/// the JIT eliminates their bodies entirely via constrained-call devirtualisation of
/// <see cref="ILayer.IsEmpty"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="VerticalMonoStack{TChild,TBuffer}"/>, each slot may be a different
/// struct type, enabling mixed compositions without a homogeneous buffer.
/// </remarks>
public struct VerticalPolyStack<T1, T2, T3, T4, T5, T6, T7, T8> : ILayer
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
            nfloat maxW = 0, totalH = 0;
            bool first = true;
            MeasureV(ref Item1, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item2, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item3, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item4, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item5, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item6, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item7, Gap, guide, ref maxW, ref totalH, ref first);
            MeasureV(ref Item8, Gap, guide, ref maxW, ref totalH, ref first);
            guide.DesiredSize = new Size(maxW, totalH);
        }

        if (guide.IsArrange)
        {
            nfloat y = guide.ArrangedRect.Y;
            bool first = true;
            ArrangeV(ref Item1, Gap, guide, ref y, ref first);
            ArrangeV(ref Item2, Gap, guide, ref y, ref first);
            ArrangeV(ref Item3, Gap, guide, ref y, ref first);
            ArrangeV(ref Item4, Gap, guide, ref y, ref first);
            ArrangeV(ref Item5, Gap, guide, ref y, ref first);
            ArrangeV(ref Item6, Gap, guide, ref y, ref first);
            ArrangeV(ref Item7, Gap, guide, ref y, ref first);
            ArrangeV(ref Item8, Gap, guide, ref y, ref first);
        }

        if (guide.IsRender)
        {
            IMeasureContext? mc = guide.RenderContext;
            nfloat y = guide.ArrangedRect.Y;
            bool first = true;
            RenderV(ref Item1, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item2, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item3, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item4, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item5, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item6, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item7, Gap, guide, mc, ref y, ref first);
            RenderV(ref Item8, Gap, guide, mc, ref y, ref first);
        }

        if (guide.IsAnimate && !guide.IsRender && !guide.IsArrange && !guide.IsMeasure)
        {
            AnimateV(ref Item1, guide);
            AnimateV(ref Item2, guide);
            AnimateV(ref Item3, guide);
            AnimateV(ref Item4, guide);
            AnimateV(ref Item5, guide);
            AnimateV(ref Item6, guide);
            AnimateV(ref Item7, guide);
            AnimateV(ref Item8, guide);
        }

        return guide;
    }

    // --- static helpers — one generic instantiation per concrete T, JIT-devirtualised ---

    private static void MeasureV<T>(ref T item, nfloat gap, LayoutGuide guide,
        ref nfloat maxW, ref nfloat totalH, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        if (!first) totalH += gap;
        first = false;
        var g = guide;
        g = item.Update(g);
        totalH += g.DesiredSize.Height;
        if (g.DesiredSize.Width > maxW) maxW = g.DesiredSize.Width;
    }

    private static void ArrangeV<T>(ref T item, nfloat gap, LayoutGuide guide,
        ref nfloat y, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        var rect = guide.ArrangedRect;
        if (!first) y += gap;
        first = false;
        var mg = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Measure,
            AvailableSize = rect.Size,
            MeasureContext = guide.MeasureContext,
        };
        mg = item.Update(mg);
        var g = guide;
        g.ArrangedRect = new Rect(rect.X, y, rect.Width, mg.DesiredSize.Height);
        item.Update(g);
        y += mg.DesiredSize.Height;
    }

    private static void RenderV<T>(ref T item, nfloat gap, LayoutGuide guide,
        IMeasureContext? mc, ref nfloat y, ref bool first)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        var rect = guide.ArrangedRect;
        if (!first) y += gap;
        first = false;
        var mg = new LayoutGuide
        {
            Pass = LayoutGuide.LayoutPass.Measure,
            AvailableSize = rect.Size,
            MeasureContext = mc,
        };
        mg = item.Update(mg);
        var g = guide;
        g.ArrangedRect = new Rect(rect.X, y, rect.Width, mg.DesiredSize.Height);
        item.Update(g);
        y += mg.DesiredSize.Height;
    }

    private static void AnimateV<T>(ref T item, LayoutGuide guide)
        where T : struct, ILayer
    {
        if (item.IsEmpty) return;
        item.Update(guide);
    }
}
