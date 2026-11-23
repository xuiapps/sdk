using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet;

/// <summary>A cursor over borrowed IL bytes that reads opcodes separately from their operands.</summary>
/// <remarks>
/// Read an opcode, then consume its operands using the corresponding Instruction method before
/// reading the next opcode. Keep the owning assembly undisposed while using the cursor or borrowed targets.
/// </remarks>
public ref struct ILReader
{
    internal BlobReader Bytes;

    /// <summary>Creates a cursor at the supplied IL reader's current position.</summary>
    public ILReader(BlobReader il) => Bytes = il;

    /// <summary>The current byte offset from the start of the method's IL.</summary>
    public readonly int Offset => Bytes.Offset;

    /// <summary>Whether there are bytes left to read.</summary>
    public readonly bool HasRemaining => Bytes.RemainingBytes != 0;

    /// <summary>Moves to a previously indexed instruction offset in this method.</summary>
    public void Seek(int offset) => Bytes.Offset = offset;

    /// <summary>Reads only the opcode, leaving the cursor at its first operand byte.</summary>
    /// <remarks>The consumer dispatches on Code and rejects unsupported opcodes.</remarks>
    public Instruction Read()
    {
        ushort code = Bytes.ReadByte();
        if (code == 0xfe)
            code = (ushort)((code << 8) | Bytes.ReadByte());
        return new Instruction((OpCode)code);
    }
}
