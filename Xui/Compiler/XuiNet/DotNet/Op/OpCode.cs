// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Xui.Compiler.XuiNet.DotNet.Op;

/// <summary>IL opcode identifiers available to XuiNet, preserving the .NET opcode names and encodings.</summary>
/// <remarks>
/// Values below 0x100 encode a single byte; values beginning with 0xFE encode the
/// two-byte sequence 0xFE followed by the low byte. Instruction operands are separate
/// from the opcode. Branch displacements and metadata tokens in the member descriptions
/// refer to the original IL encoding; this enum does not prescribe an IR operand layout.
/// Prefixes modify the following instruction. Declaring an opcode here does not imply
/// that the frontend or a backend implements it.
/// See <see href="https://learn.microsoft.com/dotnet/api/system.reflection.metadata.ilopcode"/>
/// and <see href="https://learn.microsoft.com/dotnet/api/system.reflection.emit.opcodes"/>.
/// </remarks>
public enum OpCode : ushort
{
    /// <summary>Performs no operation and leaves the evaluation stack unchanged.</summary>
    Nop = 0x00,

    /// <summary>Signals a breakpoint to the debugger without changing the evaluation stack.</summary>
    Break = 0x01,

    /// <summary>Pushes argument 0; the index is encoded in the opcode.</summary>
    Ldarg_0 = 0x02,

    /// <summary>Pushes argument 1; the index is encoded in the opcode.</summary>
    Ldarg_1 = 0x03,

    /// <summary>Pushes argument 2; the index is encoded in the opcode.</summary>
    Ldarg_2 = 0x04,

    /// <summary>Pushes argument 3; the index is encoded in the opcode.</summary>
    Ldarg_3 = 0x05,

    /// <summary>Pushes local variable 0; the index is encoded in the opcode.</summary>
    Ldloc_0 = 0x06,

    /// <summary>Pushes local variable 1; the index is encoded in the opcode.</summary>
    Ldloc_1 = 0x07,

    /// <summary>Pushes local variable 2; the index is encoded in the opcode.</summary>
    Ldloc_2 = 0x08,

    /// <summary>Pushes local variable 3; the index is encoded in the opcode.</summary>
    Ldloc_3 = 0x09,

    /// <summary>Stores the top stack value in local variable 0; the index is encoded in the opcode.</summary>
    Stloc_0 = 0x0a,

    /// <summary>Stores the top stack value in local variable 1; the index is encoded in the opcode.</summary>
    Stloc_1 = 0x0b,

    /// <summary>Stores the top stack value in local variable 2; the index is encoded in the opcode.</summary>
    Stloc_2 = 0x0c,

    /// <summary>Stores the top stack value in local variable 3; the index is encoded in the opcode.</summary>
    Stloc_3 = 0x0d,

    /// <summary>Pushes argument at the unsigned byte operand index; short operand form.</summary>
    Ldarg_s = 0x0e,

    /// <summary>Pushes a managed pointer to argument at the unsigned byte operand index; short operand form.</summary>
    Ldarga_s = 0x0f,

    /// <summary>Stores the top stack value in argument at the unsigned byte operand index; short operand form.</summary>
    Starg_s = 0x10,

    /// <summary>Pushes local variable at the unsigned byte operand index; short operand form.</summary>
    Ldloc_s = 0x11,

    /// <summary>Pushes a managed pointer to local variable at the unsigned byte operand index; short operand form.</summary>
    Ldloca_s = 0x12,

    /// <summary>Stores the top stack value in local variable at the unsigned byte operand index; short operand form.</summary>
    Stloc_s = 0x13,

    /// <summary>Pushes a null object reference.</summary>
    Ldnull = 0x14,

    /// <summary>Pushes the i32 constant -1; the value is encoded in the opcode.</summary>
    Ldc_i4_m1 = 0x15,

    /// <summary>Pushes the i32 constant 0; the value is encoded in the opcode.</summary>
    Ldc_i4_0 = 0x16,

    /// <summary>Pushes the i32 constant 1; the value is encoded in the opcode.</summary>
    Ldc_i4_1 = 0x17,

    /// <summary>Pushes the i32 constant 2; the value is encoded in the opcode.</summary>
    Ldc_i4_2 = 0x18,

    /// <summary>Pushes the i32 constant 3; the value is encoded in the opcode.</summary>
    Ldc_i4_3 = 0x19,

