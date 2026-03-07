using System.Text;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math1D;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A leaf layer that implements single-line text input: typing, selection, caret blinking,
/// password masking, and mouse-driven selection. Carries all text state as struct fields.
/// <para>
/// The layer stores the arranged <see cref="contentRect"/> during <see cref="Arrange"/>
/// so that <see cref="Render"/> and pointer hit-testing both know the exact text area,
/// independent of any border or padding applied by a wrapping layer.
/// </para>
/// </summary>
public struct TextInputLayer : ILayer<View>
{
    private static readonly TimeSpan CaretBlinkInterval = TimeSpan.FromMilliseconds(530);

    // ── Text state ───────────────────────────────────────────────────────

    private StringBuilder? textBuffer;
    private StringBuilder TextBuffer => textBuffer ??= new StringBuilder();

    private Interval<uint>.ClosedOpen selection;
    private bool caretVisible;
    private TimeSpan caretToggleTime;
    private uint anchor;
    private bool isMouseSelecting;

    // ── Layout state ─────────────────────────────────────────────────────

    /// <summary>The content rect last assigned by <see cref="Arrange"/>. Used for text hit-testing.</summary>
    private Rect contentRect;

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>Gets or sets the text content.</summary>
    public string Text
    {
        get => textBuffer?.ToString() ?? string.Empty;
        set
        {
            TextBuffer.Clear();
            TextBuffer.Append(value);
            selection = new Interval<uint>.ClosedOpen((uint)value.Length, (uint)value.Length);
        }
    }

    /// <summary>
    /// Gets or sets the selection as a half-open interval [Start, End).
    /// Start == End represents the caret position.
    /// </summary>
    public Interval<uint>.ClosedOpen Selection
    {
        get => selection;
        set => selection = value;
    }

    /// <summary>When true, displays bullet characters instead of the actual text.</summary>
    public bool IsPassword { get; set; }

    /// <summary>When true (default), selects all text when the view gains focus.</summary>
    public bool SelectAllOnFocus { get; set; }

    /// <summary>Optional character filter. Return true to accept, false to reject.</summary>
    public Func<char, bool>? InputFilter { get; set; }

    // ── Visual properties ────────────────────────────────────────────────

    /// <summary>Normal text color.</summary>
    public Color Color { get; set; }

    /// <summary>Color of text inside the selection highlight.</summary>
    public Color SelectedColor { get; set; }

    /// <summary>Background color drawn behind selected text.</summary>
    public Color SelectionBackgroundColor { get; set; }

    /// <summary>Corner radius of the selection highlight rectangle. Zero by default.</summary>
    public CornerRadius SelectionCornerRadius { get; set; }

    /// <summary>Inset applied to the arranged rect before text layout and hit-testing.</summary>
    public Frame Padding { get; set; }

    // ── Font properties ──────────────────────────────────────────────────

    public string[]? FontFamily { get; set; }
    public nfloat FontSize { get; set; }
    public FontStyle FontStyle { get; set; }
    public FontWeight FontWeight { get; set; }
    public FontStretch FontStretch { get; set; }

    // ── ILayer<View> ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Update(View view, ref LayoutGuide guide)
    {
        if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
        if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
        if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
        if (guide.IsRender)  Render(view, guide.RenderContext!);
    }

    /// <inheritdoc/>
    public Size Measure(View view, Size availableSize, IMeasureContext context)
    {
        context.SetFont(GetFont());
        var displayText = GetDisplayText();
        var textMetrics = context.MeasureText(displayText.Length > 0 ? displayText : "X");
        var width = nfloat.Max(availableSize.Width, 100);
        return new Size(width, textMetrics.Size.Height + Padding.TotalHeight);
    }

    /// <inheritdoc/>
    public void Arrange(View view, Rect rect, IMeasureContext context)
    {
        contentRect = rect - Padding;
    }

