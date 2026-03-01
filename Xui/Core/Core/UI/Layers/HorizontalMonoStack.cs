// File: Xui/Core/UI/Layers/HorizontalMonoStack.cs
using System.Runtime.CompilerServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A horizontal stack layer that arranges children left-to-right, each at its natural (desired) width.
/// All children receive the full allocated height.
/// </summary>
/// <remarks>
/// <typeparamref name="TBuffer"/> must be a <see cref="LayerBuffer4{T}"/>, <see cref="LayerBuffer8{T}"/>,
/// <see cref="LayerBuffer16{T}"/>, or <see cref="LayerBuffer32{T}"/> of <typeparamref name="TChild"/>.
/// <para>
/// Children are re-measured inline during Arrange and Render passes so that stateful containers
/// (e.g. <see cref="DockLeft{TLeft,TRight}"/>) receive a fresh Measure before each Render.
/// </para>
/// </remarks>
public struct HorizontalMonoStack<TChild, TBuffer> : ILayer
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
            nfloat totalW = 0, maxH = 0;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                var g = guide;
                g = child.Update(g);
                if (i > 0) totalW += Gap;
                totalW += g.DesiredSize.Width;
                if (g.DesiredSize.Height > maxH) maxH = g.DesiredSize.Height;
            }
            guide.DesiredSize = new Size(totalW, maxH);
        }

        if (guide.IsArrange)
        {
            var rect = guide.ArrangedRect;
            nfloat x = rect.X;
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
                g.ArrangedRect = new Rect(x, rect.Y, mg.DesiredSize.Width, rect.Height);
                child.Update(g);
                x += mg.DesiredSize.Width + Gap;
            }
        }

        if (guide.IsRender)
        {
            var rect = guide.ArrangedRect;
            IMeasureContext? mc = guide.RenderContext;
            nfloat x = rect.X;
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
                g.ArrangedRect = new Rect(x, rect.Y, mg.DesiredSize.Width, rect.Height);
                child.Update(g);
                x += mg.DesiredSize.Width + Gap;
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
