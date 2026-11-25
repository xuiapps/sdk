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

    /// <summary>Defines an immutable reference to a zero-initialized GC struct with mutable fields.</summary>
    public GlobalIndex DefaultStruct(TypeIndex type)
    {
        section.Byte((byte)ValueTypeCode.EqRef).Byte((byte)Mutability.Immutable)
            .Instruction(GcOpCode.StructNewDefault).U32(type.Value).Instruction(OpCode.End);
        return new(importedGlobalCount + count++);
    }

    /// <summary>Defines an immutable eqref global containing an i32 GC array.</summary>
    public GlobalIndex I32Array(TypeIndex arrayType, scoped ReadOnlySpan<int> values)
    {
        section.Byte((byte)ValueTypeCode.EqRef).Byte((byte)Mutability.Immutable);
        foreach (int value in values) section.Instruction(OpCode.I32Const).Signed(value);
        section.Instruction(GcOpCode.ArrayNewFixed).U32(arrayType.Value).U32((uint)values.Length).Instruction(OpCode.End);
        return new GlobalIndex(importedGlobalCount + count++);
    }

    /// <summary>Number of i32 entries in each chunk of a large constant array.</summary>
    public const int I32ChunkSize = 4096;

    /// <summary>Defines an immutable eqref array of i32-array chunks, avoiding large array.new_fixed operands.</summary>
    public GlobalIndex I32ArrayChunks(TypeIndex chunksType, TypeIndex elementType, scoped ReadOnlySpan<int> values)
    {
        section.Byte((byte)ValueTypeCode.EqRef).Byte((byte)Mutability.Immutable);
        for (int start = 0; start < values.Length; start += I32ChunkSize)
        {
            var chunk = values.Slice(start, Math.Min(I32ChunkSize, values.Length - start));
            foreach (int value in chunk) section.Instruction(OpCode.I32Const).Signed(value);
            section.Instruction(GcOpCode.ArrayNewFixed).U32(elementType.Value).U32((uint)chunk.Length);
        }
        section.Instruction(GcOpCode.ArrayNewFixed).U32(chunksType.Value)
            .U32((uint)((values.Length + I32ChunkSize - 1) / I32ChunkSize)).Instruction(OpCode.End);
        return new(importedGlobalCount + count++);
    }

    /// <summary>Patches the global count and section length.</summary>
    public void Dispose()
    {
        section.PatchU32(countPosition, count);
        section.Dispose();
    }
}
