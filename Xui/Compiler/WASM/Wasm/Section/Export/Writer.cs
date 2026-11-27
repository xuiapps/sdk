using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.Export;

/// <summary>Writes named exports and counts the entries automatically.</summary>
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

    /// <summary>Exports a function by its module function index.</summary>
    public void Function(string name, FunctionIndex functionIndex) => Export(name, ExportKind.Function, functionIndex.Value);

    /// <summary>Exports linear memory for explicit host interop.</summary>
    public void Memory(string name, MemoryIndex memoryIndex) => Export(name, ExportKind.Memory, memoryIndex.Value);

    /// <summary>Exports a global by its module global index.</summary>
    public void Global(string name, GlobalIndex globalIndex) => Export(name, ExportKind.Global, globalIndex.Value);

    /// <summary>Exports a definition by its name, kind, and index in that kind's index space.</summary>
    private void Export(string name, ExportKind kind, uint index)
    {
        section.Name(name).Byte((byte)kind).U32(index);
        count++;
    }

    /// <summary>Patches the export count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
