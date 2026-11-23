public interface IPosition { int X { get; } }
public interface IMovable : IPosition { int Move(int distance); }
public interface INamed { string Name { get; } }
public interface IMarker { }
public interface IMissing { }

public class Unit : IMovable, INamed
{
    public int Position;
    public virtual int X => Position;
    public int Move(int distance) { Position += distance; return X; }
    string INamed.Name => "Scout";
}

public class Scout : Unit, IMarker
{
    public override int X => Position + 10;
}

public static class Program
{
    private static int Inspect(IPosition position)
    {
        System.Console.WriteLine(position is IMovable);
        System.Console.WriteLine(position is IMarker);
        System.Console.WriteLine(position is IMissing);
        System.Console.WriteLine((position as IMissing) == null);
        INamed named = position as INamed;
        System.Console.WriteLine(named.Name);
        int result = ((IMovable)position).Move(4);
        System.Console.WriteLine(position.X);
        System.Console.WriteLine(((Scout)position).Position);
        return result;
    }

    public static int Main() => Inspect(new Scout());
}
