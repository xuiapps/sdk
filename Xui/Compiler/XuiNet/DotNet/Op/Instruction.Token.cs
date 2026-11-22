using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with a metadata token value.</summary>
    public readonly ref struct Token
    {
        internal Token(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Value = reader.ReadInt32();
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded metadata token value.</summary>
        public readonly int Value;
    }
}
