using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Computes the absolute value of f32.</summary>
    public readonly Writer F32Abs() { body.Instruction(OpCode.F32Abs); return this; }
    /// <summary>Negates f32.</summary>
    public readonly Writer F32Neg() { body.Instruction(OpCode.F32Neg); return this; }
    /// <summary>Computes the IEEE minimum, propagating NaN and choosing negative zero.</summary>
    public readonly Writer F32Min() { body.Instruction(OpCode.F32Min); return this; }
    /// <summary>Computes the IEEE maximum, propagating NaN and choosing positive zero.</summary>
    public readonly Writer F32Max() { body.Instruction(OpCode.F32Max); return this; }
    /// <summary>Adds two f32 values.</summary>
    public readonly Writer F32Add() { body.Instruction(OpCode.F32Add); return this; }
    /// <summary>Subtracts two f32 values.</summary>
    public readonly Writer F32Sub() { body.Instruction(OpCode.F32Sub); return this; }
    /// <summary>Multiplies two f32 values.</summary>
    public readonly Writer F32Mul() { body.Instruction(OpCode.F32Mul); return this; }
    /// <summary>Divides two f32 values.</summary>
    public readonly Writer F32Div() { body.Instruction(OpCode.F32Div); return this; }
    /// <summary>Compares f32 values for equality.</summary>
    public readonly Writer F32Eq() { body.Instruction(OpCode.F32Eq); return this; }
    /// <summary>Compares f32 values for inequality.</summary>
    public readonly Writer F32Ne() { body.Instruction(OpCode.F32Ne); return this; }
    /// <summary>Compares f32 values for ordered less-than.</summary>
    public readonly Writer F32Lt() { body.Instruction(OpCode.F32Lt); return this; }
    /// <summary>Compares f32 values for ordered less-than-or-equal.</summary>
    public readonly Writer F32Le() { body.Instruction(OpCode.F32Le); return this; }
    /// <summary>Compares f32 values for ordered greater-than.</summary>
    public readonly Writer F32Gt() { body.Instruction(OpCode.F32Gt); return this; }
    /// <summary>Compares f32 values for ordered greater-than-or-equal.</summary>
    public readonly Writer F32Ge() { body.Instruction(OpCode.F32Ge); return this; }
}
