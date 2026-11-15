using System.Buffers;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math1D;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// A leaf layer that implements single-line text input: typing, selection, caret blinking,
/// password masking, and mouse-driven selection. Shares document state with native text input adapters.
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

    private TextEditingState? editor;
    /// <summary>The editing state shared with native adapters.</summary>
    public TextEditingState Editor => editor ??= new();
    /// <summary>Whether the platform draws and manipulates the selection.</summary>
    public bool NativeSelection { get; set; }

    private Interval<uint>.ClosedOpen selection
    {
        get => new((uint)Editor.Selection.Start, (uint)Editor.Selection.End);
        set => Editor.Selection = new((int)Math.Min(value.Start, (uint)int.MaxValue), (int)Math.Min(value.End, (uint)int.MaxValue));
    }
    private bool caretVisible;
    private TimeSpan caretToggleTime;
    private uint anchor;
    private bool isSelecting;
    private bool touchCaretDrag;
    private int selectingPointer;
    private Point pointerDown;
    private ITextMagnifier? magnifier;

    // ── Layout state ─────────────────────────────────────────────────────

    /// <summary>The content rect last assigned by <see cref="Arrange"/>. Used for text hit-testing.</summary>
    private Rect contentRect;

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>Whether the input contains text, without allocating a string.</summary>
    public bool HasText => editor is { Text.Length: > 0 };

    /// <summary>Gets or sets the text content.</summary>
    public string Text
    {
        get => Editor.Text;
        set => Editor.Text = value;
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
    public Func<char, bool>? InputFilter { get => Editor.InputFilter; set => Editor.InputFilter = value; }

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

    /// <summary>Font used for input measurement, hit testing, and rendering.</summary>
    public Font Font { get; set; }

    private readonly Frame EffectivePadding(View view) =>
        view.GetService(typeof(IDeviceInfo)) is IDeviceInfo { Platform: DevicePlatform.iOS }
            ? Padding + new Frame(2, 2, 2, 2)
            : Padding;

    // ── ILayer<View> ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Size Measure(View view, Size availableSize, IMeasureContext context)
    {
        context.SetFont(Font);
        var len = Editor.Text.Length;
        char[]? pooled = null;
        Span<char> buf = len <= MaxStackBufferSize
            ? stackalloc char[MaxStackBufferSize]
            : (pooled = ArrayPool<char>.Shared.Rent(len));
        try
        {
            var displayText = FillDisplayBuffer(buf, len);
            var textMetrics = context.MeasureText(displayText.Length > 0 ? displayText : "X".AsSpan());
            var width = nfloat.Max(availableSize.Width, 100);
            return new Size(width, textMetrics.Size.Height + EffectivePadding(view).TotalHeight);
        }
        finally
        {
            if (pooled != null) ArrayPool<char>.Shared.Return(pooled);
        }
    }

    /// <inheritdoc/>
    public void Arrange(View view, Rect rect, IMeasureContext context)
    {
        contentRect = rect - EffectivePadding(view);
    }

    /// <inheritdoc/>
    public void Render(View view, IContext context)
    {
        var focused = view.IsFocused;
        var frame = contentRect;

        context.SetFont(Font);
        context.TextBaseline = TextBaseline.Top;
        context.TextAlign = TextAlign.Left;

        var len = Editor.Text.Length;
        char[]? pooled = null;
        Span<char> buf = len <= MaxStackBufferSize
            ? stackalloc char[MaxStackBufferSize]
            : (pooled = ArrayPool<char>.Shared.Rent(len));
        try
        {
            var displayText = FillDisplayBuffer(buf, len);
            var sel = selection;
            var textX = frame.X;
            var textY = frame.Y;

            if (!sel.IsEmpty && focused && !NativeSelection)
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

            if (Editor.MarkedRange is { Length: > 0 } marked)
            {
                var left = marked.Start == 0 ? 0 : context.MeasureText(displayText[..marked.Start]).Size.Width;
                var right = context.MeasureText(displayText[..marked.End]).Size.Width;
                context.SetFill(Color);
                context.FillRect(new Rect(textX + left, frame.Bottom - 1, right - left, 1));
            }

            if (focused && caretVisible && sel.IsEmpty && !NativeSelection)
            {
                var caretOffset = sel.Start > 0
                    ? context.MeasureText(displayText[..(int)sel.Start]).Size.Width
                    : (nfloat)0;
                context.SetFill(Color);
                context.FillRect(new Rect(textX + caretOffset, frame.Y, 1, frame.Height));
            }
        }
        finally
        {
            if (pooled != null) ArrayPool<char>.Shared.Return(pooled);
        }
    }

    /// <inheritdoc/>
    public void Animate(View view, TimeSpan previousTime, TimeSpan currentTime)
    {
        if (!view.IsFocused || NativeSelection)
            return;

        if (caretToggleTime == TimeSpan.Zero)
            caretToggleTime = currentTime + CaretBlinkInterval;

        if (touchCaretDrag && isSelecting)
            caretVisible = true;
        else if (currentTime >= caretToggleTime)
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
            if (isSelecting) return;
            // The padded field is tappable too; only text measurement uses contentRect.
            if (!view.Frame.Contains(e.State.Position))
                return;

            var wasFocused = view.IsFocused;
            view.Focus();

            var cursorPos = HitTestCursor(e.State.Position, e.TextMeasure);
            if (cursorPos.HasValue)
            {
                anchor = cursorPos.Value;
                if (wasFocused || !SelectAllOnFocus || e.State.PointerType != PointerType.Touch)
                    selection = new Interval<uint>.ClosedOpen(cursorPos.Value, cursorPos.Value);
                isSelecting = true;
                selectingPointer = e.PointerId;
                touchCaretDrag = e.State.PointerType == PointerType.Touch;
                pointerDown = e.State.Position;
                // Text selection is an ongoing drag — ancestor scroll views must not steal capture.
                view.CapturePointer(e.PointerId, PointerGestures.Drag);
            }
            else if (!wasFocused && SelectAllOnFocus)
            {
                anchor = 0;
                selection = new Interval<uint>.ClosedOpen(0, (uint)Editor.Text.Length);
            }

            ResetCaretBlink();
            view.InvalidateRender();
        }
        else if (e.Type == PointerEventType.Move && isSelecting && e.PointerId == selectingPointer)
        {
            if (NativeSelection) return;
            var cursorPos = HitTestCursor(e.State.Position, e.TextMeasure);
            if (cursorPos.HasValue)
            {
                var pos = cursorPos.Value;
                if (touchCaretDrag)
                {
                    SetCursor(pos);
                    var travel = e.State.Position - pointerDown;
                    if (magnifier is not null || nfloat.Max(nfloat.Abs(travel.X), nfloat.Abs(travel.Y)) >= 4)
                        UpdateMagnifier(view, e.State.Position, e.TextMeasure!);
                }
                else
                    selection = anchor <= pos
                        ? new Interval<uint>.ClosedOpen(anchor, pos)
                        : new Interval<uint>.ClosedOpen(pos, anchor);
                ResetCaretBlink();
                view.InvalidateRender();
            }
        }
        else if (e.Type is PointerEventType.Up or PointerEventType.Cancel or PointerEventType.LostCapture
            && isSelecting && e.PointerId == selectingPointer)
        {
            if (NativeSelection && e.Type == PointerEventType.LostCapture)
                (view.GetService(typeof(ITextInputSession)) as ITextInputSession)?.CancelSelectionGesture();
            EndSelection(view);
        }
    }

    /// <inheritdoc/>
    public void OnKeyDown(View view, ref KeyEventRef e)
    {
        var sel = selection;
        var len = (uint)Editor.Text.Length;
        if (sel.IsEmpty) anchor = sel.Start;
        else if (anchor != sel.Start && anchor != sel.End) anchor = sel.Start;

        switch (e.Key)
        {
            case VirtualKey.Return when view is ITextInputClient client:
                client.PerformInputAction(TextInputAction.Default);
                e.Handled = true;
                break;

            case VirtualKey.Back:
                Editor.DeleteBackward();
                anchor = (uint)Editor.Selection.Start;
                ResetCaretBlink(); view.InvalidateRender(); view.InvalidateMeasure();
                e.Handled = true;
                break;

            case VirtualKey.Delete:
                Editor.DeleteForward();
                anchor = (uint)Editor.Selection.Start;
                ResetCaretBlink(); view.InvalidateRender(); view.InvalidateMeasure();
                e.Handled = true;
                break;

            case VirtualKey.Left:
                if (e.Shift)
                {
                    var cursor = GetCursor();
                    if (cursor > 0) MoveCursor((uint)Editor.Previous((int)cursor));
                }
                else if (!sel.IsEmpty) SetCursor(sel.Start);
                else if (sel.Start > 0) SetCursor((uint)Editor.Previous((int)sel.Start));
                ResetCaretBlink(); view.InvalidateRender();
                e.Handled = true;
                break;

            case VirtualKey.Right:
                if (e.Shift)
                {
                    var cursor = GetCursor();
                    if (cursor < len) MoveCursor((uint)Editor.Next((int)cursor));
                }
                else if (!sel.IsEmpty) SetCursor(sel.End);
                else if (sel.Start < len) SetCursor((uint)Editor.Next((int)sel.Start));
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

        if (!Editor.Insert(e.Character.ToString())) return;
        anchor = (uint)Editor.Selection.Start;
        ResetCaretBlink();
        view.InvalidateRender();
        view.InvalidateMeasure();
        e.Handled = true;
    }

    /// <inheritdoc/>
    public void OnFocus(View view)
    {
        if (SelectAllOnFocus)
            selection = new Interval<uint>.ClosedOpen(0, (uint)Editor.Text.Length);

        caretVisible = true;
        caretToggleTime = TimeSpan.Zero;
        view.RequestAnimationFrame();
    }

    /// <inheritdoc/>
    public void OnBlur(View view)
    {
        EndSelection(view);
        caretVisible = false;
        view.InvalidateRender();
    }

    // ── Private helpers ──────────────────────────────────────────────────

    // Stack-allocate display text for up to this many characters; rent from ArrayPool beyond that.
    private const int MaxStackBufferSize = 256;

    private void EndSelection(View view)
    {
        var wasSelecting = isSelecting;
        isSelecting = false;
        touchCaretDrag = false;
        var previous = magnifier;
        magnifier = null;
        previous?.Hide();
        // Release synchronously dispatches LostCapture; clear state first.
        if (wasSelecting) view.ReleasePointer(selectingPointer);
    }

    private void UpdateMagnifier(View view, Point position, ITextMeasureContext measure)
    {
        if (!view.IsFocused) return;
        magnifier ??= view.GetService(typeof(ITextMagnifier)) is ITextMagnifier { IsSupported: true } service
            ? service : null;
        if (magnifier is null) return;

        measure.SetFont(Font);
        var len = Editor.Text.Length;
        char[]? pooled = null;
        Span<char> buffer = len <= MaxStackBufferSize
            ? stackalloc char[MaxStackBufferSize] : (pooled = ArrayPool<char>.Shared.Rent(len));
        try
        {
            var text = FillDisplayBuffer(buffer, len);
            var offset = selection.Start == 0 ? 0 : measure.MeasureText(text[..(int)selection.Start]).Size.Width;
            var caret = new Rect(contentRect.X + offset, contentRect.Y, 1, contentRect.Height);
            // Include scrolling, carousel scaling, and navigation transforms.
            for (View? ancestor = view; ancestor is not null; ancestor = ancestor.Parent)
                caret = ancestor.InverseTransformRect(caret);
            magnifier.Show(view.LocalToGlobal(position), caret);
        }
        finally
        {
            if (pooled is not null) ArrayPool<char>.Shared.Return(pooled);
        }
    }

    /// <summary>
    /// Fills <paramref name="buf"/> with the display characters (bullets for password,
    /// raw text otherwise) and returns a <see cref="ReadOnlySpan{T}"/> over the filled region.
    /// The caller must provide a buffer of at least <paramref name="len"/> characters,
    /// and must not use the returned span after the buffer's lifetime ends.
    /// </summary>
    private ReadOnlySpan<char> FillDisplayBuffer(Span<char> buf, int len)
    {
        if (len == 0)
            return ReadOnlySpan<char>.Empty;

        if (IsPassword)
            buf[..len].Fill('\u2022');
        else
            Editor.Text.AsSpan().CopyTo(buf);

        return buf[..len];
    }

    /// <summary>Returns caret geometry in window coordinates.</summary>
    public Rect GetCaretRect(View view, int offset, ITextMeasureContext measure)
    {
        measure.SetFont(Font);
        int length = Editor.Boundary(offset);
        char[]? pooled = null;
        Span<char> buffer = length <= MaxStackBufferSize ? stackalloc char[MaxStackBufferSize]
            : (pooled = ArrayPool<char>.Shared.Rent(length));
        try
        {
            if (IsPassword) buffer[..length].Fill('\u2022');
            else Editor.Text.AsSpan(0, length).CopyTo(buffer);
            var width = length == 0 ? 0 : measure.MeasureText(buffer[..length]).Size.Width;
            var rect = new Rect(contentRect.X + width, contentRect.Y, 1, contentRect.Height);
            for (View? ancestor = view; ancestor is not null; ancestor = ancestor.Parent)
                rect = ancestor.InverseTransformRect(rect);
            return rect;
        }
        finally { if (pooled is not null) ArrayPool<char>.Shared.Return(pooled); }
    }

    /// <summary>Hit-tests text using a point in window coordinates.</summary>
    public int GetTextPosition(View view, Point point, ITextMeasureContext measure) =>
        (int)(HitTestCursor(view.GlobalToLocal(point), measure) ?? 0);

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

        textMeasure.SetFont(Font);
        var clickX = pointerPosition.X - contentRect.X;
        var len = Editor.Text.Length;
        char[]? pooled = null;
        Span<char> buf = len <= MaxStackBufferSize
            ? stackalloc char[MaxStackBufferSize]
            : (pooled = ArrayPool<char>.Shared.Rent(len));
        try
        {
            return (uint)Editor.Boundary(textMeasure.HitTestTextPosition(FillDisplayBuffer(buf, len), clickX));
        }
        finally
        {
            if (pooled != null) ArrayPool<char>.Shared.Return(pooled);
        }
    }
}
