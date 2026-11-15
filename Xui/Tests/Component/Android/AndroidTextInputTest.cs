using Xui.Core.UI.Input;
using Xui.Runtime.Android.Actual;
using Xunit;
namespace Xui.Tests.Component.Android;

public class AndroidTextInputTest
{
    [Fact]
    public void ImeCompositionReplacesMarkedTextAndCommitsAtUtf16Caret()
    {
        var editor=new TextEditingState {Text="Hello "};
        AndroidTextInput.Apply(editor,2,1,0,"wo");
        Assert.Equal(new TextRange(6,8),editor.MarkedRange);
        AndroidTextInput.Apply(editor,2,1,0,"world");
        Assert.Equal("Hello world",editor.Text);
        AndroidTextInput.Apply(editor,1,1,0,"world 🌍");
        Assert.Null(editor.MarkedRange);Assert.Equal("Hello world 🌍",editor.Text);
        Assert.Equal(editor.Text.Length,editor.Selection.Start);
        AndroidTextInput.Apply(editor,8,0,0,"");
        Assert.Equal("Hello world ",editor.Text);
    }
    [Fact]
    public void RejectedCompositionRestoresDocumentAndSelection()
    {
        var editor=new TextEditingState {Text="12",InputFilter=char.IsAsciiDigit};
        AndroidTextInput.Apply(editor,2,1,0,"abc");
        Assert.Equal("12abc",editor.Text);
        AndroidTextInput.Apply(editor,3,0,0,"");
        Assert.Equal("12",editor.Text);Assert.Equal(new TextRange(2,2),editor.Selection);
    }
    [Fact]
    public void CursorOffsetAndSurroundingDeleteRespectGraphemes()
    {
        var editor=new TextEditingState {Text="ab"};
        AndroidTextInput.Apply(editor,5,1,1,"");
        AndroidTextInput.Apply(editor,1,0,0,"🌍");
        Assert.Equal("a🌍b",editor.Text);Assert.Equal(new TextRange(1,1),editor.Selection);
        AndroidTextInput.Apply(editor,4,0,1,"");
        Assert.Equal("ab",editor.Text);
    }
}
