using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with a signed 32-bit integer value.</summary>
    public readonly ref struct Int
    {
        internal Int(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Value = reader.ReadInt32();
        }

        private Int(OpCode code, int value)
        {
            Code = code;
            Value = value;
        }

        internal static Int ReadShort(OpCode code, scoped ref BlobReader reader) => new(code, reader.ReadSByte());

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded signed 32-bit integer value.</summary>
        public readonly int Value;
    }
}
