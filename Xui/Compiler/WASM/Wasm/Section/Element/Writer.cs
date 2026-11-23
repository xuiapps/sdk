using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Element;

/// <summary>Initializes ranges of the default function table at module instantiation.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;
    internal Writer(Section.Writer section) { this.section = section; countPosition = section.ReserveU32(); }
    /// <summary>Writes function indices at a constant offset in table zero.</summary>
    public void Functions(uint offset, scoped ReadOnlySpan<FunctionIndex> functions)
    {
        section.U32((uint)SegmentKind.ActiveFunctions).Instruction(OpCode.I32Const).Signed(unchecked((int)offset))
            .Instruction(OpCode.End).U32((uint)functions.Length);
        foreach (var function in functions) section.U32(function.Value);
        count++;
    }
    /// <summary>Patches the segment count and section length.</summary>
    public void Dispose() { section.PatchU32(countPosition, count); section.Dispose(); }
}
