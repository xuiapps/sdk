using Xui.Core.Math2D;
namespace LinkedViews;

public class Panel : View
{
    // This field has the same metadata row as View.Frame, in a different PE.
    public Rect Content;
    public Panel() : base("Panel") { }
    public override Rect Arrange(Rect bounds)
    {
        base.Arrange(bounds);
        Content = new Rect(bounds.X + 10, bounds.Y + 20, bounds.Width, bounds.Height);
        return Content;
    }
}
