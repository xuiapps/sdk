using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.Memory;

/// <summary>Writes unshared WASM32 memory definitions, measured in 64 KiB pages.</summary>
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

    /// <summary>Defines a memory with initial and optional maximum page counts.</summary>
    public MemoryIndex Memory(uint minimum, uint? maximum = null)
    {
        section.Byte((byte)(maximum.HasValue ? LimitsKind.MinimumAndMaximum : LimitsKind.Minimum)).U32(minimum);
        if (maximum is { } limit) section.U32(limit);
        return new(count++);
    }

    /// <summary>Patches the definition count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
