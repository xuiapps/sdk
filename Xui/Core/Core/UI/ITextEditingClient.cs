using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI;

/// <summary>A canvas text editor that a native text-input service can manipulate.</summary>
public interface ITextEditingClient : ITextInputClient
{
    /// <summary>The document and selection shared with the native input system.</summary>
    TextEditingState Editor { get; }
    /// <summary>Whether a native interaction owns caret, selection handles, and touch selection.</summary>
    bool NativeSelection { get; set; }
    /// <summary>The input's hit-test bounds, in logical window coordinates.</summary>
    Rect InputBounds { get; }
    /// <summary>Returns the caret rectangle for a UTF-16 offset, in logical window coordinates.</summary>
    Rect GetCaretRect(int offset, ITextMeasureContext measure);
    /// <summary>Hit-tests a logical window point, returning a UTF-16 grapheme boundary.</summary>
    int GetTextPosition(Point point, ITextMeasureContext measure);
}
