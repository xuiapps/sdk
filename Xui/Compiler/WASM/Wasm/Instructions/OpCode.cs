namespace Xui.Compiler.WASM.Wasm.Instructions;

/// <summary>Single-byte instruction codes currently emitted by the compiler.</summary>
public enum OpCode : byte
{
    /// <summary>Reads a reference table entry.</summary>
    TableGet = 0x25,
    /// <summary>Writes a reference table entry.</summary>
    TableSet = 0x26,
    /// <summary>Begins a structured exception handler table.</summary>
    TryTable = 0x1f,
    /// <summary>Throws a tagged exception payload.</summary>
    Throw = 0x08,
    /// <summary>Rethrows an exception reference.</summary>
    ThrowRef = 0x0a,
    /// <summary>Begins a block whose label receives its results.</summary>
    Block = 0x02,
    /// <summary>Begins a loop whose label receives its parameters.</summary>
    Loop = 0x03,
    /// <summary>Branches to an enclosing control label.</summary>
    Br = 0x0c,

    /// <summary>Branches to a label when the i32 condition is nonzero.</summary>
    BrIf = 0x0d,
    /// <summary>Branches to an indexed label, with an out-of-range default.</summary>
    BrTable = 0x0e,
    /// <summary>Begins a conditional block.</summary>
    If = 0x04,
    /// <summary>Begins the alternative arm of a conditional.</summary>
    Else = 0x05,
    /// <summary>Marks a point that cannot be reached.</summary>
    Unreachable = 0x00,
    /// <summary>Tests an i64 for zero.</summary>
    I64Eqz = 0x50,
    /// <summary>Ends a block, function body, or initializer expression.</summary>
    End = 0x0b,
    /// <summary>Returns from the current function.</summary>
    Return = 0x0f,
    /// <summary>Calls a function by index.</summary>
    Call = 0x10,

