namespace Xui.Compiler.XuiNet.DotNet.Op;

/// <summary>An IL opcode with explicit readers for its typed instruction operands.</summary>
/// <remarks>
/// Choose the reader matching Code. Each call consumes operands from the same cursor that read
/// this opcode. Operandless instructions need no further read. An incompatible opcode is rejected
/// before consuming any operand bytes.
/// </remarks>
public readonly ref partial struct Instruction
{
    internal Instruction(OpCode code) => Code = code;

    /// <summary>The IL operation, including compact forms and instruction prefixes.</summary>
    public OpCode Code { get; }

    /// <summary>Consumes a signed 32-bit integer from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Int ReadInt(scoped ref ILReader reader)
    {
        if (Code != OpCode.Ldc_i4)
            throw InvalidReader(nameof(ReadInt));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a sign-extended 8-bit integer from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Int ReadShortInt(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Ldc_i4_s or OpCode.Unaligned))
            throw InvalidReader(nameof(ReadShortInt));
        return Int.ReadShort(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a signed 64-bit integer from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Long ReadLong(scoped ref ILReader reader)
    {
        if (Code != OpCode.Ldc_i8)
            throw InvalidReader(nameof(ReadLong));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a 32-bit floating-point value from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Float ReadFloat(scoped ref ILReader reader)
    {
        if (Code != OpCode.Ldc_r4)
            throw InvalidReader(nameof(ReadFloat));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a 64-bit floating-point value from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Double ReadDouble(scoped ref ILReader reader)
    {
        if (Code != OpCode.Ldc_r8)
            throw InvalidReader(nameof(ReadDouble));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a metadata token from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Token ReadToken(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Jmp or OpCode.Call or OpCode.Calli or OpCode.Callvirt or
            OpCode.Cpobj or OpCode.Ldobj or OpCode.Ldstr or OpCode.Newobj or
            OpCode.Castclass or OpCode.Isinst or OpCode.Unbox or OpCode.Ldfld or
            OpCode.Ldflda or OpCode.Stfld or OpCode.Ldsfld or OpCode.Ldsflda or
            OpCode.Stsfld or OpCode.Stobj or OpCode.Box or OpCode.Newarr or
            OpCode.Ldelema or OpCode.Ldelem or OpCode.Stelem or OpCode.Unbox_any or
            OpCode.Refanyval or OpCode.Mkrefany or OpCode.Ldtoken or OpCode.Ldftn or
            OpCode.Ldvirtftn or OpCode.Initobj or OpCode.Constrained or OpCode.Sizeof))
            throw InvalidReader(nameof(ReadToken));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a 16-bit argument or local index from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Variable ReadVariable(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Ldarg or OpCode.Ldarga or OpCode.Starg or OpCode.Ldloc or
            OpCode.Ldloca or OpCode.Stloc))
            throw InvalidReader(nameof(ReadVariable));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes an 8-bit argument or local index from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Variable ReadShortVariable(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Ldarg_s or OpCode.Ldarga_s or OpCode.Starg_s or OpCode.Ldloc_s or
            OpCode.Ldloca_s or OpCode.Stloc_s))
            throw InvalidReader(nameof(ReadShortVariable));
        return Variable.ReadShort(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a 32-bit branch displacement from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Branch ReadBranch(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Br or OpCode.Brfalse or OpCode.Brtrue or OpCode.Beq or
            OpCode.Bge or OpCode.Bgt or OpCode.Ble or OpCode.Blt or
            OpCode.Bne_un or OpCode.Bge_un or OpCode.Bgt_un or OpCode.Ble_un or
            OpCode.Blt_un or OpCode.Leave))
            throw InvalidReader(nameof(ReadBranch));
        return new(Code, ref reader.Bytes);
    }

    /// <summary>Consumes an 8-bit branch displacement from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Branch ReadShortBranch(scoped ref ILReader reader)
    {
        if (Code is not (OpCode.Br_s or OpCode.Brfalse_s or OpCode.Brtrue_s or OpCode.Beq_s or
            OpCode.Bge_s or OpCode.Bgt_s or OpCode.Ble_s or OpCode.Blt_s or
            OpCode.Bne_un_s or OpCode.Bge_un_s or OpCode.Bgt_un_s or OpCode.Ble_un_s or
            OpCode.Blt_un_s or OpCode.Leave_s))
            throw InvalidReader(nameof(ReadShortBranch));
        return Branch.ReadShort(Code, ref reader.Bytes);
    }

    /// <summary>Consumes a borrowed switch table from the cursor.</summary>
    /// <exception cref="InvalidOperationException">Code does not use this operand encoding; the cursor is unchanged.</exception>
    public Switch ReadSwitch(scoped ref ILReader reader)
    {
        if (Code != OpCode.Switch)
            throw InvalidReader(nameof(ReadSwitch));
        return new(Code, ref reader.Bytes);
    }

    private InvalidOperationException InvalidReader(string method) =>
        new($"Opcode {Code} cannot be read with {method}.");
}
