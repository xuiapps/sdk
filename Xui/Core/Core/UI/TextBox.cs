using System;
using Xui.Core.Canvas;
using Xui.Core.Math1D;
using Xui.Core.Math2D;
using Xui.Core.UI.Layer;

namespace Xui.Core.UI;

/// <summary>
/// A single-line text input view implemented as
/// <c>LayerView&lt;View, FocusBorderLayer&lt;View, TextInputLayer&gt;&gt;</c>.
/// All text editing state, selection, caret blinking, and hit-testing live in
/// <see cref="TextInputLayer"/>; focus-aware border drawing lives in
/// <see cref="FocusBorderLayer{TView,TChild}"/>.
/// </summary>
public class TextBox : LayerView<View, FocusBorderLayer<View, TextInputLayer>>
{
    public override bool Focusable => true;

    public TextBox()
    {
        Layer.BackgroundColor         = Colors.White;
        Layer.BorderThickness         = 1;
        Layer.BorderColor             = Colors.Gray;
        Layer.FocusedBorderColor      = Colors.Blue;
        Layer.Padding                 = 3;

        Layer.Border.Child.SelectAllOnFocus         = true;
        Layer.Border.Child.Color                    = Colors.Black;
        Layer.Border.Child.SelectedColor            = Colors.White;
        Layer.Border.Child.SelectionBackgroundColor = Colors.Blue;
        Layer.Border.Child.FontFamily               = ["Inter"];
        Layer.Border.Child.FontSize                 = 15;
        Layer.Border.Child.FontWeight               = FontWeight.Normal;
    }

    public string Text
    {
        get => Layer.Border.Child.Text;
        set => Layer.Border.Child.Text = value;
    }

    public Interval<uint>.ClosedOpen Selection
    {
        get => Layer.Border.Child.Selection;
        set => Layer.Border.Child.Selection = value;
    }

    public bool IsPassword
    {
        get => Layer.Border.Child.IsPassword;
        set => Layer.Border.Child.IsPassword = value;
    }

    public bool SelectAllOnFocus
    {
        get => Layer.Border.Child.SelectAllOnFocus;
        set => Layer.Border.Child.SelectAllOnFocus = value;
    }

    public Func<char, bool>? InputFilter
    {
        get => Layer.Border.Child.InputFilter;
        set => Layer.Border.Child.InputFilter = value;
    }

    public Color Color
    {
        get => Layer.Border.Child.Color;
        set => Layer.Border.Child.Color = value;
    }

    public Color SelectedColor
    {
        get => Layer.Border.Child.SelectedColor;
        set => Layer.Border.Child.SelectedColor = value;
    }

    public Color SelectionBackgroundColor
    {
        get => Layer.Border.Child.SelectionBackgroundColor;
        set => Layer.Border.Child.SelectionBackgroundColor = value;
    }

    public string[] FontFamily
    {
        get => Layer.Border.Child.FontFamily ?? ["Inter"];
        set => Layer.Border.Child.FontFamily = value;
    }

    public nfloat FontSize
    {
        get => Layer.Border.Child.FontSize;
        set => Layer.Border.Child.FontSize = value;
    }

    public FontStyle FontStyle
    {
        get => Layer.Border.Child.FontStyle;
        set => Layer.Border.Child.FontStyle = value;
    }

    public FontWeight FontWeight
    {
        get => Layer.Border.Child.FontWeight;
        set => Layer.Border.Child.FontWeight = value;
    }

    public FontStretch FontStretch
    {
        get => Layer.Border.Child.FontStretch;
        set => Layer.Border.Child.FontStretch = value;
    }
}
