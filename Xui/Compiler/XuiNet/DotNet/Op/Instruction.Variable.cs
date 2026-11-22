using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with an unsigned argument or local index value.</summary>
    public readonly ref struct Variable
    {
        internal Variable(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Index = reader.ReadUInt16();
        }

        private Variable(OpCode code, ushort value)
        {
            Code = code;
            Index = value;
        }

        internal static Variable ReadShort(OpCode code, scoped ref BlobReader reader) => new(code, reader.ReadByte());

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded unsigned argument or local index value.</summary>
        public readonly ushort Index;
    }
}
