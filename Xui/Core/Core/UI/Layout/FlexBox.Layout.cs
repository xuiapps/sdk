using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Core.UI.Layout;

public partial class FlexBox
{
    // Reuse layout storage while a container resizes or animates.
    private Item[] items = [];
    private Line[] lines = [];
    private int lineCount;
    private bool IsRow => FlexDirection is Direction.Row or Direction.RowReverse;
    private nfloat MainGap => IsRow ? ColumnGap : RowGap;
    private nfloat CrossGap => IsRow ? RowGap : ColumnGap;
    private nfloat Main(Size size) => IsRow ? size.Width : size.Height;
    private nfloat Cross(Size size) => IsRow ? size.Height : size.Width;
    private Size Axes(nfloat main, nfloat cross) => IsRow ? new(main, cross) : new(cross, main);

    private struct Item
    {
        public int Index;
        public nfloat Basis, Main, Cross, Min, Max, Grow, Shrink;
    }

    private struct Line
    {
        public int Start, Count;
        public nfloat Main, Cross;
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        BuildLines(available, context);
        nfloat main = 0, cross = 0;
        for (var i = 0; i < lineCount; i++)
        {
            main = nfloat.Max(main, lines[i].Main);
            cross += lines[i].Cross;
        }
        return Axes(main, cross + nfloat.Max(0, lineCount - 1) * CrossGap);
    }

    private void BuildLines(Size available, IMeasureContext context)
    {
        if (items.Length < Count)
        {
            var capacity = Math.Max(Count, Math.Max(8, items.Length * 2));
            items = new Item[capacity];
            lines = new Line[capacity];
        }
        var limit = Main(available);
        for (var i = 0; i < Count; i++)
        {
            var child = this[i];
            var desired = child.Measure(Axes(nfloat.PositiveInfinity, Cross(available)), context);
            var margin = IsRow ? child.Margin.TotalWidth : child.Margin.TotalHeight;
            var basis = child[Basis];
            var minimum = (IsRow ? child.MinimumWidth : child.MinimumHeight) + margin;
            var maximum = (IsRow ? child.MaximumWidth : child.MaximumHeight) + margin;
            items[i] = new Item
            {
                Index = i,
                Basis = nfloat.IsFinite(basis) ? nfloat.Max(0, basis) + margin : Main(desired),
                Cross = Cross(desired), Min = minimum, Max = maximum,
                Grow = nfloat.Max(0, child[Grow]), Shrink = nfloat.Max(0, child[Shrink]),
            };
            items[i].Main = nfloat.Clamp(items[i].Basis, minimum, maximum);
            // Stable order without temporary lists or comparer allocations.
            for (var j = i; j > 0 && this[items[j - 1].Index][Order] > child[Order]; j--)
                (items[j - 1], items[j]) = (items[j], items[j - 1]);
        }
        lineCount = 0;
        for (var i = 0; i < Count; i++)
        {
            if (lineCount == 0 || FlexWrap != Wrap.NoWrap && lines[lineCount - 1].Count > 0 &&
                lines[lineCount - 1].Main + MainGap + items[i].Main > limit)
                lines[lineCount++] = new Line { Start = i };
            ref var line = ref lines[lineCount - 1];
            line.Main += (line.Count > 0 ? MainGap : 0) + items[i].Main;
            line.Cross = nfloat.Max(line.Cross, items[i].Cross);
            line.Count++;
        }
        for (var i = 0; i < lineCount; i++)
        {
            ref var line = ref lines[i];
            if (nfloat.IsFinite(limit)) ResolveFlexibleSizes(ref line, limit);
            line.Cross = 0;
            for (var j = line.Start; j < line.Start + line.Count; j++)
            {
                ref var item = ref items[j];
                item.Cross = Cross(this[item.Index].Measure(Axes(item.Main, Cross(available)), context));
                line.Cross = nfloat.Max(line.Cross, item.Cross);
            }
        }
    }

