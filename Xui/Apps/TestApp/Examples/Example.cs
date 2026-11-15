using Xui.Core.UI;

public class Example : View
{
    public Example() => Id = $"Example.{GetType().Name}";

    private View? content;

    public string Title { init; get; } = "Example";

    public View? Content
    {
        get => this.content;
        init => this.SetProtectedChild(ref this.content, value);
    }

    public override int Count => this.content is null ? 0 : 1;

    public override View this[int index] =>
        index == 0 && this.content is not null ? this.content : throw new IndexOutOfRangeException();
}