    /// <summary>Pushes the i32 constant 4; the value is encoded in the opcode.</summary>
    Ldc_i4_4 = 0x1a,

    /// <summary>Pushes the i32 constant 5; the value is encoded in the opcode.</summary>
    Ldc_i4_5 = 0x1b,

    /// <summary>Pushes the i32 constant 6; the value is encoded in the opcode.</summary>
    Ldc_i4_6 = 0x1c,

    /// <summary>Pushes the i32 constant 7; the value is encoded in the opcode.</summary>
    Ldc_i4_7 = 0x1d,

    /// <summary>Pushes the i32 constant 8; the value is encoded in the opcode.</summary>
    Ldc_i4_8 = 0x1e,

    /// <summary>Pushes the signed byte operand, sign-extended to an i32; short operand form.</summary>
    Ldc_i4_s = 0x1f,

    /// <summary>Pushes the signed 32-bit integer operand.</summary>
    Ldc_i4 = 0x20,

    /// <summary>Pushes the signed 64-bit integer operand.</summary>
    Ldc_i8 = 0x21,

    /// <summary>Pushes the 32-bit floating-point operand onto the floating-point evaluation stack.</summary>
    Ldc_r4 = 0x22,

    /// <summary>Pushes the 64-bit floating-point operand onto the floating-point evaluation stack.</summary>
    Ldc_r8 = 0x23,

    /// <summary>Copies the top stack value, leaving two copies on the stack.</summary>
    Dup = 0x25,

    /// <summary>Discards the top stack value.</summary>
    Pop = 0x26,

    /// <summary>Transfers execution to the method identified by the operand token, reusing the current arguments; requires an empty stack.</summary>
    Jmp = 0x27,

    /// <summary>Calls the method identified by the operand token, consuming its arguments and pushing its result if any.</summary>
    Call = 0x28,

    /// <summary>Calls a function pointer using the calling convention and signature identified by the operand token.</summary>
    Calli = 0x29,

    /// <summary>Returns to the caller, supplying the top stack value for a non-void method.</summary>
    Ret = 0x2a,

    /// <summary>Branches unconditionally; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Br_s = 0x2b,

    /// <summary>Pops a condition and branches if it is zero or null; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Brfalse_s = 0x2c,

    /// <summary>Pops a condition and branches if it is nonzero or non-null; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Brtrue_s = 0x2d,

    /// <summary>Pops two values and branches if they are equal; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Beq_s = 0x2e,

    /// <summary>Pops two values and branches if the preceding value is greater than or equal to the top value, using signed integer or ordered floating-point comparison; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Bge_s = 0x2f,

    /// <summary>Pops two values and branches if the preceding value is greater than the top value, using signed integer or ordered floating-point comparison; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Bgt_s = 0x30,

    /// <summary>Pops two values and branches if the preceding value is less than or equal to the top value, using signed integer or ordered floating-point comparison; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Ble_s = 0x31,

    /// <summary>Pops two values and branches if the preceding value is less than the top value, using signed integer or ordered floating-point comparison; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Blt_s = 0x32,

    /// <summary>Pops two values and branches if they differ or are unordered floating-point values; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Bne_un_s = 0x33,

    /// <summary>Pops two values and branches if the preceding value is greater than or equal to the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Bge_un_s = 0x34,

    /// <summary>Pops two values and branches if the preceding value is greater than the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Bgt_un_s = 0x35,

    /// <summary>Pops two values and branches if the preceding value is less than or equal to the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Ble_un_s = 0x36,

    /// <summary>Pops two values and branches if the preceding value is less than the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Blt_un_s = 0x37,

    /// <summary>Branches unconditionally; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Br = 0x38,

    /// <summary>Pops a condition and branches if it is zero or null; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Brfalse = 0x39,

    /// <summary>Pops a condition and branches if it is nonzero or non-null; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Brtrue = 0x3a,

    /// <summary>Pops two values and branches if they are equal; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Beq = 0x3b,

    /// <summary>Pops two values and branches if the preceding value is greater than or equal to the top value, using signed integer or ordered floating-point comparison; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Bge = 0x3c,

    /// <summary>Pops two values and branches if the preceding value is greater than the top value, using signed integer or ordered floating-point comparison; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Bgt = 0x3d,

