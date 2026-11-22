using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.WASM.Wasm.Section.Memory;

namespace Xui.Compiler.WASM.Wasm.Section.Table;

/// <summary>Writes fixed-size nullable function tables used for indirect dispatch.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;
    internal Writer(Section.Writer section) { this.section = section; countPosition = section.ReserveU32(); }
    /// <summary>Defines a table with equal minimum and maximum sizes, initially null.</summary>
    public TableIndex Functions(uint size)
    {
        section.Byte((byte)ValueTypeCode.FuncRef).Byte((byte)LimitsKind.MinimumAndMaximum).U32(size).U32(size);
        return new(count++);
    }
    /// <summary>Patches the table count and section length.</summary>
    public void Dispose() { section.PatchU32(countPosition, count); section.Dispose(); }
}
