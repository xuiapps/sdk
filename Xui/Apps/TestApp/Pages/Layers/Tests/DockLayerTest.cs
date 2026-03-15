using System.Runtime.InteropServices;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.Core.UI.Layer;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Layers.Tests;

/// <summary>
/// Demonstrates <see cref="DockLayer"/> composing <see cref="ButtonLayer{THost,TAction}"/> with
/// <see cref="TextInputLayer"/> for three common input patterns.
/// </summary>
public class DockLayerTest : View
{
    private readonly Label lblClearable, lblCombo, lblNumeric;
    private readonly ClearableInput clearable;
    private readonly ComboInput combo;
    private readonly NumericInput numeric;

    public override int Count => 6;
    public override View this[int index] => index switch
    {
        0 => lblClearable,
        1 => clearable,
        2 => lblCombo,
        3 => combo,
        4 => lblNumeric,
        5 => numeric,
        _ => throw new IndexOutOfRangeException(),
    };

    public DockLayerTest()
    {
        lblClearable = MakeLabel("Clearable input");
        clearable    = new ClearableInput();

        lblCombo = MakeLabel("Combo / dropdown");
        combo    = new ComboInput();

        lblNumeric = MakeLabel("Numeric stepper");
        numeric    = new NumericInput();

        AddProtectedChild(lblClearable);
        AddProtectedChild(clearable);
        AddProtectedChild(lblCombo);
        AddProtectedChild(combo);
        AddProtectedChild(lblNumeric);
        AddProtectedChild(numeric);
    }

    private static Label MakeLabel(string text) => new Label
    {
        Text       = text,
        FontFamily = ["Inter"],
        FontSize   = 12,
        FontWeight = FontWeight.Normal,
    };

    protected override Size MeasureCore(Size availableSize, IMeasureContext context)
    {
        NFloat w = availableSize.Width - 40;
        foreach (var v in new View[] { lblClearable, clearable, lblCombo, combo, lblNumeric, numeric })
            v.Measure(new Size(w, availableSize.Height), context);
        return availableSize;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        NFloat x   = rect.X + 20;
        NFloat w   = rect.Width - 40;
        NFloat y   = rect.Y + 20;
        NFloat gap = 10;

        void Place(View v, NFloat h) { v.Arrange(new Rect(x, y, w, h), context); y += h + gap; }

        Place(lblClearable, 16);
        Place(clearable,    clearable.Frame.Height > 0 ? clearable.Frame.Height : 28);
        y += 8;
        Place(lblCombo, 16);
        Place(combo,    combo.Frame.Height > 0 ? combo.Frame.Height : 28);
        y += 8;
        Place(lblNumeric, 16);
        Place(numeric,    numeric.Frame.Height > 0 ? numeric.Frame.Height : 28);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(new Color(0xF8, 0xF9, 0xFA, 0xFF));
        context.FillRect(Frame);
        base.RenderCore(context);
    }

    // ── Widget 1: text input with a clear "×" button that appears when text is non-empty ──

    private class ClearableInput
        : LayerView<ClearableInput, FocusBorderLayer<ClearableInput, DockLayer.Dock2<ClearableInput, TextInputLayer, ButtonLayer<ClearableInput, ClearableInput.ClearAction>>>>
    {
        public override bool Focusable => true;

        public ClearableInput()
        {
            // Outer border
            Layer.BackgroundColor    = White;
            Layer.BorderThickness    = 1;
            Layer.BorderColor        = new Color(0xCC, 0xCC, 0xCC, 0xFF);
            Layer.FocusedBorderColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            Layer.CornerRadius       = new CornerRadius(5);
            Layer.Padding            = 0;

            // Dock alignment
            Layer.Border.Child.Child1.Align = DockLayer.Align.Stretch;
            Layer.Border.Child.Child2.Align = DockLayer.Align.Right;

            // Text layer
            ref var inp = ref Layer.Border.Child.Child1.Child;
            inp.Color                    = Black;
            inp.SelectedColor            = White;
            inp.SelectionBackgroundColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            inp.Padding                  = 3;
            inp.FontFamily               = ["Inter"];
            inp.FontSize                 = 15;
            inp.FontWeight               = FontWeight.Normal;
            inp.FontStretch              = FontStretch.Normal;
            inp.FontStyle                = FontStyle.Normal;
            inp.SelectAllOnFocus         = true;

            // Clear button (hidden until text is present)
            Layer.Border.Child.Child2.Child = new ButtonLayer<ClearableInput, ClearAction>
            {
                Label        = "×",
                Margin       = 2,
                CornerRadius = new CornerRadius(5),
                NormalColor  = new Color(0xE0, 0xE0, 0xE0, 0xFF),
                HoverColor   = new Color(0xC8, 0xC8, 0xC8, 0xFF),
                PressedColor = new Color(0xA8, 0xA8, 0xA8, 0xFF),
                LabelColor   = new Color(0x44, 0x44, 0x44, 0xFF),
                FontSize     = 14,
                Visible      = false,
            };
        }

        internal void Clear()
        {
            Layer.Border.Child.Child1.Child.Text    = "";
            Layer.Border.Child.Child2.Child.Visible = false;
            InvalidateMeasure();
        }

        public override void OnChar(ref KeyEventRef e)
        {
            base.OnChar(ref e);
            SyncClearButton();
        }

        public override void OnKeyDown(ref KeyEventRef e)
        {
            base.OnKeyDown(ref e);
            SyncClearButton();
        }

        private void SyncClearButton()
        {
            bool hasText    = Layer.Border.Child.Child1.Child.Text.Length > 0;
            bool wasVisible = Layer.Border.Child.Child2.Child.Visible;
            if (hasText != wasVisible)
            {
                Layer.Border.Child.Child2.Child.Visible = hasText;
                InvalidateMeasure();
            }
        }

        internal struct ClearAction : IButtonAction<ClearableInput>
        {
            public void Execute(ClearableInput host) => host.Clear();
        }
    }

