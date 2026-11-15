using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layer;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A design-system-aware text input. Uses the same <see cref="FocusBorderLayer{TView,TChild}"/>
/// + <see cref="TextInputLayer"/> layer stack as <see cref="TextBox"/>, but fills all visual
/// properties from <see cref="IDesignSystem"/> tokens instead of exposing them individually.
/// </summary>
public class TextInput : LayerView<View, FocusBorderLayer<View, TextInputLayer>>, IDesignSystemChangeNotifications, ITextEditingClient
{
    /// <summary>Creates a text input with shared native editing state.</summary>
    public TextInput() { Editor.Changed += EditingChanged; }

    private readonly DesignSystemCache designSystem = new();
    private bool appliedFallback;
    /// <inheritdoc/>
    public override bool Focusable => true;

    /// <summary>Keyboard hints, applied when this field receives focus.</summary>
    public TextInputOptions InputOptions { get; set; }

    /// <inheritdoc/>
    public bool CanPerformInputAction(TextInputAction action) => TextInputNavigation.CanPerform(this, action);

    /// <inheritdoc/>
    public void PerformInputAction(TextInputAction action) => TextInputNavigation.Perform(this, action);

    /// <inheritdoc/>
    public Xui.Core.UI.Input.TextEditingState Editor => Layer.Border.Child.Editor;

    /// <inheritdoc/>
    public bool NativeSelection
    {
        get => Layer.Border.Child.NativeSelection;
        set { Layer.Border.Child.NativeSelection = value; InvalidateRender(); }
    }

    /// <inheritdoc/>
    public Rect InputBounds
    {
        get
        {
            var rect = Frame;
            for (View? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
                rect = ancestor.InverseTransformRect(rect);
            return rect;
        }
    }

    /// <inheritdoc/>
    public Rect GetCaretRect(int offset, ITextMeasureContext measure) => Layer.Border.Child.GetCaretRect(this, offset, measure);

    /// <inheritdoc/>
    public int GetTextPosition(Point point, ITextMeasureContext measure) => Layer.Border.Child.GetTextPosition(this, point, measure);

    private void EditingChanged(Xui.Core.UI.Input.TextEditingChange change)
    {
        if ((change & Xui.Core.UI.Input.TextEditingChange.Text) != 0) InvalidateMeasure();
        InvalidateRender();
    }

    /// <inheritdoc/>
    public bool HasText => Layer.Border.Child.HasText;

    /// <summary>Gets or sets the text content.</summary>
    public string Text
    {
        get => Layer.Border.Child.Text;
        set => Layer.Border.Child.Text = value;
    }

    /// <summary>Gets or sets whether input is masked as a password.</summary>
    public bool IsPassword
    {
        get => Layer.Border.Child.IsPassword;
        set => Layer.Border.Child.IsPassword = value;
    }

    /// <inheritdoc/>
    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        ApplyDesignSystem();
        return base.MeasureCore(availableSize, context);
    }

    /// <inheritdoc/>
    protected override void RenderCore(IContext context)
    {
        ApplyDesignSystem();
        base.RenderCore(context);
    }

    private void ApplyDesignSystem()
    {
        designSystem.Resolve(this, this);
        if (designSystem.Current is null)
        {
            if (appliedFallback)
                return;

            appliedFallback = true;
            // Fallback defaults if no design system
            Layer.BackgroundColor = Colors.White;
            Layer.BorderColor = Colors.Gray;
            Layer.FocusedBorderColor = Colors.Blue;
            Layer.BorderThickness = 1;
            Layer.Padding = 3;
            Layer.Border.Child.Font = new Font(15, "Inter");
            Layer.Border.Child.Color = Colors.Black;
            Layer.Border.Child.SelectedColor = Colors.White;
            Layer.Border.Child.SelectionBackgroundColor = Colors.Blue;
            return;
        }

        appliedFallback = false;

    }

    /// <inheritdoc/>
    public void OnDesignTokenChange()
    {
        var ds = designSystem.Current;
        if (ds is null) return;

        var textStyle = ds.Typography.Body.M;

        // Typography
        Layer.Border.Child.Font = new Font(textStyle.FontSize, textStyle.FontFamily, textStyle.FontWeight, textStyle.FontStyle);
        Layer.Border.Child.SelectAllOnFocus = true;

        // Colors
        Layer.Border.Child.Color = ds.Colors.Surface.Foreground;
        Layer.Border.Child.SelectedColor = ds.Colors.Primary.Foreground;
        Layer.Border.Child.SelectionBackgroundColor = ds.Colors.Primary.Background;

        // Border & background
        Layer.BackgroundColor = ds.Colors.Surface.Background;
        Layer.BorderColor = ds.Colors.Outline;
        Layer.FocusedBorderColor = ds.Colors.Primary.Background;
        Layer.BorderThickness = 1;
        Layer.CornerRadius = ds.Shape.Small;

        // Spacing — active scale for interactive element
        Layer.Padding = ds.Spacing.Active.S;
        this.Invalidate();
    }

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        Blur();
        designSystem.Deactivate(this);
        base.OnDeactivate();
    }
}
