using System.Runtime.CompilerServices;
using Xui.Compiler.WASM.Wasm.Section;

namespace Xui.Compiler.WASM.Wasm;

/// <summary>
/// Writes WebAssembly binary encodings to a stream to compose a module's <c>.wasm</c> blob.
/// </summary>
/// <remarks>
/// The caller controls the module header and section order. Section payloads are
/// written through disposable <see cref="Section.Writer"/> scopes.
/// Header methods return this writer to support chaining.
/// The caller owns the stream and is responsible for flushing and disposing it.
/// </remarks>
/// <param name="stream">The destination stream for the encoded bytes.</param>
public sealed class ModuleWriter(Stream stream)
{
    /// <summary>Writes the four-byte WebAssembly magic signature.</summary>
    /// <returns>This writer for chaining with <see cref="V1"/>.</returns>
    public ModuleWriter Header() => Bytes([0, 0x61, 0x73, 0x6d]);

    /// <summary>Writes binary format version 1 as four little-endian bytes.</summary>
    /// <returns>This writer for chaining.</returns>
    public ModuleWriter V1() => Bytes([1, 0, 0, 0]);

    /// <summary>Begins the module's type definitions section.</summary>
    /// <returns>A specialized writer scope to dispose before beginning the next section.</returns>
    public Section.TypeDefinitions.Writer TypeDefinitionsSection() => new(Section(SectionId.Type));

    /// <summary>Begins the module's import section.</summary>
    /// <returns>A specialized writer scope to dispose before beginning the next section.</returns>
    public Section.Import.Writer ImportSection() => new(Section(SectionId.Import));

    /// <summary>Begins function declarations, with indices following the imported functions.</summary>
    public Section.Function.Writer FunctionSection(uint importedFunctionCount = 0) =>
        new(Section(SectionId.Function), importedFunctionCount);

    /// <summary>Begins reference table definitions.</summary>
    public Section.Table.Writer TableSection() => new(Section(SectionId.Table));

    /// <summary>Begins active function-table segments.</summary>
    public Section.Element.Writer ElementSection() => new(Section(SectionId.Element));

    /// <summary>Begins linear memory definitions.</summary>
    public Section.Memory.Writer MemorySection() => new(Section(SectionId.Memory));

    /// <summary>Begins global definitions, with indices following the imported globals.</summary>
    public Section.Global.Writer GlobalSection(uint importedGlobalCount = 0) =>
        new(Section(SectionId.Global), importedGlobalCount);

    /// <summary>Begins the module's named exports.</summary>
    public Section.Export.Writer ExportSection() => new(Section(SectionId.Export));

    /// <summary>Writes the complete data segment count section before the code section.</summary>
    /// <param name="count">The number of segments in the later data section.</param>
    /// <returns>This writer for chaining.</returns>
    public ModuleWriter DataCount(uint count)
    {
        using var section = Section(SectionId.DataCount);
        section.U32(count);
        return this;
    }

    /// <summary>Begins the module's function bodies.</summary>
    public Section.Code.Writer CodeSection() => new(Section(SectionId.Code));

    /// <summary>Begins the module's data segments, following the code section.</summary>
    public Section.Data.Writer DataSection() => new(Section(SectionId.Data));

    /// <summary>Begins a section whose payload length is patched when its scope is disposed.</summary>
    /// <param name="id">The section's binary identifier.</param>
    /// <returns>A writer scope to dispose before beginning the next section.</returns>
    /// <remarks>The underlying stream must support seeking. The scope leaves it open.</remarks>
    public Section.Writer Section(SectionId id)
    {
        stream.WriteByte((byte)id);
        return new Section.Writer(stream);
    }

    /// <summary>Writes raw bytes without a length prefix.</summary>
    /// <param name="value">The bytes to write.</param>
    /// <returns>This writer for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ModuleWriter Bytes(ReadOnlySpan<byte> value)
    {
        stream.Write(value);
        return this;
    }
}
