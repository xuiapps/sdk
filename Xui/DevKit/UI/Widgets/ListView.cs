using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A vertical list with selectable items, internally scrollable.
/// Each <see cref="ListViewItem"/> supports hover and selection states
/// using design system tokens.
/// </summary>
public class ListView : View
{
    private int selectedIndex = -1;
    private readonly ScrollView scrollView;
    private readonly ListViewContent content;

    private Color selectedFillColor;
    private Color selectedOutlineColor;
    private Color hoverFillColor;
    private Color outlineColor;
    private CornerRadius itemRadius;

    /// <summary>Gets or sets the selected item index (-1 for no selection).</summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set { selectedIndex = value; InvalidateRender(); }
    }

    /// <summary>Invoked when the selection changes.</summary>
    public Action<int>? SelectionChanged { get; set; }

    /// <summary>Which color group to use for selection. Default is Primary.</summary>
    public ColorRole Role { get; set; } = ColorRole.Primary;

    public ListView()
    {
        content = new ListViewContent(this);
        scrollView = new ScrollView { Content = content };
        this.AddProtectedChild(scrollView);
    }

    /// <summary>
    /// The items to display. Set via collection initializer.
    /// </summary>
    public ReadOnlySpan<View> Content
    {
        init
        {
            foreach (var view in value)
                content.Add(view);
        }
    }

    /// <inheritdoc/>
    public override int Count => 1;

    /// <inheritdoc/>
    public override View this[int index] => index == 0 ? scrollView : throw new IndexOutOfRangeException();

    internal void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;

        var group = Role switch
        {
            ColorRole.Secondary => ds.Colors.Secondary,
            ColorRole.Tertiary => ds.Colors.Tertiary,
            _ => ds.Colors.Primary,
        };

        selectedFillColor = group.Container;
        selectedOutlineColor = group.Background;
        hoverFillColor = ds.Colors.Surface.Container;
        outlineColor = ds.Colors.Outline;
        itemRadius = ds.Shape.Small;
    }

    internal Color SelectedFillColor => selectedFillColor;
    internal Color SelectedOutlineColor => selectedOutlineColor;
    internal Color HoverFillColor => hoverFillColor;
    internal Color OutlineColor => outlineColor;
    internal CornerRadius ItemRadius => itemRadius;

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        scrollView.Measure(available, context);
        return available;
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        scrollView.Arrange(rect, context);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        base.RenderCore(context);
    }

    internal void SelectItem(int index)
    {
        if (selectedIndex == index) return;
        selectedIndex = index;
        SelectionChanged?.Invoke(index);
        InvalidateRender();
    }

    internal int GetItemIndex(ListViewItem item)
    {
        for (int i = 0; i < content.Count; i++)
            if (content[i] == item) return i;
        return -1;
    }
}

/// <summary>
/// Internal vertical stack that hosts ListViewItems inside the ListView's ScrollView.
/// </summary>
internal class ListViewContent : ViewCollection
{
    private readonly ListView owner;

    public ListViewContent(ListView owner) => this.owner = owner;

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        nfloat totalH = 0;
        for (int i = 0; i < Count; i++)
        {
            var child = this[i];
            var size = child.Measure(new Size(available.Width, nfloat.PositiveInfinity), context);
            totalH += size.Height;
        }
        return new Size(available.Width, totalH);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        nfloat y = rect.Y;
        for (int i = 0; i < Count; i++)
        {
            var child = this[i];
            var desired = child.Measure(new Size(rect.Width, nfloat.PositiveInfinity), context);
            child.Arrange(new Rect(rect.X, y, rect.Width, desired.Height), context);
            y += desired.Height;
        }
    }
}

/// <summary>
/// A single selectable item inside a <see cref="ListView"/>.
/// Renders hover/selection backgrounds and hosts a content view.
/// </summary>
public class ListViewItem : View
{
    private View? content;
    private bool hover;
    private nfloat itemPadding;

    /// <summary>The content view displayed inside this item.</summary>
    public View? ItemContent
    {
        get => content;
        set => this.SetProtectedChild(ref content, value);
    }

    /// <inheritdoc/>
    public override int Count => content is null ? 0 : 1;

    /// <inheritdoc/>
    public override View this[int index] =>
        index == 0 && content is not null ? content : throw new IndexOutOfRangeException();

    private void ApplyDesignSystem()
    {
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null) return;
        itemPadding = ds.Spacing.Passive.S;
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        if (content == null)
            return new Size(available.Width, itemPadding * 2);

        var inner = new Size(available.Width - itemPadding * 2, nfloat.PositiveInfinity);
        var childSize = content.Measure(inner, context);
        return new Size(available.Width, childSize.Height + itemPadding * 2);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        if (content == null) return;
        content.Arrange(new Rect(
            this.Frame.X + itemPadding,
            this.Frame.Y + itemPadding,
            this.Frame.Width - itemPadding * 2,
            this.Frame.Height - itemPadding * 2
        ), context);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();

        var listView = FindListView();
        if (listView == null) { base.RenderCore(context); return; }

        var idx = listView.GetItemIndex(this);
        bool isSelected = idx == listView.SelectedIndex;

        var itemRect = new Rect(
            this.Frame.X + 2, this.Frame.Y + 1,
            this.Frame.Width - 4, this.Frame.Height - 2
        );
        var radius = listView.ItemRadius;

        if (isSelected)
        {
            context.BeginPath();
            context.RoundRect(itemRect, radius);
            context.SetFill(listView.SelectedFillColor);
            context.Fill(FillRule.NonZero);
            context.SetStroke(listView.SelectedOutlineColor);
            context.LineWidth = 1;
            context.Stroke();
        }
        else if (hover)
        {
            context.BeginPath();
            context.RoundRect(itemRect, radius);
            context.SetFill(listView.HoverFillColor);
            context.Fill(FillRule.NonZero);
            context.SetStroke(listView.OutlineColor);
            context.LineWidth = 1;
            context.Stroke();
        }

        base.RenderCore(context);
    }

    private ListView? FindListView()
    {
        View? v = this.Parent;
        while (v != null)
        {
            if (v is ListView lv) return lv;
            v = v.Parent;
        }
        return null;
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter)
        {
            hover = true;
            InvalidateRender();
        }
        else if (e.Type == PointerEventType.Leave)
        {
            hover = false;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            var listView = FindListView();
            if (listView != null)
            {
                var idx = listView.GetItemIndex(this);
                if (idx >= 0) listView.SelectItem(idx);
            }
        }

        base.OnPointerEvent(ref e, phase);
    }
}
