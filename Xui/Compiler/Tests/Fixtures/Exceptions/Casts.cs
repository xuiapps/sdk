using System;

public interface IValue { int Read(); }
public class Value : IValue { public int Read() => 42; }
public class Other { }

public static class Program
{
    private static int Read(object value)
    {
        try
        {
            return ((IValue)value).Read();
        }
        catch (InvalidCastException error)
        {
            Console.WriteLine(error.Message);
            return -1;
        }
        finally
        {
            Console.WriteLine("finished");
        }
    }

    public static int Main()
    {
        Console.WriteLine(Read(new Other()));
        return Read(new Value());
    }
}
