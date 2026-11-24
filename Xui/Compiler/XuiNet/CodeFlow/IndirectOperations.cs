using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Checks scalar/reference indirect accesses using the same storage widths as vector accesses.</summary>
public static class IndirectOperations
{
    public static void Validate(OpCode code, SignatureType element)
    {
        var vectorCode = code switch
        {
            OpCode.Ldind_i1 => OpCode.Ldelem_i1,
            OpCode.Ldind_u1 => OpCode.Ldelem_u1,
            OpCode.Ldind_i2 => OpCode.Ldelem_i2,
            OpCode.Ldind_u2 => OpCode.Ldelem_u2,
            OpCode.Ldind_u4 => OpCode.Ldelem_u4,
            OpCode.Ldind_i8 => OpCode.Ldelem_i8,
            OpCode.Ldind_i => OpCode.Ldelem_i,
            OpCode.Ldind_r4 => OpCode.Ldelem_r4,
            OpCode.Ldind_r8 => OpCode.Ldelem_r8,
            OpCode.Ldind_ref => OpCode.Ldelem_ref,
            OpCode.Stind_i1 => OpCode.Stelem_i1,
            OpCode.Stind_i2 => OpCode.Stelem_i2,
            OpCode.Stind_i8 => OpCode.Stelem_i8,
            OpCode.Stind_i => OpCode.Stelem_i,
            OpCode.Stind_r4 => OpCode.Stelem_r4,
            OpCode.Stind_r8 => OpCode.Stelem_r8,
            OpCode.Stind_ref => OpCode.Stelem_ref,
            _ => throw new NotSupportedException($"Unsupported indirect operation {code}.")
        };
        ArrayOperations.Validate(vectorCode, element);
    }
}
