using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

/// <summary>Writes the type count and type definitions in a module's type section.</summary>
/// <remarks>Disposal patches the type count and section length and leaves the module stream open.</remarks>
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

    /// <summary>Writes a function signature with no parameters and no results.</summary>
    /// <returns>The index of the new type definition.</returns>
    public TypeIndex FunctionType() => FunctionType([]);

    /// <summary>Writes a complete function signature and includes it in the section's type count.</summary>
    /// <param name="parameters">The parameter types, in declaration order.</param>
    /// <param name="result">The result type, or null for a void signature.</param>
    /// <returns>The index of the new type definition.</returns>
    public TypeIndex FunctionType(scoped ReadOnlySpan<ValueType> parameters, ValueType? result = null)
        => FunctionType(parameters, result is { } value ? [value] : []);

    /// <summary>Writes a function or block signature with any number of parameters and results.</summary>
    public TypeIndex FunctionType(scoped ReadOnlySpan<ValueType> parameters, scoped ReadOnlySpan<ValueType> results)
    {
        var index = new TypeIndex(count++);
        section.Byte((byte)TypeDefinitionCode.Function).U32((uint)parameters.Length);
        foreach (var parameter in parameters) parameter.WriteTo(section);
        section.U32((uint)results.Length);
        foreach (var result in results) result.WriteTo(section);
        return index;
    }

    /// <summary>Defines a GC array of packed integers in its own implicit recursive group.</summary>
    /// <param name="elementType">The packed storage type of each element.</param>
    /// <param name="mutability">Whether array elements can be modified.</param>
    /// <returns>The index of the new type definition.</returns>
    public TypeIndex ArrayType(PackedTypeCode elementType, Mutability mutability = Mutability.Mutable)
    {
        var index = new TypeIndex(count++);
        section.Byte((byte)TypeDefinitionCode.Array).Byte((byte)elementType).Byte((byte)mutability);
        return index;
    }

    /// <summary>Defines a GC array of scalar or reference elements.</summary>
    public TypeIndex ArrayType(ValueType elementType, Mutability mutability = Mutability.Mutable)
    {
        var index = new TypeIndex(count++);
        section.Byte((byte)TypeDefinitionCode.Array);
        elementType.WriteTo(section);
        section.Byte((byte)mutability);
        return index;
    }

    /// <summary>Defines a GC struct, optionally as an extensible type or a subtype with an inherited field prefix.</summary>
    /// <param name="fields">The complete field sequence, including all inherited fields.</param>
    /// <param name="mutability">Mutability shared by the supplied fields.</param>
    /// <param name="baseType">An earlier extensible struct type, or null for no declared supertype.</param>
    /// <param name="extensible">Whether subsequent struct types may derive from this definition.</param>
    public TypeIndex StructType(scoped ReadOnlySpan<ValueType> fields, Mutability mutability = Mutability.Mutable, TypeIndex? baseType = null, bool extensible = false)
    {
        var index = new TypeIndex(count++);
        if (extensible || baseType.HasValue)
        {
            section.Byte((byte)(extensible ? TypeDefinitionCode.Subtype : TypeDefinitionCode.FinalSubtype)).U32(baseType.HasValue ? 1u : 0u);
            if (baseType is { } parent) section.U32(parent.Value);
        }
        section.Byte((byte)TypeDefinitionCode.Struct).U32((uint)fields.Length);
        foreach (var field in fields) { field.WriteTo(section); section.Byte((byte)mutability); }
        return index;
    }

    /// <summary>Completes the type section and patches its type count and payload length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
