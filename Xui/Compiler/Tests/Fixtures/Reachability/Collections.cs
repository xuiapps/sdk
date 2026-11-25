public sealed class Dictionary
{
    public int Value;
    public int Read() => Value;
    public int Unused() => 99;
}

public sealed class HashSet
{
    public int Count() => 7;
}

internal sealed class InternalCollection
{
    public int Count() => 8;
}

public static class Program
{
    public static int Main()
    {
        var values = new Dictionary();
        values.Value = 42;
        return values.Read();
    }

    public static int OtherEntry() => new HashSet().Count();
    private static int PrivateEntry() => new InternalCollection().Count();
}