    /// <summary>Pops two values and branches if the preceding value is less than or equal to the top value, using signed integer or ordered floating-point comparison; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Ble = 0x3e,

    /// <summary>Pops two values and branches if the preceding value is less than the top value, using signed integer or ordered floating-point comparison; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Blt = 0x3f,

    /// <summary>Pops two values and branches if they differ or are unordered floating-point values; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Bne_un = 0x40,

    /// <summary>Pops two values and branches if the preceding value is greater than or equal to the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Bge_un = 0x41,

    /// <summary>Pops two values and branches if the preceding value is greater than the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Bgt_un = 0x42,

    /// <summary>Pops two values and branches if the preceding value is less than or equal to the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Ble_un = 0x43,

    /// <summary>Pops two values and branches if the preceding value is less than the top value using unsigned comparison, or if floating-point operands are unordered; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Blt_un = 0x44,

    /// <summary>Pops a zero-based selector and branches through the operand jump table; an out-of-range selector falls through.</summary>
    Switch = 0x45,

    /// <summary>Loads a signed 8-bit integer, sign-extended to i32 from the address on the stack.</summary>
    Ldind_i1 = 0x46,

    /// <summary>Loads an unsigned 8-bit integer, zero-extended to i32 from the address on the stack.</summary>
    Ldind_u1 = 0x47,

    /// <summary>Loads a signed 16-bit integer, sign-extended to i32 from the address on the stack.</summary>
    Ldind_i2 = 0x48,

    /// <summary>Loads an unsigned 16-bit integer, zero-extended to i32 from the address on the stack.</summary>
    Ldind_u2 = 0x49,

    /// <summary>Loads a signed 32-bit integer from the address on the stack.</summary>
    Ldind_i4 = 0x4a,

    /// <summary>Loads an unsigned 32-bit integer from the address on the stack.</summary>
    Ldind_u4 = 0x4b,

    /// <summary>Loads a 64-bit integer from the address on the stack.</summary>
    Ldind_i8 = 0x4c,

    /// <summary>Loads a native-sized integer from the address on the stack.</summary>
    Ldind_i = 0x4d,

    /// <summary>Loads a 32-bit floating-point value from the address on the stack.</summary>
    Ldind_r4 = 0x4e,

    /// <summary>Loads a 64-bit floating-point value from the address on the stack.</summary>
    Ldind_r8 = 0x4f,

    /// <summary>Loads an object reference from the address on the stack.</summary>
    Ldind_ref = 0x50,

    /// <summary>Stores an object reference at the destination address on the stack.</summary>
    Stind_ref = 0x51,

    /// <summary>Stores the low 8 bits of an integer at the destination address on the stack.</summary>
    Stind_i1 = 0x52,

    /// <summary>Stores the low 16 bits of an integer at the destination address on the stack.</summary>
    Stind_i2 = 0x53,

    /// <summary>Stores a 32-bit integer at the destination address on the stack.</summary>
    Stind_i4 = 0x54,

    /// <summary>Stores a 64-bit integer at the destination address on the stack.</summary>
    Stind_i8 = 0x55,

    /// <summary>Stores a value rounded to 32-bit floating point at the destination address on the stack.</summary>
    Stind_r4 = 0x56,

    /// <summary>Stores a value rounded to 64-bit floating point at the destination address on the stack.</summary>
    Stind_r8 = 0x57,

    /// <summary>Adds two values and pushes the result; integer overflow is unchecked.</summary>
    Add = 0x58,

    /// <summary>Subtracts the top value from the preceding value; integer overflow is unchecked.</summary>
    Sub = 0x59,

    /// <summary>Multiplies two values and pushes the result; integer overflow is unchecked.</summary>
    Mul = 0x5a,

    /// <summary>Divides the preceding value by the top value using signed integer or floating-point division.</summary>
    Div = 0x5b,

    /// <summary>Divides the preceding integer by the top integer, interpreting both as unsigned.</summary>
    Div_un = 0x5c,

    /// <summary>Computes the remainder of the preceding value divided by the top value using signed integer or floating-point semantics.</summary>
    Rem = 0x5d,

    /// <summary>Computes the unsigned integer remainder of the preceding value divided by the top value.</summary>
    Rem_un = 0x5e,

    /// <summary>Pops two integers and pushes their bitwise AND.</summary>
    And = 0x5f,

