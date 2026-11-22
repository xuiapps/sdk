using System;

public struct Point { public int X; public int Y; }
public struct Label { public string Text; public int Number; }
public struct Caption { public Label Value; public Widget Target; }

public class Widget
{
    public Point Position;
    public Caption Caption;
    public Widget(int x, string text)
    {
        Position.X = x;
        Caption.Value.Text = text;
    }
}

public static class Program
{
    public static int Main()
    {
        var original = new Widget(1, "first");
        var alias = original;
        alias.Position.X = 4;
        var label = original.Caption.Value;
        label.Text = "copy";
        label.Number = 7;
        original.Caption.Value.Number = 2;
        original.Caption.Target = original;
        var other = new Widget(9, "second");
        other.Caption = original.Caption;
        other.Caption.Value.Text = "other";
        other.Caption.Target.Position.Y = 6;
        Console.WriteLine(original.Position.X);
        Console.WriteLine(original.Position.Y);
        Console.WriteLine(original.Caption.Value.Text);
        Console.WriteLine(label.Text);
        Console.WriteLine(other.Caption.Value.Text);
        Console.WriteLine(original.Caption.Value.Number);
        Console.WriteLine(label.Number);
        var point = original.Position;
        point.X = 20;
        original.Position = point;
        point.X = 99;
        return original.Position.X;
    }
}
