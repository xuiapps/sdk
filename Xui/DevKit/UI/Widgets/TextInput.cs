using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layer;
using Xui.DevKit.UI.Design;

namespace Xui.DevKit.UI.Widgets;

/// <summary>
/// A design-system-aware text input. Uses the same <see cref="FocusBorderLayer{TView,TChild}"/>
/// + <see cref="TextInputLayer"/> layer stack as <see cref="TextBox"/>, but fills all visual
/// properties from <see cref="IDesignSystem"/> tokens instead of exposing them individually.
/// </summary>
public class TextInput : LayerView<View, FocusBorderLayer<View, TextInputLayer>>
{
    /// <inheritdoc/>
    public override bool Focusable => true;

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
    protected override void OnActivate()
    {
        base.OnActivate();
        ApplyDesignSystem();
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
        var ds = this.GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (ds == null)
        {
            // Fallback defaults if no design system
            Layer.BackgroundColor = Colors.White;
            Layer.BorderColor = Colors.Gray;
            Layer.FocusedBorderColor = Colors.Blue;
            Layer.BorderThickness = 1;
            Layer.Padding = 3;
            Layer.Border.Child.FontFamily = ["Inter"];
            Layer.Border.Child.FontSize = 15;
            Layer.Border.Child.Color = Colors.Black;
            Layer.Border.Child.SelectedColor = Colors.White;
            Layer.Border.Child.SelectionBackgroundColor = Colors.Blue;
            return;
        }

        var textStyle = ds.Typography.Body.M;

        // Typography
        Layer.Border.Child.FontFamily = [textStyle.FontFamily];
        Layer.Border.Child.FontSize = textStyle.FontSize;
        Layer.Border.Child.FontWeight = textStyle.FontWeight;
        Layer.Border.Child.FontStyle = textStyle.FontStyle;
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
    }
}
