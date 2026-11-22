using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with a signed 64-bit integer value.</summary>
    public readonly ref struct Long
    {
        internal Long(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Value = reader.ReadInt64();
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded signed 64-bit integer value.</summary>
        public readonly long Value;
    }
}