    // ── Widget 2: combobox-style with a right-aligned dropdown arrow ──

    private class ComboInput
        : LayerView<ComboInput, FocusBorderLayer<ComboInput, DockLayer.Dock2<ComboInput, TextInputLayer, ButtonLayer<ComboInput, ComboInput.DropdownAction>>>>
    {
        public override bool Focusable => true;
        private IPopup? activePopup;

        public ComboInput()
        {
            // Border — no right padding so the arrow button is flush to the border edge
            Layer.BackgroundColor    = White;
            Layer.BorderThickness    = 1;
            Layer.BorderColor        = new Color(0xCC, 0xCC, 0xCC, 0xFF);
            Layer.FocusedBorderColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            Layer.CornerRadius       = new CornerRadius(5);
            Layer.Padding            = 0;

            // Dock alignment
            Layer.Border.Child.Child1.Align = DockLayer.Align.Stretch;
            Layer.Border.Child.Child2.Align = DockLayer.Align.Right;

            // Text layer
            ref var inp = ref Layer.Border.Child.Child1.Child;
            inp.Color                    = Black;
            inp.SelectedColor            = White;
            inp.SelectionBackgroundColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            inp.Padding                  = 3;
            inp.FontFamily               = ["Inter"];
            inp.FontSize                 = 15;
            inp.FontWeight               = FontWeight.Normal;
            inp.FontStretch              = FontStretch.Normal;
            inp.FontStyle                = FontStyle.Normal;
            inp.SelectAllOnFocus         = true;

            // Dropdown arrow button — no margin, right corners rounded to match border
            Layer.Border.Child.Child2.Child = new ButtonLayer<ComboInput, DropdownAction>
            {
                Label        = "▾",
                Margin       = 0,
                CornerRadius = new CornerRadius(0, 4, 4, 0),
                NormalColor  = new Color(0xF0, 0xF0, 0xF0, 0xFF),
                HoverColor   = new Color(0xD8, 0xD8, 0xD8, 0xFF),
                PressedColor = new Color(0xC0, 0xC0, 0xC0, 0xFF),
                LabelColor   = new Color(0x44, 0x44, 0x44, 0xFF),
                FontSize     = 12,
                Visible      = true,
            };
        }

        internal void OpenDropdown()
        {
            if (activePopup is { IsVisible: true })
            {
                activePopup.Close();
                return;
            }

            activePopup = this.GetService<IPopup>();
            if (activePopup == null) return;

            var content = new DropdownContent();
            activePopup.Show(content, this.Frame, PopupPlacement.Below,
                new Size(this.Frame.Width, 80), PopupEffect.Translucent);
        }

        internal struct DropdownAction : IButtonAction<ComboInput>
        {
            public void Execute(ComboInput host) => host.OpenDropdown();
        }
    }

    /// <summary>
    /// Simple dropdown content view with a few label items.
    /// </summary>
    private class DropdownContent : View
    {
        private readonly Label item1, item2, item3;

        public override int Count => 3;
        public override View this[int index] => index switch
        {
            0 => item1,
            1 => item2,
            2 => item3,
            _ => throw new IndexOutOfRangeException(),
        };

