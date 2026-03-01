// File: Xui/Core/UI/Layers/UniformMonoGrid.cs
using System.Runtime.CompilerServices;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.UI.Layers;

/// <summary>
/// A uniform grid layer that arranges children in a <see cref="Rows"/> × <see cref="Columns"/> grid,
/// giving every cell the same size.
/// </summary>
/// <remarks>
/// <para>
/// <typeparamref name="TBuffer"/> must be a <see cref="LayerBuffer4{T}"/>, <see cref="LayerBuffer8{T}"/>,
/// <see cref="LayerBuffer16{T}"/>, or <see cref="LayerBuffer32{T}"/> of <typeparamref name="TChild"/>.
/// Cells are ordered left-to-right, top-to-bottom. Set <see cref="Count"/> to skip trailing cells.
/// </para>
/// <para>
/// <see cref="AspectRatio"/> controls the cell shape. When ≤ 0 or <c>NaN</c> cells fill the available
/// space freely. When positive, <c>cellWidth / cellHeight = AspectRatio</c> and the grid is centered
/// within the available area (e.g. 1 = square cells, 4/3 ≈ 1.33 = slightly wider than tall).
/// </para>
/// </remarks>
public struct UniformMonoGrid<TChild, TBuffer> : ILayer
    where TChild : struct, ILayer
    where TBuffer : struct
{
    public TBuffer Children;
    public int Count;
    public int Columns;
    public int Rows;

    /// <summary>Pixel gap between cells (both horizontal and vertical).</summary>
    public nfloat Gap;

    /// <summary>
    /// Desired cell aspect ratio (width / height). ≤ 0 or NaN = fill available space freely.
    /// 1 = square, 4f/3f = slightly wider than tall, etc.
    /// </summary>
    public nfloat AspectRatio;

    // Computed during Measure; used during Render and hit-testing.
    public nfloat CellWidth;
    public nfloat CellHeight;

    public LayoutGuide Update(LayoutGuide guide)
    {
        ref TChild first = ref Unsafe.As<TBuffer, TChild>(ref Children);

        if (guide.IsMeasure)
        {
            var avail = guide.AvailableSize;
            int cols = Columns > 0 ? Columns : 1;
            int rows = Rows > 0 ? Rows : 1;
            nfloat ar = AspectRatio;

            if (ar <= 0 || nfloat.IsNaN(ar))
            {
                // Free sizing: cells fill all available space.
                CellWidth  = (avail.Width  - Gap * (cols - 1)) / cols;
                CellHeight = (avail.Height - Gap * (rows - 1)) / rows;
            }
            else
            {
                // Aspect-ratio constrained: fit the largest cells with cellW/cellH = ar.
                nfloat maxByWidth  = (avail.Width  - Gap * (cols - 1)) / cols;
                nfloat maxByHeight = (avail.Height - Gap * (rows - 1)) / rows;

                // cellW = ar * cellH, choose the tighter constraint.
                CellHeight = nfloat.Min(maxByWidth / ar, maxByHeight);
                CellWidth  = CellHeight * ar;
            }

            guide.DesiredSize = new Size(
                CellWidth  * cols + Gap * (cols - 1),
                CellHeight * rows + Gap * (rows - 1));
        }

        if (guide.IsArrange)
        {
            var rect = guide.ArrangedRect;
            int cols = Columns > 0 ? Columns : 1;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                int col = i % cols;
                int row = i / cols;
                var g = guide;
                g.ArrangedRect = new Rect(
                    rect.X + col * (CellWidth  + Gap),
                    rect.Y + row * (CellHeight + Gap),
                    CellWidth, CellHeight);
                child.Update(g);
            }
        }

        if (guide.IsRender)
        {
            var rect = guide.ArrangedRect;
            int cols = Columns > 0 ? Columns : 1;
            for (int i = 0; i < Count; i++)
            {
                ref TChild child = ref Unsafe.Add(ref first, i);
                int col = i % cols;
                int row = i / cols;
                var g = guide;
                g.ArrangedRect = new Rect(
                    rect.X + col * (CellWidth  + Gap),
                    rect.Y + row * (CellHeight + Gap),
                    CellWidth, CellHeight);
                child.Update(g);
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
