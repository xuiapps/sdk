using Xui.Core.Math2D;
using LinkedViews;

public class Child : Panel
{
    public int Count;
    public override Rect Arrange(Rect bounds) { Count += 1; return base.Arrange(bounds); }
}
public static class Program
{
    public static int Main()
    {
        var child = new Child();
        IView view = child;
        Rect result = view.Arrange(new Rect(1, 2, 30, 40));
        View parent = child;
        System.Console.WriteLine(parent.Name);
        System.Console.WriteLine((double)child.Frame.X);
        System.Console.WriteLine((double)child.Content.X);
        System.Console.WriteLine((double)result.Y);
        System.Console.WriteLine(parent is Panel);
        return (parent as Panel).Content.Width == result.Width ? child.Count + 41 : 1;
    }
}
