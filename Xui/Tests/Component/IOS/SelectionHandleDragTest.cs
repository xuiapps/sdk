using Xui.Core.UI.Input;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public class SelectionHandleDragTest
{
    [Theory]
    [InlineData(true, 4, 5)]
    [InlineData(false, 1, 2)]
    public void MeetingAndCrossingRetainsBothEndpointsAndCanExpandAgain(bool leading, int start, int end)
    {
        var editor = new TextEditingState { Text = "abcdef", Selection = new(1, 5) };
        var drag = new IOSTextSelectionDrag();
        drag.Begin(editor.Selection, leading);
        int anchor = leading ? 5 : 1;
        var minimum = new TextRange(start, end);
        editor.Selection = drag.ConstrainSelection(editor, new(anchor, anchor));
        Assert.Equal(minimum, editor.Selection);
        Assert.Equal(leading ? start : end, drag.ConstrainPosition(editor, anchor));

        var crossed = leading ? new TextRange(5, 6) : new TextRange(0, 1);
        Assert.Equal(minimum, drag.ConstrainSelection(editor, crossed));
        Assert.Equal(leading ? start : end, drag.ConstrainPosition(editor, leading ? 6 : 0));
        editor.Selection = drag.ConstrainSelection(editor, new(1, 5));
        Assert.Equal(new TextRange(1, 5), editor.Selection);

        drag.End(); // Release or cancellation restores ordinary caret placement.
        Assert.Equal(new TextRange(3, 3), drag.ConstrainSelection(editor, new(3, 3)));
        Assert.Equal(3, drag.ConstrainPosition(editor, 3));
    }

    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("👨‍👩‍👧‍👦")]
    public void MinimumSelectionIsOneWholeGraphemeAtEitherDocumentEdge(string text)
    {
        var editor = new TextEditingState { Text = text, Selection = new(0, text.Length) };
        foreach (bool leading in new[] { true, false })
        {
            var drag = new IOSTextSelectionDrag();
            drag.Begin(editor.Selection, leading);
            int anchor = leading ? text.Length : 0;
            Assert.Equal(new TextRange(0, text.Length), drag.ConstrainSelection(editor, new(anchor, anchor)));
            Assert.Equal(leading ? 0 : text.Length, drag.ConstrainPosition(editor, anchor));
        }
    }

    [Fact]
    public void CaretOnlyInteractionsAndEmptyFieldsRemainUnrestricted()
    {
        var editor = new TextEditingState { Text = "abc", Selection = new(1, 1) };
        var drag = new IOSTextSelectionDrag();
        drag.Begin(editor.Selection, true);
        Assert.Equal(new TextRange(2, 2), drag.ConstrainSelection(editor, new(2, 2)));
        Assert.Equal(2, drag.ConstrainPosition(editor, 2));
        editor.Text = "";
        drag.Begin(editor.Selection, false);
        Assert.Equal(new TextRange(0, 0), drag.ConstrainSelection(editor, new(0, 0)));
    }
}
