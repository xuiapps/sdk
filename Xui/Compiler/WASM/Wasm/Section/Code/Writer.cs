using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Code;

/// <summary>Writes function bodies and counts them automatically.</summary>
/// <remarks>Bodies must appear in the same order as declarations in the function section.</remarks>
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

    /// <summary>Begins a function body with local types in index order; parameters are not included.</summary>
    /// <returns>A scope to dispose before writing another body or closing the code section.</returns>
    public FunctionBody.Writer FunctionBody(scoped ReadOnlySpan<ValueTypeCode> locals = default)
    {
        count++;
        return new(section.FunctionBody(), locals);
    }

    /// <summary>Patches the function body count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
