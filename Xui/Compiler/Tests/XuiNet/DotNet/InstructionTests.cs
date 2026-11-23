using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xunit;
using OpCode = Xui.Compiler.XuiNet.DotNet.Op.OpCode;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class InstructionTests
{
    [Fact]
    public void ReaderGuardsMatchOpcodeOperandEncodings()
    {
        var encodings = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(System.Reflection.Emit.OpCode))
            .Select(field => (System.Reflection.Emit.OpCode)field.GetValue(null)!)
            .Where(opcode => Enum.IsDefined((OpCode)unchecked((ushort)opcode.Value)))
            .ToDictionary(opcode => (OpCode)unchecked((ushort)opcode.Value), opcode => opcode.OperandType);
        Assert.Equal(Enum.GetValues<OpCode>().Length, encodings.Count);
        encodings.Add((OpCode)0xfeff, OperandType.InlineNone); // Unknown opcodes accept no typed reader.
        OperandType[] readers = [OperandType.InlineI, OperandType.ShortInlineI, OperandType.InlineI8,
            OperandType.ShortInlineR, OperandType.InlineR, OperandType.InlineTok, OperandType.InlineVar,
            OperandType.ShortInlineVar, OperandType.InlineBrTarget, OperandType.ShortInlineBrTarget,
            OperandType.InlineSwitch];

        // Reuse a tiny method's PE storage for each opcode followed by zeroed operand bytes.
        byte[] image = Frontend.Roslyn.Compile("System.Console.WriteLine(\"Hello World\");");
        int rva;
        int ilOffset;
        using (var stream = new MemoryStream(image))
        using (var pe = new PEReader(stream))
        {
            var entryPoint = (MethodDefinitionHandle)MetadataTokens.EntityHandle(
                pe.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress);
            rva = pe.GetMetadataReader().GetMethodDefinition(entryPoint).RelativeVirtualAddress;
            var section = pe.PEHeaders.SectionHeaders.Single(section =>
                rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
            int headerOffset = section.PointerToRawData + rva - section.VirtualAddress;
            Assert.Equal(2, image[headerOffset] & 3);
            Assert.True(pe.GetMethodBody(rva).GetILReader().Length >= 10);
            ilOffset = headerOffset + 1;
        }

        foreach (var (code, encoding) in encodings)
        {
            image.AsSpan(ilOffset, 10).Clear();
            int prefixLength = (ushort)code > byte.MaxValue ? 1 : 0;
            if (prefixLength != 0) image[ilOffset] = (byte)((ushort)code >> 8);
            image[ilOffset + prefixLength] = (byte)code;
            using var stream = new MemoryStream(image);
            using var pe = new PEReader(stream);
            var reader = new ILReader(pe.GetMethodBody(rva).GetILReader());
            var instruction = reader.Read();
            Assert.Equal(code, instruction.Code);
            var expected = encoding switch
            {
                OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineSig or
                OperandType.InlineString or OperandType.InlineType => OperandType.InlineTok,
                _ => encoding
            };

            foreach (var kind in readers)
            {
                var cursor = reader;
                bool rejected = false;
                OpCode actual = default;
                try { actual = ReadOperand(instruction, kind, ref cursor); }
                catch (InvalidOperationException) { rejected = true; }
                Assert.True(rejected == (kind != expected), $"{code}: reader {kind}, encoding {encoding}.");
                if (rejected)
                    Assert.Equal(reader.Offset, cursor.Offset);
                else
                {
                    Assert.Equal(code, actual);
                    int bytes = kind switch
                    {
                        OperandType.ShortInlineI or OperandType.ShortInlineVar or OperandType.ShortInlineBrTarget => 1,
                        OperandType.InlineVar => 2,
                        OperandType.InlineI8 or OperandType.InlineR => 8,
                        _ => 4 // Includes a switch with zero targets.
                    };
                    Assert.Equal(reader.Offset + bytes, cursor.Offset);
                }
            }
        }
    }

    private static OpCode ReadOperand(Instruction instruction, OperandType kind, ref ILReader reader) => kind switch
    {
        OperandType.InlineI => instruction.ReadInt(ref reader).Code,
        OperandType.ShortInlineI => instruction.ReadShortInt(ref reader).Code,
        OperandType.InlineI8 => instruction.ReadLong(ref reader).Code,
        OperandType.ShortInlineR => instruction.ReadFloat(ref reader).Code,
        OperandType.InlineR => instruction.ReadDouble(ref reader).Code,
        OperandType.InlineTok => instruction.ReadToken(ref reader).Code,
        OperandType.InlineVar => instruction.ReadVariable(ref reader).Code,
        OperandType.ShortInlineVar => instruction.ReadShortVariable(ref reader).Code,
        OperandType.InlineBrTarget => instruction.ReadBranch(ref reader).Code,
        OperandType.ShortInlineBrTarget => instruction.ReadShortBranch(ref reader).Code,
        OperandType.InlineSwitch => instruction.ReadSwitch(ref reader).Code,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