    /// <summary>Pops two integers and pushes their bitwise OR.</summary>
    Or = 0x60,

    /// <summary>Pops two integers and pushes their bitwise exclusive OR.</summary>
    Xor = 0x61,

    /// <summary>Shifts an integer left by the bit count popped from the stack, filling with zero bits.</summary>
    Shl = 0x62,

    /// <summary>Shifts an integer right by the bit count popped from the stack, preserving its sign.</summary>
    Shr = 0x63,

    /// <summary>Shifts an integer right by the bit count popped from the stack, filling with zero bits.</summary>
    Shr_un = 0x64,

    /// <summary>Negates the top numeric value without checking integer overflow.</summary>
    Neg = 0x65,

    /// <summary>Inverts every bit of the top integer value.</summary>
    Not = 0x66,

    /// <summary>Converts the top value to a signed 8-bit integer, without an overflow check; the result is sign-extended to i32 on the stack.</summary>
    Conv_i1 = 0x67,

    /// <summary>Converts the top value to a signed 16-bit integer, without an overflow check; the result is sign-extended to i32 on the stack.</summary>
    Conv_i2 = 0x68,

    /// <summary>Converts the top value to a signed 32-bit integer, without an overflow check.</summary>
    Conv_i4 = 0x69,

    /// <summary>Converts the top value to a signed 64-bit integer, without an overflow check.</summary>
    Conv_i8 = 0x6a,

    /// <summary>Converts the top value to 32-bit floating point, rounding to the target precision.</summary>
    Conv_r4 = 0x6b,

    /// <summary>Converts the top value to 64-bit floating point, rounding to the target precision.</summary>
    Conv_r8 = 0x6c,

    /// <summary>Converts the top value to an unsigned 32-bit integer, without an overflow check.</summary>
    Conv_u4 = 0x6d,

    /// <summary>Converts the top value to an unsigned 64-bit integer, without an overflow check.</summary>
    Conv_u8 = 0x6e,

    /// <summary>Calls an instance method using the operand method token, checking the receiver for null and performing virtual dispatch when applicable.</summary>
    Callvirt = 0x6f,

    /// <summary>Copies a value of the operand token type from the source address to the destination address on the stack.</summary>
    Cpobj = 0x70,

    /// <summary>Loads a value of the operand token type from the address on the stack.</summary>
    Ldobj = 0x71,

    /// <summary>Pushes the string literal identified by the operand user-string token.</summary>
    Ldstr = 0x72,

    /// <summary>Invokes the constructor identified by the operand token and pushes the constructed object reference or value.</summary>
    Newobj = 0x73,

    /// <summary>Checks an object reference against the operand token type, preserving null and throwing if a non-null value is incompatible.</summary>
    Castclass = 0x74,

    /// <summary>Tests an object reference against the operand token type, yielding the compatible reference or null.</summary>
    Isinst = 0x75,

    /// <summary>Converts an unsigned integer to the floating-point evaluation-stack representation.</summary>
    Conv_r_un = 0x76,

    /// <summary>Checks a boxed value against the operand token type and pushes a managed pointer to its payload.</summary>
    Unbox = 0x79,

    /// <summary>Throws the exception object popped from the stack.</summary>
    Throw = 0x7a,

    /// <summary>Loads the instance field identified by the operand token from the receiver on the stack.</summary>
    Ldfld = 0x7b,

    /// <summary>Pushes a managed pointer to the instance field identified by the operand token.</summary>
    Ldflda = 0x7c,

    /// <summary>Stores the top value in the instance field identified by the operand token on the preceding receiver.</summary>
    Stfld = 0x7d,

    /// <summary>Loads the static field identified by the operand token.</summary>
    Ldsfld = 0x7e,

    /// <summary>Pushes a managed pointer to the static field identified by the operand token.</summary>
    Ldsflda = 0x7f,

    /// <summary>Stores the top value in the static field identified by the operand token.</summary>
    Stsfld = 0x80,

    /// <summary>Stores a value of the operand token type at the destination address on the stack.</summary>
    Stobj = 0x81,

    /// <summary>Converts the top value to a signed 8-bit integer, interpreting the input integer as unsigned, throwing on overflow; the result is sign-extended to i32 on the stack.</summary>
    Conv_ovf_i1_un = 0x82,

