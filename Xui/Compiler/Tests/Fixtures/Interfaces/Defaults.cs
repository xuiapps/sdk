using System;

public struct Bounds { public int X, Y; }
public interface ILayout
{
    string Name { get; }
    Bounds Arrange(Bounds bounds) => bounds;
    void Show(Bounds bounds)
    {
        Bounds arranged = Arrange(bounds);
        Console.WriteLine(Name);
        Console.WriteLine(arranged.X);
        Console.WriteLine(arranged.Y);
    }
}
public interface IInsetLayout : ILayout
{
    Bounds ILayout.Arrange(Bounds bounds) => new Bounds { X = bounds.X + 10, Y = bounds.Y + 20 };
}
public class Panel : IInsetLayout { public string Name => "panel"; }
public class CustomPanel : Panel, ILayout
{
    public Bounds Arrange(Bounds bounds) => new Bounds { X = bounds.X + 30, Y = bounds.Y + 40 };
}
public static class Program
{
    public static int Main()
    {
        ILayout panel = new Panel();
        panel.Show(new Bounds { X = 1, Y = 2 });
        panel = new CustomPanel();
        panel.Show(new Bounds { X = 3, Y = 4 });
        return 0;
    }
}
