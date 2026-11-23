using System;

public abstract class Box<T>
{
    public T Value;

    protected Box(T value) { Value = value; }
    public virtual T Read() => Value;
    public abstract T Echo(T value);
}

public class TaggedBox<TTag, T> : Box<T>
{
    public TTag Tag;

    public TaggedBox(T value, TTag tag) : base(value) { Tag = tag; }
    public override T Read() => base.Read();
    public override T Echo(T value) => value;
}

public sealed class IntBox : TaggedBox<string, int>
{
    public int Extra;

    public IntBox(int value, int extra) : base(value, "number") { Extra = extra; }
    public override int Read() => base.Read() + Extra;
}

public static class Program
{
    private static int ReadNumber(Box<int> box)
    {
        Console.WriteLine(box.Read());
        Console.WriteLine(box.Echo(7));
        Console.WriteLine(box is IntBox);
        Console.WriteLine(box is TaggedBox<string, int>);
        var tagged = box as TaggedBox<string, int>;
        Console.WriteLine(tagged.Tag);
        return box.Read();
    }

    private static void ReadText(Box<string> box)
    {
        Console.WriteLine(box.Read());
        Console.WriteLine(box.Echo("World"));
        Console.WriteLine(box is TaggedBox<int, string>);
        Console.WriteLine((box as TaggedBox<string, string>) == null);
    }

    public static int Main()
    {
        var number = new IntBox(40, 2);
        int result = ReadNumber(number);
        ReadText(new TaggedBox<int, string>("Hello", 3));
        number.Value = 50;
        Console.WriteLine(number.Read());
        return result;
    }
}
