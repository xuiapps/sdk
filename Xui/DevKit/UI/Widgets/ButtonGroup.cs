using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A horizontal group of buttons that share a continuous border.
/// Supports <see cref="ButtonVariant"/> (Filled, Outline, Text) and single selection.
/// </summary>
public class ButtonGroup : ViewCollection, IDesignSystemChangeNotifications
{
    private int selectedIndex;

    private Color fillColor => designSystem.Current?.Colors.Surface.Background ?? default;
    private Color selectedFillColor => group?.Container ?? default;
    private Color selectedFilledBgColor => group?.Background ?? default;
    private Color outlineColor => group?.Background ?? default;
    private Color hoverFillColor => designSystem.Current?.Colors.Surface.Container ?? default;
    private Color pressedFillColor => group?.SelectedPressedBackground ?? default;
    private CornerRadius cornerRadius => designSystem.Current?.Shape.Full ?? default;
    private readonly DesignSystemCache designSystem = new();

    /// <summary>Gets or sets the selected button index (0-based).</summary>
    public int SelectedIndex
    {
        get => selectedIndex;
        set { selectedIndex = value; InvalidateRender(); }
    }

    /// <summary>Invoked when the selection changes.</summary>
    public Action<int>? SelectionChanged { get; set; }

    /// <summary>Which color group to use. Default is Primary.</summary>
    public ColorRole Role { get; set; } = ColorRole.Primary;

    /// <summary>Visual style. Default is Outline.</summary>
    public ButtonVariant Variant { get; set; } = ButtonVariant.Outline;

    private ColorGroup? group => designSystem.Current is { } ds ? ResolveGroup(ds) : null;

    private ColorGroup ResolveGroup(IDesignSystem ds) => Role switch
    {
        ColorRole.Secondary => ds.Colors.Secondary,
        ColorRole.Tertiary => ds.Colors.Tertiary,
        _ => ds.Colors.Primary,
    };

    private void ApplyDesignSystem()
    {
        designSystem.Resolve(this, this);
    }

    /// <inheritdoc/>
    public void OnDesignTokenChange(DesignSystemChange changes)
    {
        if ((changes & (DesignSystemChange.Typography | DesignSystemChange.Spacing)) != 0)
            this.Invalidate();
        else if (changes != DesignSystemChange.None)
            this.InvalidateRender();
    }

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        designSystem.Deactivate(this);
        base.OnDeactivate();
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        nfloat totalW = 0;
        nfloat maxH = 0;

        for (int i = 0; i < Count; i++)
        {
            var child = this[i];
            var childSize = child.Measure(available, context);
            totalW += childSize.Width;
            maxH = nfloat.Max(maxH, childSize.Height);
        }

        return new Size(totalW, maxH);
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        nfloat x = rect.X;
        for (int i = 0; i < Count; i++)
        {
            var child = this[i];
            var desired = child.Measure(new Size(nfloat.PositiveInfinity, rect.Height), context);
            child.Arrange(new Rect(x, rect.Y, desired.Width, rect.Height), context, desired);
            x += desired.Width;
        }
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        if (Count == 0) return;

        var first = this[0];
        var last = this[Count - 1];
        var groupRect = new Rect(first.Frame.X, first.Frame.Y,
            last.Frame.X + last.Frame.Width - first.Frame.X,
            first.Frame.Height);

        // Group background
        var groupBg = Variant == ButtonVariant.Filled ? selectedFilledBgColor : fillColor;
        context.BeginPath();
        context.RoundRect(groupRect, cornerRadius);
        context.SetFill(groupBg);
        context.Fill(FillRule.NonZero);

        // Per-item highlight (selected / hovered / pressed)
        for (int i = 0; i < Count; i++)
        {
            var child = this[i];
            bool isSelected = i == selectedIndex;
            var bgi = child as ButtonGroupItem;
            bool isHovered = bgi != null && bgi.IsHovered;
            bool isPressed = bgi != null && bgi.IsPressed;

            if (!isSelected && !isHovered && !isPressed) continue;

            // Corner radius matches position: first=left, last=right, middle=square
            var cr = cornerRadius;
            var itemCorners = new CornerRadius(
                i == 0 ? cr.TopLeft : 0,
                i == Count - 1 ? cr.TopRight : 0,
                i == Count - 1 ? cr.BottomRight : 0,
                i == 0 ? cr.BottomLeft : 0
            );

            Color bg;
            if (isPressed && !isSelected)
                bg = pressedFillColor;
            else if (Variant == ButtonVariant.Filled)
                bg = isSelected ? selectedFillColor : (isHovered ? hoverFillColor : selectedFillColor);
            else if (Variant == ButtonVariant.Outline)
                bg = isSelected ? selectedFilledBgColor : (isPressed ? pressedFillColor : hoverFillColor);
            else
                bg = isSelected ? selectedFillColor : (isPressed ? pressedFillColor : hoverFillColor);

            context.BeginPath();
            context.RoundRect(child.Frame, itemCorners);
            context.SetFill(bg);
            context.Fill(FillRule.NonZero);
        }

        // Outer border (Outline gets it, Filled gets it, Text doesn't)
        if (Variant != ButtonVariant.Text)
        {
            context.BeginPath();
            context.RoundRect(groupRect, cornerRadius);
            context.SetStroke(outlineColor);
            context.LineWidth = 1;
            context.Stroke();
        }

