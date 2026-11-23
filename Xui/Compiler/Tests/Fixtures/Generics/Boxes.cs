using System;

public class Box<T>
{
    public T Value;
    public Box(T value) { Value = value; }
    public T Read() => Value;
    public void Write(T value) { Value = value; }
    public T Exchange(T value)
    {
        T previous = Value;
        Value = value;
        return previous;
    }
    public static T Echo(T value) => value;
}

public static class Program
{
    public static int Main()
    {
        var number = new Box<int>(41);
        number.Write(number.Read() + 1);
        var text = new Box<string>("Hello");
        Console.WriteLine(number.Read());
        Console.WriteLine(text.Read());
        Console.WriteLine(number.Exchange(7));
        Console.WriteLine(number.Value);
        Console.WriteLine(text.Exchange("World"));
        Console.WriteLine(text.Read());
        return Box<int>.Echo(0);
    }
}
