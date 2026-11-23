using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>A branch instruction with its destination resolved to an absolute IL byte offset.</summary>
    public readonly ref struct Branch
    {
        internal Branch(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            int displacement = reader.ReadInt32();
            Target = reader.Offset + displacement;
        }

        private Branch(OpCode code, int target)
        {
            Code = code;
            Target = target;
        }

        internal static Branch ReadShort(OpCode code, scoped ref BlobReader reader)
        {
            int displacement = reader.ReadSByte();
            return new Branch(code, reader.Offset + displacement);
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The target byte offset from the start of the method's IL.</summary>
        public readonly int Target;
    }
}
