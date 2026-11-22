using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.Data;

/// <summary>Writes passive UTF-16 data segments and counts the entries automatically.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;

    internal Writer(Section.Writer section)
    {
        this.section = section;
        countPosition = section.ReserveU32();
    }

    /// <summary>Writes a passive segment containing the string's UTF-16 little-endian code units.</summary>
    /// <remarks>Preserves every code unit, including unpaired surrogates, without transcoding.</remarks>
    /// <param name="value">The literal to store in the module.</param>
    /// <returns>The new segment's index in the module's data index space.</returns>
    public DataIndex PassiveUtf16(string value)
    {
        section.U32((uint)DataSegmentKind.Passive).U32((uint)value.Length * 2);
        foreach (char codeUnit in value)
            section.Byte((byte)codeUnit).Byte((byte)(codeUnit >> 8));
        return new DataIndex(count++);
    }

    /// <summary>Patches the segment count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
