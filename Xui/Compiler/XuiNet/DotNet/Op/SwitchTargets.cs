using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

/// <summary>A borrowed view of a switch table, decoding little-endian displacements on access.</summary>
/// <remarks>No target array is allocated. Keep the owning assembly undisposed while using this view.</remarks>
public readonly ref struct SwitchTargets
{
    private readonly BlobReader table;

    internal SwitchTargets(BlobReader table, int length, int baseOffset)
    {
        this.table = table;
        Length = length;
        BaseOffset = baseOffset;
    }

    /// <summary>The number of encoded destinations.</summary>
    public int Length { get; }

    /// <summary>The IL offset immediately after the entire switch instruction, relative to which entries are encoded.</summary>
    public int BaseOffset { get; }

    /// <summary>Reads an entry and returns its absolute IL byte offset.</summary>
    public int this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            var reader = table;
            reader.Offset += index * sizeof(int);
            return BaseOffset + reader.ReadInt32();
        }
    }

    /// <summary>Creates a non-allocating cursor over the destinations.</summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>A pattern-based enumerator over switch destinations.</summary>
    public ref struct Enumerator
    {
        private readonly SwitchTargets targets;
        private int index;

        internal Enumerator(SwitchTargets targets)
        {
            this.targets = targets;
            index = -1;
        }

        /// <summary>The current absolute IL byte offset.</summary>
        public readonly int Current => targets[index];

        /// <summary>Advances to the next destination.</summary>
        public bool MoveNext() => ++index < targets.Length;
    }
}
