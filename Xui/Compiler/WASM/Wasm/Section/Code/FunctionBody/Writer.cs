using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

/// <summary>Writes a function's instructions, final end opcode, and body length.</summary>
/// <remarks>Instruction methods return this writer for chaining. Close nested blocks before disposing the body.</remarks>
public ref partial struct Writer
{
    private Section.Writer body;
    private bool disposed;

    internal Writer(Section.Writer body, scoped ReadOnlySpan<ValueTypeCode> locals)
    {
        this.body = body;
        uint groups = 0;
        for (int i = 0; i < locals.Length; i++)
            if (i == 0 || locals[i] != locals[i - 1]) groups++;
        body.U32(groups);
        for (int first = 0; first < locals.Length;)
        {
            int end = first + 1;
            while (end < locals.Length && locals[end] == locals[first]) end++;
            body.U32((uint)(end - first)).Byte((byte)locals[first]);
            first = end;
        }
    }

    /// <summary>Pushes a 32-bit integer constant.</summary>
    public readonly Writer I32Const(int value)
    {
        body.Instruction(OpCode.I32Const).Signed(value);
        return this;
    }

    /// <summary>Pushes a 32-bit floating-point constant.</summary>
    public readonly Writer F32Const(float value)
    {
        body.Instruction(OpCode.F32Const).F32(value);
        return this;
    }

    /// <summary>Pushes a 64-bit floating-point constant.</summary>
    public readonly Writer F64Const(double value)
    {
        body.Instruction(OpCode.F64Const).F64(value);
        return this;
    }

    /// <summary>Adds two i32 values with wrapping arithmetic.</summary>
    public readonly Writer I32Add()
    {
        body.Instruction(OpCode.I32Add);
        return this;
    }

    /// <summary>Computes the square root at f32 precision, preserving signed zero.</summary>
    public readonly Writer F32Sqrt() { body.Instruction(OpCode.F32Sqrt); return this; }

    /// <summary>Computes the square root at f64 precision, preserving signed zero.</summary>
    public readonly Writer F64Sqrt() { body.Instruction(OpCode.F64Sqrt); return this; }

    /// <summary>Adds two f64 values.</summary>
    public readonly Writer F64Add()
    {
        body.Instruction(OpCode.F64Add);
        return this;
    }

    /// <summary>Rounds the top f64 value to f32.</summary>
    public readonly Writer F32DemoteF64()
    {
        body.Instruction(OpCode.F32DemoteF64);
        return this;
    }

    /// <summary>Promotes the top f32 value to f64.</summary>
    public readonly Writer F64PromoteF32()
    {
        body.Instruction(OpCode.F64PromoteF32);
        return this;
    }

    /// <summary>Selects the first numeric value when the condition is nonzero, otherwise the second.</summary>
    public readonly Writer Select() { body.Instruction(OpCode.Select); return this; }

    /// <summary>Discards the top value.</summary>
    public readonly Writer Drop()
    {
        body.Instruction(OpCode.Drop);
        return this;
    }

    /// <summary>Pushes a signed LEB128-encoded i64 constant.</summary>
    public readonly Writer I64Const(long value)
    {
        body.Instruction(OpCode.I64Const).Signed(value);
        return this;
    }

    /// <summary>Tests an i32 value for zero.</summary>
    public readonly Writer I32Eqz()
    {
        body.Instruction(OpCode.I32Eqz);
        return this;
    }

    /// <summary>Compares i32 values for equality.</summary>
    public readonly Writer I32Eq()
    {
        body.Instruction(OpCode.I32Eq);
        return this;
    }

    /// <summary>Compares signed i32 values for less than.</summary>
    public readonly Writer I32LtS()
    {
        body.Instruction(OpCode.I32LtS);
        return this;
    }

    /// <summary>Compares unsigned i32 values for less than.</summary>
    public readonly Writer I32LtU()
    {
        body.Instruction(OpCode.I32LtU);
        return this;
    }

    /// <summary>Compares signed i32 values for greater than.</summary>
    public readonly Writer I32GtS()
    {
        body.Instruction(OpCode.I32GtS);
        return this;
    }

    /// <summary>Compares unsigned i32 values for greater than.</summary>
    public readonly Writer I32GtU()
    {
        body.Instruction(OpCode.I32GtU);
        return this;
    }

    /// <summary>Compares i64 values for equality.</summary>
    public readonly Writer I64Eq()
    {
        body.Instruction(OpCode.I64Eq);
        return this;
    }

    /// <summary>Compares signed i64 values for less than.</summary>
    public readonly Writer I64LtS()
    {
        body.Instruction(OpCode.I64LtS);
        return this;
    }

    /// <summary>Compares unsigned i64 values for less than.</summary>
    public readonly Writer I64LtU()
    {
        body.Instruction(OpCode.I64LtU);
        return this;
    }

    /// <summary>Compares signed i64 values for greater than.</summary>
    public readonly Writer I64GtS()
    {
        body.Instruction(OpCode.I64GtS);
        return this;
    }

    /// <summary>Compares unsigned i64 values for greater than.</summary>
    public readonly Writer I64GtU()
    {
        body.Instruction(OpCode.I64GtU);
        return this;
    }

    /// <summary>Compares f64 values for equality; NaN compares false.</summary>
    public readonly Writer F64Eq()
    {
        body.Instruction(OpCode.F64Eq);
        return this;
    }

    /// <summary>Compares f64 values for less than; NaN compares false.</summary>
    public readonly Writer F64Lt()
    {
        body.Instruction(OpCode.F64Lt);
        return this;
    }

    /// <summary>Compares f64 values for greater than; NaN compares false.</summary>
    public readonly Writer F64Gt()
    {
        body.Instruction(OpCode.F64Gt);
        return this;
    }

    /// <summary>Compares f64 values for less than or equal; NaN compares false.</summary>
    public readonly Writer F64Le()
    {
        body.Instruction(OpCode.F64Le);
        return this;
    }

    /// <summary>Compares f64 values for greater than or equal; NaN compares false.</summary>
    public readonly Writer F64Ge()
    {
        body.Instruction(OpCode.F64Ge);
        return this;
    }

    /// <summary>Subtracts i32 values with wrapping arithmetic.</summary>
    public readonly Writer I32Sub()
    {
        body.Instruction(OpCode.I32Sub);
        return this;
    }

    /// <summary>Multiplies i32 values with wrapping arithmetic.</summary>
    public readonly Writer I32Mul()
    {
        body.Instruction(OpCode.I32Mul);
        return this;
    }

    /// <summary>Computes bitwise AND of i32 values.</summary>
    public readonly Writer I32And()
    {
        body.Instruction(OpCode.I32And);
        return this;
    }

    /// <summary>Computes bitwise OR of i32 values.</summary>
    public readonly Writer I32Or()
    {
        body.Instruction(OpCode.I32Or);
        return this;
    }

    /// <summary>Computes bitwise XOR of i32 values.</summary>
    public readonly Writer I32Xor()
    {
        body.Instruction(OpCode.I32Xor);
        return this;
    }

    /// <summary>Shifts an i32 left, masking the count to five bits.</summary>
    public readonly Writer I32Shl()
    {
        body.Instruction(OpCode.I32Shl);
        return this;
    }

    /// <summary>Shifts a signed i32 right, masking the count to five bits.</summary>
    public readonly Writer I32ShrS()
    {
        body.Instruction(OpCode.I32ShrS);
        return this;
    }

    /// <summary>Shifts an unsigned i32 right, masking the count to five bits.</summary>
    public readonly Writer I32ShrU()
    {
        body.Instruction(OpCode.I32ShrU);
        return this;
    }

    /// <summary>Adds i64 values with wrapping arithmetic.</summary>
    public readonly Writer I64Add()
    {
        body.Instruction(OpCode.I64Add);
        return this;
    }

    /// <summary>Subtracts i64 values with wrapping arithmetic.</summary>
    public readonly Writer I64Sub()
    {
        body.Instruction(OpCode.I64Sub);
        return this;
    }

    /// <summary>Multiplies i64 values with wrapping arithmetic.</summary>
    public readonly Writer I64Mul()
    {
        body.Instruction(OpCode.I64Mul);
        return this;
    }

    /// <summary>Computes bitwise AND of i64 values.</summary>
    public readonly Writer I64And()
    {
        body.Instruction(OpCode.I64And);
        return this;
    }

    /// <summary>Computes bitwise OR of i64 values.</summary>
    public readonly Writer I64Or()
    {
        body.Instruction(OpCode.I64Or);
        return this;
    }

    /// <summary>Computes bitwise XOR of i64 values.</summary>
    public readonly Writer I64Xor()
    {
        body.Instruction(OpCode.I64Xor);
        return this;
    }

    /// <summary>Shifts an i64 left, masking the i64 count to six bits.</summary>
    public readonly Writer I64Shl()
    {
        body.Instruction(OpCode.I64Shl);
        return this;
    }

    /// <summary>Shifts a signed i64 right, masking the i64 count to six bits.</summary>
    public readonly Writer I64ShrS()
    {
        body.Instruction(OpCode.I64ShrS);
        return this;
    }

    /// <summary>Shifts an unsigned i64 right, masking the i64 count to six bits.</summary>
    public readonly Writer I64ShrU()
    {
        body.Instruction(OpCode.I64ShrU);
        return this;
    }

    /// <summary>Negates an f64 value, preserving signed zero.</summary>
    public readonly Writer F64Neg()
    {
        body.Instruction(OpCode.F64Neg);
        return this;
    }

    /// <summary>Subtracts f64 values.</summary>
    public readonly Writer F64Sub()
    {
        body.Instruction(OpCode.F64Sub);
        return this;
    }

    /// <summary>Multiplies f64 values.</summary>
    public readonly Writer F64Mul()
    {
        body.Instruction(OpCode.F64Mul);
        return this;
    }

    /// <summary>Divides two floating evaluation-stack values.</summary>
    public readonly Writer F64Div() { body.Instruction(OpCode.F64Div); return this; }

    /// <summary>Reinterprets f32 bits as i32 without numeric conversion.</summary>
    public readonly Writer I32ReinterpretF32()
    {
        body.Instruction(OpCode.I32ReinterpretF32);
        return this;
    }

    /// <summary>Reinterprets f64 bits as i64 without numeric conversion.</summary>
    public readonly Writer I64ReinterpretF64()
    {
        body.Instruction(OpCode.I64ReinterpretF64);
        return this;
    }

    /// <summary>Keeps the low 32 bits of an i64 value.</summary>
    public readonly Writer I32WrapI64()
    {
        body.Instruction(OpCode.I32WrapI64);
        return this;
    }

    /// <summary>Sign-extends i32 to i64.</summary>
    public readonly Writer I64ExtendI32S()
    {
        body.Instruction(OpCode.I64ExtendI32S);
        return this;
    }

    /// <summary>Zero-extends i32 to i64.</summary>
    public readonly Writer I64ExtendI32U()
    {
        body.Instruction(OpCode.I64ExtendI32U);
        return this;
    }

    /// <summary>Truncates f64 to signed i32; overflow saturates and NaN becomes zero without trapping.</summary>
    public readonly Writer I32TruncSatF64S()
    {
        body.Instruction(OpCode.Misc).U32((uint)SaturatingConversionOpCode.I32TruncSatF64S);
        return this;
    }

    /// <summary>Truncates a floating-point value to a signed 64-bit integer with saturation.</summary>
    public readonly Writer I64TruncSatF64S()
    {
        body.Instruction(OpCode.Misc).U32((uint)SaturatingConversionOpCode.I64TruncSatF64S);
        return this;
    }

    /// <summary>Truncates f64 toward zero to signed i64; callers must guard NaN and out-of-range values to avoid traps.</summary>
    public readonly Writer I64TruncF64S() { body.Instruction(OpCode.I64TruncF64S); return this; }

    /// <summary>Converts signed i32 to f32.</summary>
    public readonly Writer F32ConvertI32S()
    {
        body.Instruction(OpCode.F32ConvertI32S);
        return this;
    }

    /// <summary>Converts signed i64 directly to f32.</summary>
    public readonly Writer F32ConvertI64S()
    {
        body.Instruction(OpCode.F32ConvertI64S);
        return this;
    }

    /// <summary>Converts unsigned i32 directly to f32.</summary>
    public readonly Writer F32ConvertI32U() { body.Instruction(OpCode.F32ConvertI32U); return this; }

    /// <summary>Converts unsigned i64 directly to f32, with a single rounding.</summary>
    public readonly Writer F32ConvertI64U() { body.Instruction(OpCode.F32ConvertI64U); return this; }

    /// <summary>Converts signed i32 to f64.</summary>
    public readonly Writer F64ConvertI32S()
    {
        body.Instruction(OpCode.F64ConvertI32S);
        return this;
    }

    /// <summary>Converts unsigned i32 to f64.</summary>
    public readonly Writer F64ConvertI32U()
    {
        body.Instruction(OpCode.F64ConvertI32U);
        return this;
    }

    /// <summary>Converts signed i64 to f64.</summary>
    public readonly Writer F64ConvertI64S()
    {
        body.Instruction(OpCode.F64ConvertI64S);
        return this;
    }

    /// <summary>Converts unsigned i64 to f64.</summary>
    public readonly Writer F64ConvertI64U()
    {
        body.Instruction(OpCode.F64ConvertI64U);
        return this;
    }

    /// <summary>Sign-extends the low eight bits to i32.</summary>
    public readonly Writer I32Extend8S()
    {
        body.Instruction(OpCode.I32Extend8S);
        return this;
    }

    /// <summary>Sign-extends the low sixteen bits to i32.</summary>
    public readonly Writer I32Extend16S()
    {
        body.Instruction(OpCode.I32Extend16S);
        return this;
    }

    /// <summary>Calls a function by its module index.</summary>
    public readonly Writer Call(FunctionIndex function)
    {
        body.Instruction(OpCode.Call).U32(function.Value);
        return this;
    }

    /// <summary>Loads a parameter or local by its function index.</summary>
    public readonly Writer LocalGet(LocalIndex local)
    {
        body.Instruction(OpCode.LocalGet).U32(local.Value);
        return this;
    }

    /// <summary>Stores the top value in a parameter or local by its function index.</summary>
    public readonly Writer LocalSet(LocalIndex local)
    {
        body.Instruction(OpCode.LocalSet).U32(local.Value);
        return this;
    }

    /// <summary>Stores the top value in a parameter or local without consuming it.</summary>
    public readonly Writer LocalTee(LocalIndex local)
    {
        body.Instruction(OpCode.LocalTee).U32(local.Value);
        return this;
    }

    /// <summary>Loads a global by its module index.</summary>
    public readonly Writer GlobalGet(GlobalIndex global)
    {
        body.Instruction(OpCode.GlobalGet).U32(global.Value);
        return this;
    }

    /// <summary>Stores a global by its module index.</summary>
    public readonly Writer GlobalSet(GlobalIndex global)
    {
        body.Instruction(OpCode.GlobalSet).U32(global.Value);
        return this;
    }

    /// <summary>Creates an array from a data segment using the byte offset and element count on the stack.</summary>
    public readonly Writer ArrayNewData(TypeIndex type, DataIndex data)
    {
        body.Instruction(GcOpCode.ArrayNewData).U32(type.Value).U32(data.Value);
        return this;
    }

    /// <summary>Tests whether the reference on the stack is null.</summary>
    public readonly Writer RefIsNull()
    {
        body.Instruction(OpCode.RefIsNull);
        return this;
    }

    /// <summary>Begins a conditional block with no parameters or results, consuming an i32 condition.</summary>
    public readonly Writer If()
    {
        body.Instruction(OpCode.If).Byte((byte)BlockTypeCode.Empty);
        return this;
    }

    /// <summary>Begins a typed block; branches to its label pass the signature's results.</summary>
    public readonly Writer Block(TypeIndex signature)
    {
        body.Instruction(OpCode.Block).Signed(signature.Value);
        return this;
    }

    /// <summary>Begins a typed loop; branches to its label pass the signature's parameters.</summary>
    public readonly Writer Loop(TypeIndex signature)
    {
        body.Instruction(OpCode.Loop).Signed(signature.Value);
        return this;
    }

    /// <summary>Passes label arguments and branches to the enclosing label at this depth.</summary>
    public readonly Writer Br(LabelIndex label)
    {
        body.Instruction(OpCode.Br).U32(label.Value);
        return this;
    }

    /// <summary>Begins a conditional with parameters and results from a module type definition.</summary>
    public readonly Writer If(TypeIndex signature)
    {
        body.Instruction(OpCode.If).Signed(signature.Value);
        return this;
    }

    /// <summary>Begins the alternative arm of the current conditional.</summary>
    public readonly Writer Else()
    {
        body.Instruction(OpCode.Else);
        return this;
    }

    /// <summary>Marks an unreachable continuation after terminating arms.</summary>
    public readonly Writer Unreachable()
    {
        body.Instruction(OpCode.Unreachable);
        return this;
    }

    /// <summary>Tests an i64 for zero.</summary>
    public readonly Writer I64Eqz()
    {
        body.Instruction(OpCode.I64Eqz);
        return this;
    }

    /// <summary>Ends a nested block. The function's final end is written automatically on disposal.</summary>
    public readonly Writer End()
    {
        body.Instruction(OpCode.End);
        return this;
    }

    /// <summary>Returns from the current function.</summary>
    public readonly Writer Return()
    {
        body.Instruction(OpCode.Return);
        return this;
    }

    /// <summary>Writes the function's final end opcode and patches its body length.</summary>
    public void Dispose()
    {
        if (disposed) return;
        body.Instruction(OpCode.End);
        body.Dispose();
        disposed = true;
    }
}