    /// <summary>Converts the top value to a signed 16-bit integer, interpreting the input integer as unsigned, throwing on overflow; the result is sign-extended to i32 on the stack.</summary>
    Conv_ovf_i2_un = 0x83,

    /// <summary>Converts the top value to a signed 32-bit integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_i4_un = 0x84,

    /// <summary>Converts the top value to a signed 64-bit integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_i8_un = 0x85,

    /// <summary>Converts the top value to an unsigned 8-bit integer, interpreting the input integer as unsigned, throwing on overflow; the result is zero-extended to i32 on the stack.</summary>
    Conv_ovf_u1_un = 0x86,

    /// <summary>Converts the top value to an unsigned 16-bit integer, interpreting the input integer as unsigned, throwing on overflow; the result is zero-extended to i32 on the stack.</summary>
    Conv_ovf_u2_un = 0x87,

    /// <summary>Converts the top value to an unsigned 32-bit integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_u4_un = 0x88,

    /// <summary>Converts the top value to an unsigned 64-bit integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_u8_un = 0x89,

    /// <summary>Converts the top value to a signed native-sized integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_i_un = 0x8a,

    /// <summary>Converts the top value to an unsigned native-sized integer, interpreting the input integer as unsigned, throwing on overflow.</summary>
    Conv_ovf_u_un = 0x8b,

    /// <summary>Converts a value of the operand token type to its boxed object representation.</summary>
    Box = 0x8c,

    /// <summary>Allocates a zero-based, one-dimensional array with the stack-supplied length and operand token element type.</summary>
    Newarr = 0x8d,

    /// <summary>Replaces an array reference with its element count as a native unsigned integer.</summary>
    Ldlen = 0x8e,

    /// <summary>Pops an index and array reference and pushes a managed pointer to the element of the operand token type.</summary>
    Ldelema = 0x8f,

    /// <summary>Loads a signed 8-bit integer, sign-extended to i32 from the array reference and index on the stack.</summary>
    Ldelem_i1 = 0x90,

    /// <summary>Loads an unsigned 8-bit integer, zero-extended to i32 from the array reference and index on the stack.</summary>
    Ldelem_u1 = 0x91,

    /// <summary>Loads a signed 16-bit integer, sign-extended to i32 from the array reference and index on the stack.</summary>
    Ldelem_i2 = 0x92,

    /// <summary>Loads an unsigned 16-bit integer, zero-extended to i32 from the array reference and index on the stack.</summary>
    Ldelem_u2 = 0x93,

    /// <summary>Loads a signed 32-bit integer from the array reference and index on the stack.</summary>
    Ldelem_i4 = 0x94,

    /// <summary>Loads an unsigned 32-bit integer from the array reference and index on the stack.</summary>
    Ldelem_u4 = 0x95,

    /// <summary>Loads a 64-bit integer from the array reference and index on the stack.</summary>
    Ldelem_i8 = 0x96,

    /// <summary>Loads a native-sized integer from the array reference and index on the stack.</summary>
    Ldelem_i = 0x97,

    /// <summary>Loads a 32-bit floating-point value from the array reference and index on the stack.</summary>
    Ldelem_r4 = 0x98,

    /// <summary>Loads a 64-bit floating-point value from the array reference and index on the stack.</summary>
    Ldelem_r8 = 0x99,

    /// <summary>Loads an object reference from the array reference and index on the stack.</summary>
    Ldelem_ref = 0x9a,

    /// <summary>Stores a native-sized integer in the array at the stack-supplied index.</summary>
    Stelem_i = 0x9b,

    /// <summary>Stores the low 8 bits of an integer in the array at the stack-supplied index.</summary>
    Stelem_i1 = 0x9c,

    /// <summary>Stores the low 16 bits of an integer in the array at the stack-supplied index.</summary>
    Stelem_i2 = 0x9d,

    /// <summary>Stores a 32-bit integer in the array at the stack-supplied index.</summary>
    Stelem_i4 = 0x9e,

    /// <summary>Stores a 64-bit integer in the array at the stack-supplied index.</summary>
    Stelem_i8 = 0x9f,

    /// <summary>Stores a value rounded to 32-bit floating point in the array at the stack-supplied index.</summary>
    Stelem_r4 = 0xa0,

    /// <summary>Stores a value rounded to 64-bit floating point in the array at the stack-supplied index.</summary>
    Stelem_r8 = 0xa1,

