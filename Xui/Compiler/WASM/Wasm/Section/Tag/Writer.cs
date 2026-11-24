using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.Tag;

/// <summary>Writes exception tags; each signature describes its payload and has no results.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;
    internal Writer(Section.Writer section) { this.section = section; countPosition = section.ReserveU32(); }
    /// <summary>Defines a managed exception tag with the supplied payload signature.</summary>
    public TagIndex Exception(TypeIndex signature) { section.Byte(0).U32(signature.Value); return new(count++); }
    /// <summary>Completes the tag vector and section.</summary>
    public void Dispose() { section.PatchU32(countPosition, count); section.Dispose(); }
}
