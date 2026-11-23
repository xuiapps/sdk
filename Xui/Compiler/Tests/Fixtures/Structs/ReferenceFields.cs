namespace Samples.References;

public struct Label
{
    public int X;
    public string Text;
}

public struct NestedLabel
{
    public Label Caption;
    public int Y;
}

public struct ObjectField { public object Value; }

public struct ArrayField
{
    public int[] Values;
    public int[,] Grid;
    public int[][] Rows;
}

public class Node { public Node Next; }
public interface INode { }
public struct ClassField { public Node Value; }
public struct InterfaceField { public INode Value; }

public struct StaticOnly
{
    public static string Shared;
    public int Value;
}
