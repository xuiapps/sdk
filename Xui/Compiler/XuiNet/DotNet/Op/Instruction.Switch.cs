using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Op;

public readonly ref partial struct Instruction
{
    /// <summary>A switch instruction borrowing its destination table from the PE image.</summary>
    public readonly ref struct Switch
    {
        internal Switch(OpCode code, scoped ref BlobReader reader)
        {
            Code = code;
            int count = reader.ReadInt32();
            if (count < 0 || count > reader.RemainingBytes / sizeof(int))
                throw new BadImageFormatException($"Invalid switch table at IL_{reader.Offset - sizeof(int):x4}.");
            var table = reader;
            reader.Offset += count * sizeof(int);
            Targets = new SwitchTargets(table, count, reader.Offset);
        }

        /// <summary>The original IL opcode, preserving compact forms and instruction prefixes.</summary>
        public OpCode Code { get; }

        /// <summary>The borrowed absolute branch destinations. Keep the owning assembly undisposed while using them.</summary>
        public readonly SwitchTargets Targets;
    }
}
