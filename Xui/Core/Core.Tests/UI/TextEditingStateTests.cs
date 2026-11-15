using Xui.Core.UI.Input;

namespace Xui.Core.UI.Tests;

public class TextEditingStateTests
{
    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("👨‍👩‍👧‍👦")]
    [InlineData("🇧🇬")]
    public void DeleteAndMoveRespectWholeGraphemes(string grapheme)
    {
        var editor = new TextEditingState { Text = "a" + grapheme + "b" };
        Assert.Equal(1, editor.Previous(1 + grapheme.Length));
        Assert.Equal(1 + grapheme.Length, editor.Next(1));
        editor.Selection = new(1 + grapheme.Length, 1 + grapheme.Length);
        editor.DeleteBackward();
        Assert.Equal("ab", editor.Text);
        Assert.Equal(new TextRange(1, 1), editor.Selection);
        editor.Insert(grapheme);
        editor.Selection = new(1, 1);
        editor.DeleteForward();
        Assert.Equal("ab", editor.Text);
    }

    [Fact]
    public void NativeOffsetsNormalizeWithoutSplittingUnicodeOrExceedingDocument()
    {
        var editor = new TextEditingState { Text = "a😀b" };
        editor.Selection = new(2, 2);
        Assert.Equal(new TextRange(1, 1), editor.Selection);
        editor.Selection = new(2, 3);
        Assert.Equal(new TextRange(1, 3), editor.Selection);
        editor.Selection = new(int.MaxValue, int.MinValue);
        Assert.Equal(new TextRange(0, 4), editor.Selection);
    }

    [Fact]
    public void CompositionReplacesProvisionalTextAndCommitsAtomically()
    {
        var editor = new TextEditingState { Text = "hello" };
        editor.Selection = new(0, 5);
        editor.SetMarkedText("ni", new(2, 2));
        Assert.Equal("ni", editor.Text);
        Assert.Equal(new TextRange(0, 2), editor.MarkedRange);
        editor.SetMarkedText("你", new(1, 1));
        editor.Insert("你好");
        Assert.Equal("你好", editor.Text);
        Assert.Equal(new TextRange(2, 2), editor.Selection);
        Assert.Null(editor.MarkedRange);
    }

    [Fact]
    public void InvalidCompositionRestoresOriginalDocumentAndSelection()
    {
        var editor = new TextEditingState { Text = "123", InputFilter = char.IsDigit };
        editor.Selection = new(1, 2);
        editor.SetMarkedText("letters", new(7, 7));
        editor.UnmarkText();
        Assert.Equal("123", editor.Text);
        Assert.Equal(new TextRange(1, 2), editor.Selection);
        Assert.Null(editor.MarkedRange);
        Assert.False(editor.Insert("x"));
        Assert.Equal("123", editor.Text);
    }

    [Fact]
    public void CompositionSelectionStaysWithinMarkedTextAndLeavingCommits()
    {
        var editor = new TextEditingState { Text = "abc" };
        editor.SetMarkedText("你好", new(-5, int.MaxValue));
        Assert.Equal(new TextRange(3, 5), editor.Selection);
        editor.Selection = new(0, 0);
        Assert.Null(editor.MarkedRange);
        Assert.Equal("abc你好", editor.Text);
    }

    [Fact]
    public void NotificationsBracketOneAtomicMultiCharacterEdit()
    {
        var editor = new TextEditingState { Text = "old" };
        editor.Selection = new(0, 3);
        var calls = new List<string>();
        editor.Changing += change => calls.Add($"before:{editor.Text}:{change}");
        editor.Changed += change => calls.Add($"after:{editor.Text}:{change}");
        Assert.True(editor.Insert("replacement"));
        Assert.Equal(new[] { "before:old:Text, Selection", "after:replacement:Text, Selection" }, calls);
        calls.Clear();
        editor.Selection = editor.Selection;
        Assert.Empty(calls);
        Assert.False(editor.Insert("new\nline"));
        Assert.Empty(calls);
    }
}
