public class Box<T>
{
    public T Value;
    public virtual T Read() => Value;
    public int Unused() => 99;
}

public sealed class IntBox : Box<int>
{
    public override int Read() => Value + 1;
}

public sealed class UnusedBox : Box<int>
{
    public override int Read() => 100;
}

public static class Program
{
    private static int Read(Box<int> box) => box.Read();
    private static Box<int> Create() => new IntBox();

    public static int Main()
    {
        var box = Create();
        box.Value = 41;
        return Read(box);
    }
}
