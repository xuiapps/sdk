using Xui.Core.Math2D;
namespace LinkedViews;

public interface IView { Rect Arrange(Rect bounds); }
public abstract class View : IView
{
    public Rect Frame;
    public string Name;
    protected View(string name) { Name = name; }
    public virtual Rect Arrange(Rect bounds) { Frame = bounds; return bounds; }
}