    /// <inheritdoc/>
    public void Render(View view, IContext context)
    {
        var focused = view.IsFocused;
        var frame = contentRect;

        context.SetFont(GetFont());
        context.TextBaseline = TextBaseline.Top;
        context.TextAlign = TextAlign.Left;

        var displayText = GetDisplayText();
        var sel = selection;
        var textX = frame.X;
        var textY = frame.Y;

        if (!sel.IsEmpty && focused)
        {
            var selStart = (int)sel.Start;
            var selEnd = (int)sel.End;

            var leftWidth = selStart > 0
                ? context.MeasureText(displayText[..selStart]).Size.Width
                : (nfloat)0;
            var selWidth = context.MeasureText(displayText[selStart..selEnd]).Size.Width;
            var leftAndSelWidth = context.MeasureText(displayText[..selEnd]).Size.Width;
            var selX = leftAndSelWidth - selWidth;

            var allWidth = context.MeasureText(displayText).Size.Width;
            var rightWidth = selEnd < displayText.Length
                ? context.MeasureText(displayText[selEnd..]).Size.Width
                : (nfloat)0;

            var selRect = new Rect(textX + selX, frame.Y, selWidth, frame.Height);
            context.BeginPath();
            context.RoundRect(selRect, SelectionCornerRadius);
            context.SetFill(SelectionBackgroundColor);
            context.Fill();

            if (selStart > 0)
            {
                context.SetFill(Color);
                context.FillText(displayText[..selStart], new Point(textX, textY));
            }

            context.SetFill(SelectedColor);
            context.FillText(displayText[selStart..selEnd], new Point(textX + selX, textY));

            if (selEnd < displayText.Length)
            {
                context.SetFill(Color);
                context.FillText(displayText[selEnd..], new Point(textX + (allWidth - rightWidth), textY));
            }
        }
        else
        {
            context.SetFill(Color);
            context.FillText(displayText, new Point(textX, textY));
        }

        if (focused && caretVisible && sel.IsEmpty)
        {
            var caretOffset = sel.Start > 0
                ? context.MeasureText(displayText[..(int)sel.Start]).Size.Width
                : (nfloat)0;
            context.SetFill(Color);
            context.FillRect(new Rect(textX + caretOffset, frame.Y, 1, frame.Height));
        }
    }

    /// <inheritdoc/>
    public void Animate(View view, TimeSpan previousTime, TimeSpan currentTime)
    {
        if (!view.IsFocused)
            return;

        if (caretToggleTime == TimeSpan.Zero)
            caretToggleTime = currentTime + CaretBlinkInterval;

        if (currentTime >= caretToggleTime)
        {
            caretVisible = !caretVisible;
            caretToggleTime = currentTime + CaretBlinkInterval;
            view.InvalidateRender();
        }

        view.RequestAnimationFrame();
    }

    /// <inheritdoc/>
    public void OnPointerEvent(View view, ref PointerEventRef e, EventPhase phase)
    {
        if (phase != EventPhase.Bubble)
            return;

        if (e.Type == PointerEventType.Down)
        {
            var p = e.State.Position;
            bool inContent = p.X >= contentRect.X && p.X < contentRect.X + contentRect.Width
                          && p.Y >= contentRect.Y && p.Y < contentRect.Y + contentRect.Height;
            if (!inContent)
                return;

            var wasFocused = view.IsFocused;
            view.Focus();

            var cursorPos = HitTestCursor(e.State.Position, e.TextMeasure);
            if (cursorPos.HasValue)
            {
                anchor = cursorPos.Value;
                selection = new Interval<uint>.ClosedOpen(cursorPos.Value, cursorPos.Value);
                isMouseSelecting = true;
                view.CapturePointer(e.PointerId);
            }
            else if (!wasFocused && SelectAllOnFocus)
            {
                anchor = 0;
                selection = new Interval<uint>.ClosedOpen(0, (uint)TextBuffer.Length);
            }

            ResetCaretBlink();
            view.InvalidateRender();
        }
        else if (e.Type == PointerEventType.Move && isMouseSelecting)
        {
            var cursorPos = HitTestCursor(e.State.Position, e.TextMeasure);
            if (cursorPos.HasValue)
            {
                var pos = cursorPos.Value;
                selection = anchor <= pos
                    ? new Interval<uint>.ClosedOpen(anchor, pos)
                    : new Interval<uint>.ClosedOpen(pos, anchor);
                ResetCaretBlink();
                view.InvalidateRender();
            }
        }
        else if (e.Type == PointerEventType.Up && isMouseSelecting)
        {
            isMouseSelecting = false;
            view.ReleasePointer(e.PointerId);
        }
    }

