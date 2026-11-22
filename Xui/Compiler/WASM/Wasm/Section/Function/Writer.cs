using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.Function;

/// <summary>Writes defined functions' type indices and counts the entries automatically.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private readonly uint importedFunctionCount;
    private uint count;

    internal Writer(Section.Writer section, uint importedFunctionCount)
    {
        this.section = section;
        this.importedFunctionCount = importedFunctionCount;
        countPosition = section.ReserveU32();
    }

    /// <summary>Declares a function whose body will appear in the same order in the code section.</summary>
    /// <returns>The function index, including the preceding imported functions.</returns>
    public FunctionIndex Function(TypeIndex typeIndex)
    {
        section.U32(typeIndex.Value);
        return new FunctionIndex(importedFunctionCount + count++);
    }

    /// <summary>Patches the function count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
