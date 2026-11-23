using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Loads four bytes as i32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Load(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Load).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads eight bytes as i64. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I64Load(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I64Load).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads four bytes as f32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer F32Load(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.F32Load).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads eight bytes as f64. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer F64Load(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.F64Load).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads and sign-extends one byte. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Load8S(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Load8S).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads and zero-extends one byte. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Load8U(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Load8U).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads and sign-extends two bytes. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Load16S(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Load16S).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Loads and zero-extends two bytes. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Load16U(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Load16U).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores four bytes from i32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Store(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Store).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores eight bytes from i64. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I64Store(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I64Store).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores four bytes from f32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer F32Store(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.F32Store).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores eight bytes from f64. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer F64Store(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.F64Store).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores the low byte of i32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Store8(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Store8).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Stores the low two bytes of i32. Uses memory zero; alignment is expressed as a power-of-two exponent.</summary>
    public readonly Writer I32Store16(uint offset = 0, uint alignment = 0)
    {
        body.Instruction(OpCode.I32Store16).U32(alignment).U32(offset);
        return this;
    }

    /// <summary>Copies length bytes from source to destination addresses on the stack; overlap is supported.</summary>
    public readonly Writer MemoryCopy(MemoryIndex destination = default, MemoryIndex source = default)
    {
        body.Instruction(OpCode.Misc).U32((uint)BulkMemoryOpCode.MemoryCopy).U32(destination.Value).U32(source.Value);
        return this;
    }

    /// <summary>Fills length bytes at the destination with the low byte of the supplied value.</summary>
    public readonly Writer MemoryFill(MemoryIndex memory = default)
    {
        body.Instruction(OpCode.Misc).U32((uint)BulkMemoryOpCode.MemoryFill).U32(memory.Value);
        return this;
    }
}