    /// <inheritdoc/>
    public void OnKeyDown(View view, ref KeyEventRef e)
    {
        var sel = selection;
        var len = (uint)TextBuffer.Length;

        switch (e.Key)
        {
            case VirtualKey.Back:
                if (!sel.IsEmpty)
                {
                    TextBuffer.Remove((int)sel.Start, (int)(sel.End - sel.Start));
                    SetCursor(sel.Start);
                }
                else if (sel.Start > 0)
                {
                    TextBuffer.Remove((int)sel.Start - 1, 1);
                    SetCursor(sel.Start - 1);
                }
                ResetCaretBlink(); view.InvalidateRender(); view.InvalidateMeasure();
                e.Handled = true;
                break;

            case VirtualKey.Delete:
                if (!sel.IsEmpty)
                {
                    TextBuffer.Remove((int)sel.Start, (int)(sel.End - sel.Start));
                    SetCursor(sel.Start);
                }
                else if (sel.Start < len)
                {
                    TextBuffer.Remove((int)sel.Start, 1);
                    SetCursor(sel.Start);
                }
                ResetCaretBlink(); view.InvalidateRender(); view.InvalidateMeasure();
                e.Handled = true;
                break;

            case VirtualKey.Left:
                if (e.Shift)
                {
                    var cursor = GetCursor();
                    if (cursor > 0) MoveCursor(cursor - 1);
                }
                else if (!sel.IsEmpty) SetCursor(sel.Start);
                else if (sel.Start > 0) SetCursor(sel.Start - 1);
                ResetCaretBlink(); view.InvalidateRender();
                e.Handled = true;
                break;

            case VirtualKey.Right:
                if (e.Shift)
                {
                    var cursor = GetCursor();
                    if (cursor < len) MoveCursor(cursor + 1);
                }
                else if (!sel.IsEmpty) SetCursor(sel.End);
                else if (sel.Start < len) SetCursor(sel.Start + 1);
                ResetCaretBlink(); view.InvalidateRender();
                e.Handled = true;
                break;

            case VirtualKey.Home:
                if (e.Shift) MoveCursor(0); else SetCursor(0);
                ResetCaretBlink(); view.InvalidateRender();
                e.Handled = true;
                break;

            case VirtualKey.End:
                if (e.Shift) MoveCursor(len); else SetCursor(len);
                ResetCaretBlink(); view.InvalidateRender();
                e.Handled = true;
                break;
        }
    }

    /// <inheritdoc/>
    public void OnChar(View view, ref KeyEventRef e)
    {
        if (char.IsControl(e.Character))
            return;
        if (InputFilter != null && !InputFilter(e.Character))
            return;

        var sel = selection;
        if (!sel.IsEmpty)
            TextBuffer.Remove((int)sel.Start, (int)(sel.End - sel.Start));

        TextBuffer.Insert((int)sel.Start, e.Character);
        SetCursor(sel.Start + 1);
        ResetCaretBlink();
        view.InvalidateRender();
        view.InvalidateMeasure();
        e.Handled = true;
    }

    /// <inheritdoc/>
    public void OnFocus(View view)
    {
        if (SelectAllOnFocus)
            selection = new Interval<uint>.ClosedOpen(0, (uint)TextBuffer.Length);

        caretVisible = true;
        caretToggleTime = TimeSpan.Zero;
        view.RequestAnimationFrame();
    }

    /// <inheritdoc/>
    public void OnBlur(View view)
    {
        caretVisible = false;
        view.InvalidateRender();
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private Font GetFont() => new Font
    {
        FontFamily = FontFamily ?? ["Inter"],
        FontSize   = FontSize > 0 ? FontSize : 15,
        FontWeight = FontWeight,
        FontStretch = FontStretch,
        FontStyle  = FontStyle,
    };

    private string GetDisplayText() =>
        IsPassword ? new string('\u2022', TextBuffer.Length) : TextBuffer.ToString();

    private void SetCursor(uint pos)
    {
        anchor = pos;
        selection = new Interval<uint>.ClosedOpen(pos, pos);
    }

    private void MoveCursor(uint pos)
    {
        selection = anchor <= pos
            ? new Interval<uint>.ClosedOpen(anchor, pos)
            : new Interval<uint>.ClosedOpen(pos, anchor);
    }

    private uint GetCursor() =>
        selection.End == anchor ? selection.Start : selection.End;

    private void ResetCaretBlink()
    {
        caretVisible = true;
        caretToggleTime = TimeSpan.Zero;
    }

    private uint? HitTestCursor(Point pointerPosition, ITextMeasureContext? textMeasure)
    {
        if (textMeasure == null)
            return null;

        textMeasure.SetFont(GetFont());
        var clickX = pointerPosition.X - contentRect.X;
        return (uint)textMeasure.HitTestTextPosition(GetDisplayText(), clickX);
    }
}
