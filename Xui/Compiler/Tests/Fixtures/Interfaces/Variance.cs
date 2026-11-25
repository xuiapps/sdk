using System;

public class Animal { public int Value; }
public class Dog : Animal { }
public interface ISource<out T> { T Get(); T Read() => Get(); }
public interface ISink<in T> { int Put(T value); }

public class Source<T> : ISource<T>
{
    public T Item;
    public Source(T item) { Item = item; }
    public T Get() => Item;
}

public class Sink : ISink<Animal>
{
    public int Put(Animal value) => value.Value;
}

public static class Program
{
    static Animal Read(ISource<Animal> source) => source.Read();
    static int Write(ISink<Dog> sink, Dog dog) => sink.Put(dog);
    static void Inspect(object value)
    {
        Console.WriteLine(value is ISource<Animal>);
        Console.WriteLine(value is ISource<object>);
        Console.WriteLine(value is ISource<Dog>);
    }

    public static int Main()
    {
        var dog = new Dog();
        dog.Value = 42;
        var source = new Source<Dog>(dog);
        Console.WriteLine(Read(source) == dog);
        Inspect(source);
        Inspect(new Source<Animal>(new Animal()));
        Inspect(new Source<int>(7));
        Console.WriteLine(Write(new Sink(), dog));
        return Read(source).Value;
    }
}
