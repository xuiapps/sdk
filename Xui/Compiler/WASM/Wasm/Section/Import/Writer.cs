using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Import;

/// <summary>Writes function and global imports, tracking their count and indices.</summary>
/// <remarks>
/// Function and global indices are counted separately. Disposal patches the total
/// import count and section length and leaves the module stream open.
/// </remarks>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private uint count;
    private uint functionCount;
    private uint globalCount;

    /// <summary>The number of imported functions preceding defined functions.</summary>
    public readonly uint FunctionCount => functionCount;

    internal Writer(Section.Writer section)
    {
        this.section = section;
        countPosition = section.ReserveU32();
    }

    /// <summary>Imports a function with a previously declared signature.</summary>
    /// <param name="module">The host module name.</param>
    /// <param name="name">The function name within that module.</param>
    /// <param name="typeIndex">The index of the function's signature in the type section.</param>
    /// <returns>The imported function's index in the module's function index space.</returns>
    public FunctionIndex Function(string module, string name, TypeIndex typeIndex)
    {
        section
            .Name(module)
            .Name(name)
            .Byte((byte)ImportKind.Function)
            .U32(typeIndex.Value);
        count++;
        return new FunctionIndex(functionCount++);
    }

    /// <summary>Imports a global value, immutable by default.</summary>
    /// <param name="module">The host module name.</param>
    /// <param name="name">The global name within that module.</param>
    /// <param name="type">The global's value type.</param>
    /// <param name="mutability">Whether the global's value can be reassigned.</param>
    /// <returns>The imported global's index in the module's global index space.</returns>
    public GlobalIndex Global(string module, string name, ValueTypeCode type, Mutability mutability = Mutability.Immutable)
    {
        section
            .Name(module)
            .Name(name)
            .Byte((byte)ImportKind.Global)
            .Byte((byte)type).Byte((byte)mutability);
        count++;
        return new GlobalIndex(globalCount++);
    }

    /// <summary>Completes the import section and patches its import count and payload length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