    /// <summary>Stores an object reference in the array at the stack-supplied index.</summary>
    Stelem_ref = 0xa2,

    /// <summary>Loads an array element at the stack-supplied index using the operand token type.</summary>
    Ldelem = 0xa3,

    /// <summary>Stores a value at the stack-supplied array index using the operand token type.</summary>
    Stelem = 0xa4,

    /// <summary>Extracts a boxed value of the operand token type; for reference types, performs a cast instead.</summary>
    Unbox_any = 0xa5,

    /// <summary>Converts the top value to a signed 8-bit integer, throwing on overflow; the result is sign-extended to i32 on the stack.</summary>
    Conv_ovf_i1 = 0xb3,

    /// <summary>Converts the top value to an unsigned 8-bit integer, throwing on overflow; the result is zero-extended to i32 on the stack.</summary>
    Conv_ovf_u1 = 0xb4,

    /// <summary>Converts the top value to a signed 16-bit integer, throwing on overflow; the result is sign-extended to i32 on the stack.</summary>
    Conv_ovf_i2 = 0xb5,

    /// <summary>Converts the top value to an unsigned 16-bit integer, throwing on overflow; the result is zero-extended to i32 on the stack.</summary>
    Conv_ovf_u2 = 0xb6,

    /// <summary>Converts the top value to a signed 32-bit integer, throwing on overflow.</summary>
    Conv_ovf_i4 = 0xb7,

    /// <summary>Converts the top value to an unsigned 32-bit integer, throwing on overflow.</summary>
    Conv_ovf_u4 = 0xb8,

    /// <summary>Converts the top value to a signed 64-bit integer, throwing on overflow.</summary>
    Conv_ovf_i8 = 0xb9,

    /// <summary>Converts the top value to an unsigned 64-bit integer, throwing on overflow.</summary>
    Conv_ovf_u8 = 0xba,

    /// <summary>Checks a typed reference against the operand token type and extracts its managed pointer.</summary>
    Refanyval = 0xc2,

    /// <summary>Checks the top floating-point value and throws if it is NaN or infinity.</summary>
    Ckfinite = 0xc3,

    /// <summary>Combines a managed pointer with the operand token type to produce a typed reference.</summary>
    Mkrefany = 0xc6,

    /// <summary>Pushes a runtime type, method, or field handle for the operand metadata token.</summary>
    Ldtoken = 0xd0,

    /// <summary>Converts the top value to an unsigned 16-bit integer, without an overflow check; the result is zero-extended to i32 on the stack.</summary>
    Conv_u2 = 0xd1,

    /// <summary>Converts the top value to an unsigned 8-bit integer, without an overflow check; the result is zero-extended to i32 on the stack.</summary>
    Conv_u1 = 0xd2,

    /// <summary>Converts the top value to a signed native-sized integer, without an overflow check.</summary>
    Conv_i = 0xd3,

    /// <summary>Converts the top value to a signed native-sized integer, throwing on overflow.</summary>
    Conv_ovf_i = 0xd4,

    /// <summary>Converts the top value to an unsigned native-sized integer, throwing on overflow.</summary>
    Conv_ovf_u = 0xd5,

    /// <summary>Adds the two integer operands, throwing on signed overflow.</summary>
    Add_ovf = 0xd6,

    /// <summary>Adds the two integer operands, throwing on unsigned overflow.</summary>
    Add_ovf_un = 0xd7,

    /// <summary>Multiplies the two integer operands, throwing on signed overflow.</summary>
    Mul_ovf = 0xd8,

    /// <summary>Multiplies the two integer operands, throwing on unsigned overflow.</summary>
    Mul_ovf_un = 0xd9,

    /// <summary>Subtracts the top integer from the preceding integer, throwing on signed overflow.</summary>
    Sub_ovf = 0xda,

    /// <summary>Subtracts the top integer from the preceding integer, throwing on unsigned overflow.</summary>
    Sub_ovf_un = 0xdb,

    /// <summary>Finishes a finally or fault handler and resumes the pending leave or exception processing.</summary>
    Endfinally = 0xdc,

    /// <summary>Clears the evaluation stack and transfers control out of a protected region, executing intervening finally handlers; the operand is a signed 32-bit displacement from the end of this instruction.</summary>
    Leave = 0xdd,

