using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Global;

/// <summary>Writes global definitions with constant initializers and counts the entries.</summary>
public ref struct Writer
{
    private Section.Writer section;
    private readonly long countPosition;
    private readonly uint importedGlobalCount;
    private uint count;

    internal Writer(Section.Writer section, uint importedGlobalCount)
    {
        this.section = section;
        this.importedGlobalCount = importedGlobalCount;
        countPosition = section.ReserveU32();
    }

    /// <summary>Defines a nullable reference global initialized to null.</summary>
    /// <param name="type">The shorthand reference type: ExternRef or FuncRef.</param>
    /// <param name="mutability">Whether the global can be reassigned.</param>
    /// <returns>The global index, including the preceding imported globals.</returns>
    public GlobalIndex RefNull(ValueTypeCode type, Mutability mutability = Mutability.Immutable)
    {
        section.Byte((byte)type).Byte((byte)mutability)
            .Instruction(OpCode.RefNull).Byte((byte)type)
            .Instruction(OpCode.End);
        return new GlobalIndex(importedGlobalCount + count++);
    }

    /// <summary>Defines an i32 global initialized to a constant.</summary>
    public GlobalIndex I32(int value, Mutability mutability = Mutability.Immutable)
    {
        section.Byte((byte)ValueTypeCode.I32).Byte((byte)mutability)
            .Instruction(OpCode.I32Const).Signed(value).Instruction(OpCode.End);
        return new GlobalIndex(importedGlobalCount + count++);
    }

    /// <summary>Begins an immutable eqref global initialized by a constant GC struct expression.</summary>
    public StructInitializer Struct(TypeIndex type)
    {
        section.Byte((byte)ValueTypeCode.EqRef).Byte((byte)Mutability.Immutable);
        return new(section, type, new GlobalIndex(importedGlobalCount + count++));
    }

    /// <summary>Defines an immutable eqref global containing an i32 GC array.</summary>
    public GlobalIndex I32Array(TypeIndex arrayType, scoped ReadOnlySpan<int> values)
    {
        section.Byte((byte)ValueTypeCode.EqRef).Byte((byte)Mutability.Immutable);
        foreach (int value in values) section.Instruction(OpCode.I32Const).Signed(value);
        section.Instruction(GcOpCode.ArrayNewFixed).U32(arrayType.Value).U32((uint)values.Length).Instruction(OpCode.End);
        return new GlobalIndex(importedGlobalCount + count++);
    }

    /// <summary>Patches the global count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
