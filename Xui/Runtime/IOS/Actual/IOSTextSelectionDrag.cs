using System;
using Xui.Core.UI.Input;

namespace Xui.Runtime.IOS.Actual;

/// <summary>Keeps a native handle drag nonempty without splitting a grapheme.</summary>
internal struct IOSTextSelectionDrag
{
    private bool active, leading;
    private int anchor;

    internal void Begin(TextRange selection, bool leading)
    {
        active = selection.Length > 0;
        this.leading = leading;
        anchor = leading ? selection.End : selection.Start;
    }

    internal void End() => active = false;

    internal int ConstrainPosition(TextEditingState editor, int position)
    {
        if (!active) return position;
        return leading ? Math.Min(position, editor.Previous(anchor))
            : Math.Max(position, editor.Next(anchor));
    }

    internal TextRange ConstrainSelection(TextEditingState editor, TextRange proposed)
    {
        if (!active) return proposed;
        return leading ? new(ConstrainPosition(editor, proposed.Start), anchor)
            : new(anchor, ConstrainPosition(editor, proposed.End));
    }
}
