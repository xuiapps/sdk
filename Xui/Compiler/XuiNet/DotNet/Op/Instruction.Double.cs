using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>An instruction with a 64-bit floating-point value.</summary>
    public readonly ref struct Double
    {
        internal Double(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            Value = reader.ReadDouble();
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The decoded 64-bit floating-point value.</summary>
        public readonly double Value;
    }
}
