using System.Collections.ObjectModel;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.Layout;

/// <summary>A resolved unboxed value-type layout for one target.</summary>
public sealed class StructLayout
{
    internal StructLayout(int size, int alignment, Dictionary<FieldDefinitionHandle, FieldLayout> fields)
    {
        Size = size;
        Alignment = alignment;
        Fields = new ReadOnlyDictionary<FieldDefinitionHandle, FieldLayout>(fields);
    }

    /// <summary>The total storage size in bytes, including padding or a larger declared size.</summary>
    public int Size { get; }
    /// <summary>The required alignment in bytes, after applying packing.</summary>
    public int Alignment { get; }
    /// <summary>Instance-field offsets and sizes, keyed by their source metadata handles. Static fields are excluded.</summary>
    public IReadOnlyDictionary<FieldDefinitionHandle, FieldLayout> Fields { get; }
}
