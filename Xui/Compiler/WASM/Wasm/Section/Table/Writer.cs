using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.WASM.Wasm.Section.Memory;

namespace Xui.Compiler.WASM.Wasm.Section.Table;

/// <summary>Writes fixed-size nullable tables for indirect dispatch and managed reference slots.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;
    internal Writer(Section.Writer section) { this.section = section; countPosition = section.ReserveU32(); }
    /// <summary>Defines a table with equal minimum and maximum sizes, initially null.</summary>
    public TableIndex Functions(uint size)
    {
        return References(ValueTypeCode.FuncRef, size);
    }
    /// <summary>Defines a fixed-size nullable reference table, initially filled with null.</summary>
    public TableIndex References(ValueTypeCode type, uint size)
    {
        section.Byte((byte)type).Byte((byte)LimitsKind.MinimumAndMaximum).U32(size).U32(size);
        return new(count++);
    }
    /// <summary>Patches the table count and section length.</summary>
    public void Dispose() { section.PatchU32(countPosition, count); section.Dispose(); }
}
