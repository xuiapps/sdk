namespace Xui.Core.Canvas.Composition;

public abstract partial class Node
{
    /// <summary>Stack-only enumerator over a node's visible children.</summary>
    public ref struct Enumerator
    {
        private readonly Node node;
        private int index;

        internal Enumerator(Node node) { this.node = node; index = -1; }

        /// <summary>The current child. Valid only after MoveNext returns true.</summary>
        public readonly Node Current => index >= 0 && index < node.Count
            ? node[index] : throw new InvalidOperationException("The enumerator is not positioned on a child.");

        /// <summary>Advances to the next visible child in presentation order.</summary>
        public bool MoveNext()
        {
            if (index < node.Count) index++;
            return index < node.Count;
        }
    }
}
