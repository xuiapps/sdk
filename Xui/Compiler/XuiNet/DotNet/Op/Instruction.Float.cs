using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with a 32-bit floating-point value.</summary>
    public readonly ref struct Float
    {
        internal Float(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Value = reader.ReadSingle();
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded 32-bit floating-point value.</summary>
        public readonly float Value;
    }
}