        public DropdownContent()
        {
            item1 = MakeItem("Option A");
            item2 = MakeItem("Option B");
            item3 = MakeItem("Option C");
            AddProtectedChild(item1);
            AddProtectedChild(item2);
            AddProtectedChild(item3);
        }

        private static Label MakeItem(string text) => new Label
        {
            Text       = text,
            FontFamily = ["Inter"],
            FontSize   = 14,
            FontWeight = FontWeight.Normal,
        };

        protected override Size MeasureCore(Size availableSize, IMeasureContext context)
        {
            NFloat h = 0;
            for (int i = 0; i < Count; i++)
            {
                this[i].Measure(new Size(availableSize.Width, availableSize.Height), context);
                h += 24;
            }
            return new Size(availableSize.Width, h + 8);
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            NFloat y = rect.Y + 4;
            for (int i = 0; i < Count; i++)
            {
                this[i].Arrange(new Rect(rect.X + 8, y, rect.Width - 16, 20), context);
                y += 24;
            }
        }

        protected override void RenderCore(IContext context)
        {
            base.RenderCore(context);
        }
    }

    // ── Widget 3: numeric stepper — "−" | text | "+" ──

    private class NumericInput
        : LayerView<NumericInput, FocusBorderLayer<NumericInput, DockLayer.Dock3<NumericInput, ButtonLayer<NumericInput, NumericInput.DecrementAction>, TextInputLayer, ButtonLayer<NumericInput, NumericInput.IncrementAction>>>>
    {
        public override bool Focusable => true;

        public NumericInput()
        {
            // Outer border
            Layer.BackgroundColor    = White;
            Layer.BorderThickness    = 1;
            Layer.BorderColor        = new Color(0xCC, 0xCC, 0xCC, 0xFF);
            Layer.FocusedBorderColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            Layer.CornerRadius       = new CornerRadius(5);
            Layer.Padding            = 0;

            // Dock alignment: Left button | Stretch text | Right button
            Layer.Border.Child.Child1.Align = DockLayer.Align.Left;
            Layer.Border.Child.Child2.Align = DockLayer.Align.Stretch;
            Layer.Border.Child.Child3.Align = DockLayer.Align.Right;

            // "−" button (left)
            Layer.Border.Child.Child1.Child = new ButtonLayer<NumericInput, DecrementAction>
            {
                Label        = "−",
                Margin       = 0,
                CornerRadius = new CornerRadius(4, 0, 0, 4),
                NormalColor  = new Color(0xF0, 0xF0, 0xF0, 0xFF),
                HoverColor   = new Color(0xD8, 0xD8, 0xD8, 0xFF),
                PressedColor = new Color(0xC0, 0xC0, 0xC0, 0xFF),
                LabelColor   = new Color(0x22, 0x22, 0x22, 0xFF),
                FontSize     = 16,
                Visible      = true,
            };

            // Text layer (center, stretch)
            ref var inp = ref Layer.Border.Child.Child2.Child;
            inp.Color                    = Black;
            inp.SelectedColor            = White;
            inp.SelectionBackgroundColor = new Color(0x00, 0x78, 0xD4, 0xFF);
            inp.Padding                  = 3;
            inp.FontFamily               = ["Inter"];
            inp.FontSize                 = 15;
            inp.FontWeight               = FontWeight.Normal;
            inp.FontStretch              = FontStretch.Normal;
            inp.FontStyle                = FontStyle.Normal;
            inp.SelectAllOnFocus         = true;
            inp.Text                     = "0";

            // "+" button (right)
            Layer.Border.Child.Child3.Child = new ButtonLayer<NumericInput, IncrementAction>
            {
                Label        = "+",
                Margin       = 0,
                CornerRadius = new CornerRadius(0, 4, 4, 0),
                NormalColor  = new Color(0xF0, 0xF0, 0xF0, 0xFF),
                HoverColor   = new Color(0xD8, 0xD8, 0xD8, 0xFF),
                PressedColor = new Color(0xC0, 0xC0, 0xC0, 0xFF),
                LabelColor   = new Color(0x22, 0x22, 0x22, 0xFF),
                FontSize     = 16,
                Visible      = true,
            };
        }

        internal void Step(int delta)
        {
            ref var inp = ref Layer.Border.Child.Child2.Child;
            if (int.TryParse(inp.Text, out int v))
                inp.Text = (v + delta).ToString();
            InvalidateRender();
        }

        internal struct DecrementAction : IButtonAction<NumericInput>
        {
            public void Execute(NumericInput host) => host.Step(-1);
        }

        internal struct IncrementAction : IButtonAction<NumericInput>
        {
            public void Execute(NumericInput host) => host.Step(+1);
        }
    }
}