    /// <summary>Clears the evaluation stack and transfers control out of a protected region, executing intervening finally handlers; the operand is a signed 8-bit displacement from the end of this instruction.</summary>
    Leave_s = 0xde,

    /// <summary>Stores a native-sized integer at the destination address on the stack.</summary>
    Stind_i = 0xdf,

    /// <summary>Converts the top value to an unsigned native-sized integer, without an overflow check.</summary>
    Conv_u = 0xe0,

    /// <summary>Pushes a runtime argument handle for accessing the variable arguments of the current vararg method.</summary>
    Arglist = 0xfe00,

    /// <summary>Pops two values and pushes i32 one if they are equal, otherwise zero; unordered floating-point values compare false.</summary>
    Ceq = 0xfe01,

    /// <summary>Pushes i32 one if the preceding value is greater than the top value using signed integer or ordered floating-point comparison, otherwise zero.</summary>
    Cgt = 0xfe02,

    /// <summary>Pushes i32 one for an unsigned greater-than comparison or unordered floating-point operands, otherwise zero.</summary>
    Cgt_un = 0xfe03,

    /// <summary>Pushes i32 one if the preceding value is less than the top value using signed integer or ordered floating-point comparison, otherwise zero.</summary>
    Clt = 0xfe04,

    /// <summary>Pushes i32 one for an unsigned less-than comparison or unordered floating-point operands, otherwise zero.</summary>
    Clt_un = 0xfe05,

    /// <summary>Pushes a function pointer for the method identified by the operand token.</summary>
    Ldftn = 0xfe06,

    /// <summary>Resolves the operand method token against the receiver on the stack and pushes the resulting function pointer.</summary>
    Ldvirtftn = 0xfe07,

    /// <summary>Pushes argument at the unsigned 16-bit operand index.</summary>
    Ldarg = 0xfe09,

    /// <summary>Pushes a managed pointer to argument at the unsigned 16-bit operand index.</summary>
    Ldarga = 0xfe0a,

    /// <summary>Stores the top stack value in argument at the unsigned 16-bit operand index.</summary>
    Starg = 0xfe0b,

    /// <summary>Pushes local variable at the unsigned 16-bit operand index.</summary>
    Ldloc = 0xfe0c,

    /// <summary>Pushes a managed pointer to local variable at the unsigned 16-bit operand index.</summary>
    Ldloca = 0xfe0d,

    /// <summary>Stores the top stack value in local variable at the unsigned 16-bit operand index.</summary>
    Stloc = 0xfe0e,

    /// <summary>Allocates the stack-supplied number of bytes from the current method local memory pool and pushes their address.</summary>
    Localloc = 0xfe0f,

    /// <summary>Ends an exception filter, returning its i32 decision to exception handling.</summary>
    Endfilter = 0xfe11,

    /// <summary>Prefixes a memory access with an alignment guarantee of one, two, or four bytes, encoded in the following byte.</summary>
    Unaligned = 0xfe12,

    /// <summary>Prefixes a memory access with volatile ordering semantics; this does not by itself guarantee atomicity.</summary>
    Volatile = 0xfe13,

    /// <summary>Prefixes a call to request a tail transfer that releases the current method frame before entering the callee.</summary>
    Tail = 0xfe14,

    /// <summary>Initializes storage at the managed pointer on the stack to the default value of the operand token type.</summary>
    Initobj = 0xfe15,

    /// <summary>Prefixes callvirt with an operand type token so the receiver can be invoked with the appropriate value-type or reference-type behavior.</summary>
    Constrained = 0xfe16,

    /// <summary>Copies a stack-supplied byte count from the source address to the destination address; overlapping regions are not supported.</summary>
    Cpblk = 0xfe17,

    /// <summary>Fills a memory region using the destination address, byte value, and byte count supplied on the stack.</summary>
    Initblk = 0xfe18,

    /// <summary>Rethrows the exception being handled by the current catch handler.</summary>
    Rethrow = 0xfe1a,

    /// <summary>Pushes the storage size in bytes of the operand token type as an i32.</summary>
    Sizeof = 0xfe1c,

    /// <summary>Extracts a runtime type handle from the typed reference on the stack.</summary>
    Refanytype = 0xfe1d,

    /// <summary>Prefixes ldelema to obtain a managed pointer with restricted mutation and omit the usual array element type check.</summary>
    Readonly = 0xfe1e,
}
