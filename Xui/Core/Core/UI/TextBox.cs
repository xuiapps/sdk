using System;
using Xui.Core.Canvas;
using Xui.Core.Math1D;
using Xui.Core.Math2D;
using Xui.Core.UI.Layer;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>
/// A single-line text input view implemented as
/// <c>LayerView&lt;View, FocusBorderLayer&lt;View, TextInputLayer&gt;&gt;</c>.
/// All text editing state, selection, caret blinking, and hit-testing live in
/// <see cref="TextInputLayer"/>; focus-aware border drawing lives in
/// <see cref="FocusBorderLayer{TView,TChild}"/>.
/// </summary>
public class TextBox : LayerView<View, FocusBorderLayer<View, TextInputLayer>>, ITextEditingClient
{
    /// <summary>Gets whether this view can receive keyboard focus. Always <c>true</c> for <see cref="TextBox"/>.</summary>
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

    /// <inheritdoc/>
    protected override void OnDeactivate()
    {
        Blur();
        base.OnDeactivate();
    }

    /// <summary>Initializes a new <see cref="TextBox"/> with default styling.</summary>
    public TextBox()
    {
        Editor.Changed += EditingChanged;
        Layer.BackgroundColor         = Colors.White;
        Layer.BorderThickness         = 1;
        Layer.BorderColor             = Colors.Gray;
        Layer.FocusedBorderColor      = Colors.Blue;
        Layer.Padding                 = 3;

        Layer.Border.Child.SelectAllOnFocus         = true;
        Layer.Border.Child.Color                    = Colors.Black;
        Layer.Border.Child.SelectedColor            = Colors.White;
        Layer.Border.Child.SelectionBackgroundColor = Colors.Blue;
        Layer.Border.Child.Font                     = new Font(15, "Inter");
    }

    /// <summary>Gets or sets the text content of this input.</summary>
    public string Text
    {
        get => Layer.Border.Child.Text;
        set => Layer.Border.Child.Text = value;
    }

    /// <summary>Gets or sets the current text selection range.</summary>
    public Interval<uint>.ClosedOpen Selection
    {
        get => Layer.Border.Child.Selection;
        set => Layer.Border.Child.Selection = value;
    }

    /// <summary>Gets or sets whether input is masked as a password.</summary>
    public bool IsPassword
    {
        get => Layer.Border.Child.IsPassword;
        set => Layer.Border.Child.IsPassword = value;
    }

    /// <summary>Gets or sets whether all text is selected when the view receives focus.</summary>
    public bool SelectAllOnFocus
    {
        get => Layer.Border.Child.SelectAllOnFocus;
        set => Layer.Border.Child.SelectAllOnFocus = value;
    }

    /// <summary>Gets or sets the character filter predicate for input validation.</summary>
    public Func<char, bool>? InputFilter
    {
        get => Layer.Border.Child.InputFilter;
        set => Layer.Border.Child.InputFilter = value;
    }

    /// <summary>Gets or sets the text color.</summary>
    public Color Color
    {
        get => Layer.Border.Child.Color;
        set => Layer.Border.Child.Color = value;
    }

    /// <summary>Gets or sets the color of selected text.</summary>
    public Color SelectedColor
    {
        get => Layer.Border.Child.SelectedColor;
        set => Layer.Border.Child.SelectedColor = value;
    }

    /// <summary>Gets or sets the background color of the selection.</summary>
    public Color SelectionBackgroundColor
    {
        get => Layer.Border.Child.SelectionBackgroundColor;
        set => Layer.Border.Child.SelectionBackgroundColor = value;
    }

    /// <summary>Gets or sets the complete text font.</summary>
    public Font Font
    {
        get => Layer.Border.Child.Font;
        set => Layer.Border.Child.Font = value;
    }

    /// <summary>Gets or sets the font family name.</summary>
    public string FontFamily
    {
        get => Font.FontFamily;
        set => Font = new Font(Font.FontSize, value, Font.FontWeight, Font.FontStyle, Font.FontStretch, Font.LineHeight);
    }

    /// <summary>Gets or sets the font size.</summary>
    public nfloat FontSize
    {
        get => Font.FontSize;
        set => Font = new Font(value, Font.FontFamily, Font.FontWeight, Font.FontStyle, Font.FontStretch, Font.LineHeight);
    }

    /// <summary>Gets or sets the font style.</summary>
    public FontStyle FontStyle
    {
        get => Font.FontStyle;
        set => Font = new Font(Font.FontSize, Font.FontFamily, Font.FontWeight, value, Font.FontStretch, Font.LineHeight);
    }

    /// <summary>Gets or sets the font weight.</summary>
    public FontWeight FontWeight
    {
        get => Font.FontWeight;
        set => Font = new Font(Font.FontSize, Font.FontFamily, value, Font.FontStyle, Font.FontStretch, Font.LineHeight);
    }

    /// <summary>Gets or sets the font stretch.</summary>
    public FontStretch FontStretch
    {
        get => Font.FontStretch;
        set => Font = new Font(Font.FontSize, Font.FontFamily, Font.FontWeight, Font.FontStyle, value, Font.LineHeight);
    }
}
