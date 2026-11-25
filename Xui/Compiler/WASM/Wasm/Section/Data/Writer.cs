using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Data;

/// <summary>Writes active byte data and passive UTF-16 data segments, counting entries automatically.</summary>
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

    /// <summary>Initializes bytes in memory zero at a constant offset during module instantiation.</summary>
    public DataIndex Active(int offset, ReadOnlySpan<byte> bytes)
    {
        section.U32((uint)DataSegmentKind.Active).Instruction(OpCode.I32Const).Signed(offset).Instruction(OpCode.End).U32((uint)bytes.Length);
        foreach (byte value in bytes) section.Byte(value);
        return new DataIndex(count++);
    }

    /// <summary>Patches the segment count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
