using System.Globalization;

namespace Xui.Core.UI.Input;

/// <summary>A UTF-16 range in a text document, with an exclusive end.</summary>
public readonly record struct TextRange(int Start, int End)
{
    /// <summary>The number of UTF-16 code units in the range.</summary>
    public int Length => End - Start;
}

/// <summary>Changes reported before and after a text editing transaction.</summary>
[Flags]
public enum TextEditingChange
{
    /// <summary>The document or marked text changed.</summary>
    Text = 1,
    /// <summary>The insertion point or selected range changed.</summary>
    Selection = 2,
}

/// <summary>
/// Single-line editing state shared by canvas text views and native input adapters.
/// Offsets use UTF-16; selection and deletion respect Unicode grapheme boundaries.
/// </summary>
public sealed class TextEditingState
{
    private string text = string.Empty;
    private TextRange selection;
    private TextRange? markedRange;
    private string? compositionOriginal;
    private TextRange compositionSelection;

    /// <summary>Raised before an editing transaction, while old state is still readable.</summary>
    public event Action<TextEditingChange>? Changing;
    /// <summary>Raised after an editing transaction.</summary>
    public event Action<TextEditingChange>? Changed;
    /// <summary>Optional validation for committed input; composition is validated when committed.</summary>
    public Func<char, bool>? InputFilter { get; set; }

    /// <summary>The document. Assignment ends composition and places the caret at the end.</summary>
    public string Text
    {
        get => text;
        set => Apply(value ?? string.Empty, new((value ?? string.Empty).Length, (value ?? string.Empty).Length), null);
    }

    /// <summary>The normalized selection. Assignments are clamped to the document and grapheme boundaries.</summary>
    public TextRange Selection
    {
        get => selection;
        set
        {
            var normalized = Normalize(value);
            if (markedRange is { } marked && (normalized.Start < marked.Start || normalized.End > marked.End))
                UnmarkText();
            Apply(text, Normalize(value), markedRange);
        }
    }

    /// <summary>The provisional IME composition range, or null.</summary>
    public TextRange? MarkedRange => markedRange;

    /// <summary>Finds the grapheme boundary at or before an offset.</summary>
    public int Boundary(int offset)
    {
        offset = Math.Clamp(offset, 0, text.Length);
        int position = 0;
        while (position < offset)
        {
            int next = position + StringInfo.GetNextTextElementLength(text.AsSpan(position));
            if (next > offset) break;
            position = next;
        }
        return position;
    }

    /// <summary>Finds the previous grapheme boundary.</summary>
    public int Previous(int offset) => Boundary(Math.Max(0, offset - 1));

    /// <summary>Finds the next grapheme boundary.</summary>
    public int Next(int offset)
    {
        int start = Boundary(offset);
        return start < text.Length ? start + StringInfo.GetNextTextElementLength(text.AsSpan(start)) : text.Length;
    }

    /// <summary>Normalizes a range, expanding a nonempty range to whole graphemes.</summary>
    public TextRange Normalize(TextRange range)
    {
        int start = Math.Clamp(Math.Min(range.Start, range.End), 0, text.Length);
        int end = Math.Clamp(Math.Max(range.Start, range.End), 0, text.Length);
        return new(Boundary(start), start == end ? Boundary(end) : Boundary(end) == end ? end : Next(end));
    }

    /// <summary>Replaces the selection (or composition) atomically with committed input.</summary>
    public bool Insert(string value) => Replace(markedRange ?? selection, value);

    /// <summary>Replaces a range with validated single-line input.</summary>
    public bool Replace(TextRange range, string value)
    {
        if (!Accepts(value)) return false;
        range = Normalize(range);
        string updated = string.Concat(text.AsSpan(0, range.Start), value.AsSpan(), text.AsSpan(range.End));
        int caret = range.Start + value.Length;
        Apply(updated, new(caret, caret), null);
        return true;
    }

    /// <summary>Deletes the selection or the grapheme before the caret.</summary>
    public void DeleteBackward() => Replace(selection.Length > 0 ? selection : new(Previous(selection.Start), selection.Start), string.Empty);

    /// <summary>Deletes the selection or the grapheme after the caret.</summary>
    public void DeleteForward() => Replace(selection.Length > 0 ? selection : new(selection.Start, Next(selection.Start)), string.Empty);

    /// <summary>Updates provisional IME text; the selection is relative to the marked text.</summary>
    public void SetMarkedText(string value, TextRange relativeSelection)
    {
        if (value.Any(char.IsControl)) return;
        if (markedRange is null)
        {
            compositionOriginal = text;
            compositionSelection = selection;
        }
        var range = markedRange ?? selection;
        var updated = string.Concat(text.AsSpan(0, range.Start), value.AsSpan(), text.AsSpan(range.End));
        int start = range.Start + Math.Clamp(relativeSelection.Start, 0, value.Length);
        int end = range.Start + Math.Clamp(relativeSelection.End, 0, value.Length);
        Apply(updated, new(start, end), new(range.Start, range.Start + value.Length));
    }

    /// <summary>Commits composition, or restores the pre-composition document if validation fails.</summary>
    public void UnmarkText()
    {
        if (markedRange is not { } range) return;
        if (Accepts(text.AsSpan(range.Start, range.Length))) Apply(text, selection, null);
        else Apply(compositionOriginal ?? string.Empty, compositionSelection, null);
    }

    private bool Accepts(ReadOnlySpan<char> value)
    {
        foreach (char c in value)
            if (char.IsControl(c) || (InputFilter is not null && !InputFilter(c))) return false;
        return true;
    }

    private void Apply(string value, TextRange range, TextRange? marked)
    {
        var change = (text != value || markedRange != marked ? TextEditingChange.Text : 0)
            | (selection != range ? TextEditingChange.Selection : 0);
        if (change == 0) return;
        Changing?.Invoke(change);
        text = value;
        selection = Normalize(range);
        markedRange = marked;
        if (marked is null) compositionOriginal = null;
        Changed?.Invoke(change);
    }
}