        // Separator lines (skip for Filled since the bg is solid)
        if (Variant != ButtonVariant.Filled)
        {
            for (int i = 1; i < Count; i++)
            {
                var child = this[i];
                context.BeginPath();
                context.MoveTo(new Point(child.Frame.X, child.Frame.Y + 3));
                context.LineTo(new Point(child.Frame.X, child.Frame.Y + child.Frame.Height - 3));
                context.SetStroke(outlineColor);
                context.LineWidth = 1;
                context.Stroke();
            }
        }

        // Render child text
        base.RenderCore(context);
    }
}

/// <summary>
/// A single item inside a <see cref="ButtonGroup"/>. Renders only text — the group draws backgrounds.
/// </summary>
public class ButtonGroupItem : View, IDesignSystemChangeNotifications
{
    internal bool IsHovered;
    internal bool IsPressed;
    private bool pressed;
    private TextStyle textStyle => designSystem.Current?.Typography.Label.M ?? default;
    private nfloat paddingH => designSystem.Current?.Spacing.Active.M ?? 0;
    private nfloat paddingV => designSystem.Current?.Spacing.Active.S ?? 0;
    private string text = "";
    private readonly DesignSystemCache designSystem = new();

    /// <summary>The item label.</summary>
    public string Text
    {
        get => text;
        set
        {
            if (text == value) return;
            text = value;
            this.Invalidate();
        }
    }

    /// <inheritdoc/>
    public override int Count => 0;

    /// <inheritdoc/>
    public override View this[int index] => throw new IndexOutOfRangeException();

    private void ApplyDesignSystem()
    {
        designSystem.Resolve(this, this);
    }

    /// <inheritdoc/>
    public void OnDesignTokenChange(DesignSystemChange changes)
    {
        if ((changes & (DesignSystemChange.Typography | DesignSystemChange.Spacing)) != 0)
            this.Invalidate();
        else if (changes != DesignSystemChange.None)
            this.InvalidateRender();
    }

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        designSystem.Deactivate(this);
        base.OnDeactivate();
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        ApplyDesignSystem();
        context.SetFont(new Font(textStyle.FontSize, textStyle.FontFamily, textStyle.FontWeight, textStyle.FontStyle));
        var textSize = context.MeasureText(Text).Size;
        return new Size(textSize.Width + paddingH * 2, textSize.Height + paddingV * 2);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        var ds = designSystem.Current;
        if (ds == null) return;

        var parentGroup = this.Parent as ButtonGroup;
        var group = parentGroup?.Role switch
        {
            ColorRole.Secondary => ds.Colors.Secondary,
            ColorRole.Tertiary => ds.Colors.Tertiary,
            _ => ds.Colors.Primary,
        };

        bool isSelected = parentGroup != null && parentGroup.SelectedIndex == GetIndex();
        var variant = parentGroup?.Variant ?? ButtonVariant.Outline;

        Color textColor;
        if (variant == ButtonVariant.Filled)
            // Segmented bar: unselected = foreground on strong bg, selected = onContainer on lighter container
            textColor = isSelected ? group.OnContainer : group.Foreground;
        else if (variant == ButtonVariant.Outline && isSelected)
            // Outline: selected = foreground on strong bg
            textColor = group.Foreground;
        else
            // Default: group color for text
            textColor = isSelected ? group.OnContainer : group.Background;

        context.SetFont(new Font(textStyle.FontSize, textStyle.FontFamily, textStyle.FontWeight, textStyle.FontStyle));
        context.TextBaseline = TextBaseline.Top;
        context.SetFill(textColor);

        var textMeasure = context.MeasureText(Text);
        var tx = this.Frame.X + (this.Frame.Width - textMeasure.Size.Width) / 2;
        var ty = this.Frame.Y + (this.Frame.Height - textMeasure.Size.Height) / 2;
        context.FillText(Text, new Point(tx, ty));
    }

    private int GetIndex()
    {
        if (this.Parent is ButtonGroup group)
        {
            for (int i = 0; i < group.Count; i++)
                if (group[i] == this) return i;
        }
        return -1;
    }

    /// <inheritdoc/>
    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (e.Type == PointerEventType.Enter)
        {
            IsHovered = true;
            this.Parent?.InvalidateRender();
        }
        else if (e.Type == PointerEventType.Leave)
        {
            IsHovered = false;
            this.Parent?.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
        {
            CapturePointer(e.PointerId, PointerGestures.Tap);
            pressed = true;
            IsPressed = true;
            this.Parent?.InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Up)
        {
            ReleasePointer(e.PointerId);
            if (pressed && this.Parent is ButtonGroup group)
            {
                var idx = GetIndex();
                if (idx >= 0)
                {
                    group.SelectedIndex = idx;
                    group.SelectionChanged?.Invoke(idx);
                }
            }
            pressed = false;
            IsPressed = false;
            this.Parent?.InvalidateRender();
        }
        else if (e.Type == PointerEventType.LostCapture)
        {
            pressed = false;
            IsPressed = false;
            IsHovered = false;
            this.Parent?.InvalidateRender();
        }

        base.OnPointerEvent(ref e, phase);
    }
}
