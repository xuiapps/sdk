// File: Xui/Core/UI/Layers/VerticalMonoStack.cs
using System.Runtime.CompilerServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A vertical stack layer that arranges children top-to-bottom, each at its natural (desired) height.
/// All children receive the full allocated width.
/// </summary>
/// <remarks>
/// <typeparamref name="TBuffer"/> must be a <see cref="LayerBuffer4{T}"/>, <see cref="LayerBuffer8{T}"/>,
/// <see cref="LayerBuffer16{T}"/>, or <see cref="LayerBuffer32{T}"/> of <typeparamref name="TChild"/>.
/// </remarks>
public struct VerticalMonoStack<TChild, TBuffer> : ILayer
    where TChild : struct, ILayer
    where TBuffer : struct
{
    public TBuffer Children;
    public int Count;
    public nfloat Gap;

    public LayoutGuide Update(LayoutGuide guide)
    {
        ref TChild first = ref Unsafe.As<TBuffer, TChild>(ref Children);

        if (guide.IsMeasure)
        {
            nfloat maxW = 0, totalH = 0;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                var g = guide;
                g = child.Update(g);
                if (i > 0) totalH += Gap;
                totalH += g.DesiredSize.Height;
                if (g.DesiredSize.Width > maxW) maxW = g.DesiredSize.Width;
            }
            guide.DesiredSize = new Size(maxW, totalH);
        }

        if (guide.IsArrange)
        {
            var rect = guide.ArrangedRect;
            nfloat y = rect.Y;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                var mg = new LayoutGuide
                {
                    Pass = LayoutGuide.LayoutPass.Measure,
                    AvailableSize = rect.Size,
                    MeasureContext = guide.MeasureContext,
                };
                mg = child.Update(mg);

                var g = guide;
                g.ArrangedRect = new Rect(rect.X, y, rect.Width, mg.DesiredSize.Height);
                child.Update(g);
                y += mg.DesiredSize.Height + Gap;
            }
        }

        if (guide.IsRender)
        {
            var rect = guide.ArrangedRect;
            IMeasureContext? mc = guide.RenderContext;
            nfloat y = rect.Y;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                var mg = new LayoutGuide
                {
                    Pass = LayoutGuide.LayoutPass.Measure,
                    AvailableSize = rect.Size,
                    MeasureContext = mc,
                };
                mg = child.Update(mg);

                var g = guide;
                g.ArrangedRect = new Rect(rect.X, y, rect.Width, mg.DesiredSize.Height);
                child.Update(g);
                y += mg.DesiredSize.Height + Gap;
            }
        }

        if (guide.IsAnimate && !guide.IsRender && !guide.IsArrange && !guide.IsMeasure)
        {
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                child.Update(guide);
            }
        }

        return guide;
    }
}
