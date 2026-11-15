namespace Xui.Core.Canvas.Composition;

public abstract partial class Node
{
    /// <summary>Number of visible children in back-to-front order. Leaf nodes return zero.</summary>
    public virtual int Count => 0;

    /// <summary>Returns the visible child at the given index.</summary>
    public virtual Node this[int index] => throw new IndexOutOfRangeException();

    /// <summary>Enumerates visible children without allocating or boxing an enumerator.</summary>
    /// <remarks>Do not mutate the hierarchy during enumeration.</remarks>
    public Enumerator GetEnumerator() => new(this);
}