    /// <summary>Calls a function-table entry with a runtime signature check.</summary>
    CallIndirect = 0x11,
    /// <summary>Discards the top value.</summary>
    Drop = 0x1a,
    /// <summary>Selects one of two numeric values using an i32 condition.</summary>
    Select = 0x1b,
    /// <summary>Loads a parameter or local by index.</summary>
    LocalGet = 0x20,
    /// <summary>Stores a parameter or local by index.</summary>
    LocalSet = 0x21,
    /// <summary>Stores the top value in a local and leaves it on the stack.</summary>
    LocalTee = 0x22,
    /// <summary>Loads a global by index.</summary>
    GlobalGet = 0x23,
    /// <summary>Stores a global by index.</summary>
    GlobalSet = 0x24,
    /// <summary>Pushes a signed LEB128-encoded i32 constant.</summary>
    I32Const = 0x41,
    /// <summary>Pushes a signed LEB128-encoded i64 constant.</summary>
    I64Const = 0x42,
    /// <summary>Pushes an IEEE 754 binary32 constant.</summary>
    F32Const = 0x43,
    /// <summary>Pushes an IEEE 754 binary64 constant.</summary>
    F64Const = 0x44,
    /// <summary>Tests an i32 value for zero.</summary>
    I32Eqz = 0x45,
    /// <summary>Compares i32 values for equality.</summary>
    I32Eq = 0x46,
    /// <summary>Compares signed i32 values for less than.</summary>
    I32LtS = 0x48,
    /// <summary>Compares unsigned i32 values for less than.</summary>
    I32LtU = 0x49,
    /// <summary>Compares signed i32 values for greater than.</summary>
    I32GtS = 0x4a,
    /// <summary>Compares unsigned i32 values for greater than.</summary>
    I32GtU = 0x4b,
    /// <summary>Compares i64 values for equality.</summary>
    I64Eq = 0x51,
    /// <summary>Compares signed i64 values for less than.</summary>
    I64LtS = 0x53,
    /// <summary>Compares unsigned i64 values for less than.</summary>
    I64LtU = 0x54,
    /// <summary>Compares signed i64 values for greater than.</summary>
    I64GtS = 0x55,
    /// <summary>Compares unsigned i64 values for greater than.</summary>
    I64GtU = 0x56,
    /// <summary>Compares f64 values for equality; NaN compares false.</summary>
    F64Eq = 0x61,
    /// <summary>Compares f64 values for less than; NaN compares false.</summary>
    F64Lt = 0x63,
    /// <summary>Compares f64 values for greater than; NaN compares false.</summary>
    F64Gt = 0x64,
    /// <summary>Compares f64 values for less than or equal; NaN compares false.</summary>
    F64Le = 0x65,
    /// <summary>Compares f64 values for greater than or equal; NaN compares false.</summary>
    F64Ge = 0x66,
    /// <summary>Adds two i32 values with wrapping arithmetic.</summary>
    I32Add = 0x6a,
    /// <summary>Subtracts i32 values with wrapping arithmetic.</summary>
    I32Sub = 0x6b,
    /// <summary>Multiplies i32 values with wrapping arithmetic.</summary>
    I32Mul = 0x6c,
    /// <summary>Signed i32 division; zero and overflow trap.</summary>
    I32DivS = 0x6d,
    /// <summary>Unsigned i32 division; zero traps.</summary>
    I32DivU = 0x6e,
    /// <summary>Signed i32 remainder; zero traps.</summary>
    I32RemS = 0x6f,
    /// <summary>Unsigned i32 remainder; zero traps.</summary>
    I32RemU = 0x70,
    /// <summary>Computes bitwise AND of i32 values.</summary>
    I32And = 0x71,
    /// <summary>Computes bitwise OR of i32 values.</summary>
    I32Or = 0x72,
    /// <summary>Computes bitwise XOR of i32 values.</summary>
    I32Xor = 0x73,
    /// <summary>Shifts an i32 left, masking the count to five bits.</summary>
    I32Shl = 0x74,
    /// <summary>Shifts a signed i32 right, masking the count to five bits.</summary>
    I32ShrS = 0x75,
    /// <summary>Shifts an unsigned i32 right, masking the count to five bits.</summary>
    I32ShrU = 0x76,
    /// <summary>Adds i64 values with wrapping arithmetic.</summary>
    I64Add = 0x7c,
    /// <summary>Subtracts i64 values with wrapping arithmetic.</summary>
    I64Sub = 0x7d,
    /// <summary>Multiplies i64 values with wrapping arithmetic.</summary>
    I64Mul = 0x7e,
    /// <summary>Signed i64 division; zero and overflow trap.</summary>
    I64DivS = 0x7f,
    /// <summary>Unsigned 64-bit integer division.</summary>
    I64DivU = 0x80,
    /// <summary>Signed i64 remainder; zero traps.</summary>
    I64RemS = 0x81,
    /// <summary>Unsigned 64-bit integer remainder.</summary>
    I64RemU = 0x82,
    /// <summary>Computes bitwise AND of i64 values.</summary>
    I64And = 0x83,
    /// <summary>Computes bitwise OR of i64 values.</summary>
    I64Or = 0x84,
    /// <summary>Computes bitwise XOR of i64 values.</summary>
    I64Xor = 0x85,
    /// <summary>Shifts an i64 left, masking the i64 count to six bits.</summary>
    I64Shl = 0x86,
    /// <summary>Shifts a signed i64 right, masking the i64 count to six bits.</summary>
    I64ShrS = 0x87,
    /// <summary>Shifts an unsigned i64 right, masking the i64 count to six bits.</summary>
    I64ShrU = 0x88,
    /// <summary>Negates an f64 value, preserving signed zero.</summary>
    F64Neg = 0x9a,
    /// <summary>Absolute value of f32.</summary>
    F32Abs = 0x8b,
    /// <summary>Negates f32.</summary>
    F32Neg = 0x8c,
    /// <summary>IEEE minimum of f32 values.</summary>
    F32Min = 0x96,
    /// <summary>IEEE maximum of f32 values.</summary>
    F32Max = 0x97,
    /// <summary>Adds f32 values with single-precision rounding.</summary>
    F32Add = 0x92,
    /// <summary>Subtracts f32 values with single-precision rounding.</summary>
    F32Sub = 0x93,
    /// <summary>Multiplies f32 values with single-precision rounding.</summary>
    F32Mul = 0x94,
    /// <summary>Divides f32 values with IEEE 754 rounding, infinities, and NaNs.</summary>
    F32Div = 0x95,
    /// <summary>Compares f32 values for equality.</summary>
    F32Eq = 0x5b,
    /// <summary>Compares f32 values for inequality.</summary>
    F32Ne = 0x5c,
    /// <summary>Ordered f32 less-than comparison.</summary>
    F32Lt = 0x5d,
    /// <summary>Ordered f32 greater-than comparison.</summary>
    F32Gt = 0x5e,
    /// <summary>Ordered f32 less-than-or-equal comparison.</summary>
    F32Le = 0x5f,
    /// <summary>Ordered f32 greater-than-or-equal comparison.</summary>
    F32Ge = 0x60,
    /// <summary>Rounds f32 toward positive infinity.</summary>
    F32Ceil = 0x8d,
    /// <summary>Rounds f32 toward negative infinity.</summary>
    F32Floor = 0x8e,
    /// <summary>Rounds f32 toward zero.</summary>
    F32Trunc = 0x8f,
    /// <summary>Rounds f32 to nearest with ties to even.</summary>
    F32Nearest = 0x90,
    /// <summary>Rounds f64 toward positive infinity.</summary>
    F64Ceil = 0x9b,
    /// <summary>Rounds f64 toward negative infinity.</summary>
    F64Floor = 0x9c,
    /// <summary>Rounds f64 toward zero.</summary>
    F64Trunc = 0x9d,
    /// <summary>Rounds f64 to nearest with ties to even.</summary>
    F64Nearest = 0x9e,
    /// <summary>Computes the IEEE 754 square root of f32.</summary>
    F32Sqrt = 0x91,
    /// <summary>Computes the IEEE 754 square root of f64.</summary>
    F64Sqrt = 0x9f,
    /// <summary>Clears the f64 sign bit.</summary>
    F64Abs = 0x99,
    /// <summary>Copies the second f64 sign bit to the first operand.</summary>
    F64CopySign = 0xa6,
    /// <summary>Copies the second f32 sign bit to the first operand.</summary>
    F32CopySign = 0x98,
    /// <summary>Minimum f64, propagating NaN and preferring negative zero.</summary>
    F64Min = 0xa4,
    /// <summary>Maximum f64, propagating NaN and preferring positive zero.</summary>
    F64Max = 0xa5,
    /// <summary>Adds two f64 values.</summary>
    F64Add = 0xa0,
    /// <summary>Subtracts f64 values.</summary>
    F64Sub = 0xa1,
    /// <summary>Multiplies f64 values.</summary>
    F64Mul = 0xa2,
    /// <summary>Divides two IEEE 754 binary64 values.</summary>
    F64Div = 0xa3,
    /// <summary>Keeps the low 32 bits of an i64 value.</summary>
    I32WrapI64 = 0xa7,
    /// <summary>Sign-extends i32 to i64.</summary>
    I64ExtendI32S = 0xac,
    /// <summary>Zero-extends i32 to i64.</summary>
    I64ExtendI32U = 0xad,
    /// <summary>Truncates f64 toward zero to signed i64; traps on NaN or overflow.</summary>
    I64TruncF64S = 0xb0,
    /// <summary>Converts signed i32 to f32.</summary>
    F32ConvertI32S = 0xb2,
    /// <summary>Converts unsigned i32 directly to f32.</summary>
    F32ConvertI32U = 0xb3,
    /// <summary>Converts signed i64 directly to f32.</summary>
    F32ConvertI64S = 0xb4,
    /// <summary>Converts unsigned i64 directly to f32.</summary>
    F32ConvertI64U = 0xb5,
    /// <summary>Rounds an f64 value to f32.</summary>
    F32DemoteF64 = 0xb6,
    /// <summary>Converts signed i32 to f64.</summary>
    F64ConvertI32S = 0xb7,
    /// <summary>Converts unsigned i32 to f64.</summary>
    F64ConvertI32U = 0xb8,
    /// <summary>Converts signed i64 to f64.</summary>
    F64ConvertI64S = 0xb9,
    /// <summary>Converts unsigned i64 to f64.</summary>
    F64ConvertI64U = 0xba,
    /// <summary>Promotes an f32 value to f64.</summary>
    F64PromoteF32 = 0xbb,
    /// <summary>Reinterprets the IEEE 754 bits of f32 as i32.</summary>
    I32ReinterpretF32 = 0xbc,
    /// <summary>Reinterprets the IEEE 754 bits of f64 as i64.</summary>
    I64ReinterpretF64 = 0xbd,
    /// <summary>Sign-extends the low eight bits to i32.</summary>
    I32Extend8S = 0xc0,
    /// <summary>Sign-extends the low sixteen bits to i32.</summary>
    I32Extend16S = 0xc1,
    /// <summary>Pushes a null reference of the following heap type.</summary>
    RefNull = 0xd0,
    /// <summary>Tests whether a reference is null.</summary>
    RefIsNull = 0xd1,
    /// <summary>Compares GC references by identity.</summary>
    RefEq = 0xd3,
    /// <summary>Introduces a GC instruction with a U32 subopcode.</summary>
    Gc = 0xfb,
    /// <summary>Introduces miscellaneous instructions including bulk memory.</summary>
    Misc = 0xfc,
    /// <summary>Loads four bytes as i32.</summary>
    I32Load = 0x28,
    /// <summary>Loads eight bytes as i64.</summary>
    I64Load = 0x29,
    /// <summary>Loads four bytes as f32.</summary>
    F32Load = 0x2a,
    /// <summary>Loads eight bytes as f64.</summary>
    F64Load = 0x2b,
    /// <summary>Loads and sign-extends one byte.</summary>
    I32Load8S = 0x2c,
    /// <summary>Loads and zero-extends one byte.</summary>
    I32Load8U = 0x2d,
    /// <summary>Loads and sign-extends two bytes.</summary>
    I32Load16S = 0x2e,
    /// <summary>Loads and zero-extends two bytes.</summary>
    I32Load16U = 0x2f,
    /// <summary>Stores four bytes from i32.</summary>
    I32Store = 0x36,
    /// <summary>Stores eight bytes from i64.</summary>
    I64Store = 0x37,
    /// <summary>Stores four bytes from f32.</summary>
    F32Store = 0x38,
    /// <summary>Stores eight bytes from f64.</summary>
    F64Store = 0x39,
    /// <summary>Stores the low byte of i32.</summary>
    I32Store8 = 0x3a,
    /// <summary>Stores the low two bytes of i32.</summary>
    I32Store16 = 0x3b,

}