    private void ResolveFlexibleSizes(ref Line line, nfloat limit)
    {
        var free = limit - line.Main;
        bool growing = free > 0;
        nfloat totalFactor = 0;
        for (var j = line.Start; j < line.Start + line.Count; j++)
            totalFactor += growing ? items[j].Grow : items[j].Shrink;
        free *= nfloat.Min(1, totalFactor);
        // Redistribute the remainder when an item reaches its min/max constraint.
        for (var pass = 0; pass <= line.Count && nfloat.Abs(free) > 0.001f; pass++)
        {
            nfloat weight = 0;
            for (var j = line.Start; j < line.Start + line.Count; j++)
            {
                ref var item = ref items[j];
                if (growing ? item.Main < item.Max : item.Main > item.Min)
                    weight += growing ? item.Grow : item.Shrink * item.Basis;
            }
            if (weight <= 0) break;
            nfloat used = 0;
            for (var j = line.Start; j < line.Start + line.Count; j++)
            {
                ref var item = ref items[j];
                if (growing ? item.Main >= item.Max : item.Main <= item.Min) continue;
                var factor = growing ? item.Grow : item.Shrink * item.Basis;
                var next = nfloat.Clamp(item.Main + free * factor / weight, item.Min, item.Max);
                used += next - item.Main;
                item.Main = next;
            }
            line.Main += used;
            free -= used;
            if (nfloat.Abs(used) < 0.001f) break;
        }
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        BuildLines(rect.Size, context);
        if (lineCount == 0) return;
        var main = Main(rect.Size);
        var cross = Cross(rect.Size);
        nfloat crossUsed = (lineCount - 1) * CrossGap;
        for (var i = 0; i < lineCount; i++) crossUsed += lines[i].Cross;
        var crossFree = nfloat.Max(0, cross - crossUsed);
        var crossGap = CrossGap;
        nfloat crossOffset = 0;
        if (FlexWrap == Wrap.NoWrap) lines[0].Cross = cross;
        else if (FlexAlignContent is AlignContent.Normal or AlignContent.Stretch)
            for (var i = 0; i < lineCount; i++) lines[i].Cross += crossFree / lineCount;
        else
            Distribute((int)FlexAlignContent, crossFree, lineCount, ref crossOffset, ref crossGap);

        for (var i = 0; i < lineCount; i++)
        {
            var line = lines[i];
            var mainGap = MainGap;
            nfloat mainOffset = 0;
            Distribute(FlexJustifyContent == JustifyContent.End ? 2 : (int)FlexJustifyContent,
                nfloat.Max(0, main - line.Main), line.Count, ref mainOffset, ref mainGap);
            for (var j = line.Start; j < line.Start + line.Count; j++)
            {
                var item = items[j];
                var itemCross = FlexAlignItems is AlignItems.Normal or AlignItems.Stretch ? line.Cross : item.Cross;
                var child = this[item.Index];
                var crossMargin = IsRow ? child.Margin.TotalHeight : child.Margin.TotalWidth;
                itemCross = nfloat.Clamp(itemCross,
                    (IsRow ? child.MinimumHeight : child.MinimumWidth) + crossMargin,
                    (IsRow ? child.MaximumHeight : child.MaximumWidth) + crossMargin);
                var localCross = FlexAlignItems switch
                {
                    AlignItems.Center => (line.Cross - itemCross) / 2,
                    AlignItems.FlexEnd => line.Cross - itemCross,
                    _ => (nfloat)0,
                };
                var mainPos = FlexDirection is Direction.RowReverse or Direction.ColumnReverse
                    ? main - mainOffset - item.Main : mainOffset;
                var crossPos = FlexWrap == Wrap.WrapReverse
                    ? cross - crossOffset - line.Cross + localCross : crossOffset + localCross;
                var size = Axes(item.Main, itemCross);
                var offset = Axes(mainPos, crossPos);
                this[item.Index].Arrange(new Rect(rect.X + offset.Width, rect.Y + offset.Height, size.Width, size.Height), context, size);
                mainOffset += item.Main + mainGap;
            }
            crossOffset += line.Cross + crossGap;
        }
    }

    // JustifyContent and AlignContent share the first six distribution values.
    private static void Distribute(int alignment, nfloat free, int count, ref nfloat offset, ref nfloat gap)
    {
        switch (alignment)
        {
            case 2: offset = free; break;
            case 3: offset = free / 2; break;
            case 4 when count > 1: gap += free / (count - 1); break;
            case 5: gap += free / count; offset = free / count / 2; break;
            case 6: gap += free / (count + 1); offset = free / (count + 1); break;
        }
    }
}
