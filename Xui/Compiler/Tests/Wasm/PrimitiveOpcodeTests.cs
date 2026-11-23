using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class PrimitiveOpcodeTests
{
    [Theory]
    [InlineData(ILOpCode.Add, int.MaxValue, 1, int.MinValue)]
    [InlineData(ILOpCode.Sub, 7, 10, -3)]
    [InlineData(ILOpCode.Sub, int.MinValue, 1, int.MaxValue)]
    [InlineData(ILOpCode.Mul, -7, 6, -42)]
    [InlineData(ILOpCode.Mul, int.MaxValue, 2, -2)]
    [InlineData(ILOpCode.And, 12, 10, 8)]
    [InlineData(ILOpCode.Or, 12, 10, 14)]
    [InlineData(ILOpCode.Xor, 12, 10, 6)]
    public Task IntegerBinaryOperations(ILOpCode op, int left, int right, int expected) =>
        AssertExecutes(il => { il.LoadConstantI4(left); il.LoadConstantI4(right); il.OpCode(op); }, expected);

    [Theory]
    [InlineData(ILOpCode.Add, long.MaxValue, 1L, long.MinValue)]
    [InlineData(ILOpCode.Sub, long.MinValue, 1L, long.MaxValue)]
    [InlineData(ILOpCode.Mul, long.MaxValue, 2L, -2L)]
    [InlineData(ILOpCode.And, 0x10000000cL, 0x10000000aL, 0x100000008L)]
    [InlineData(ILOpCode.Or, 0x10000000cL, 0x10000000aL, 0x10000000eL)]
    [InlineData(ILOpCode.Xor, 0x10000000cL, 0x10000000aL, 6L)]
    public Task LongBinaryOperations(ILOpCode op, long left, long right, long expected) =>
        AssertExecutes(il =>
        {
            il.LoadConstantI8(left); il.LoadConstantI8(right); il.OpCode(op);
            il.LoadConstantI8(expected); il.OpCode(ILOpCode.Ceq);
        }, 1);

    [Theory]
    [InlineData(ILOpCode.Sub, 1.25, 2.5, -1.25)]
    [InlineData(ILOpCode.Mul, -1.25, 2.5, -3.125)]
    public Task FloatingPointBinaryOperations(ILOpCode op, double left, double right, double expected) =>
        AssertExecutes(il =>
        {
            il.LoadConstantR8(left); il.LoadConstantR8(right); il.OpCode(op);
            il.LoadConstantR8(expected); il.OpCode(ILOpCode.Ceq);
        }, 1);

    [Theory]
    [InlineData(ILOpCode.Not, 42, -43)]
    [InlineData(ILOpCode.Not, long.MinValue, long.MaxValue)]
    [InlineData(ILOpCode.Neg, int.MinValue, int.MinValue)]
    [InlineData(ILOpCode.Neg, long.MinValue, long.MinValue)]
    [InlineData(ILOpCode.Neg, 1.25, -1.25)]
    public Task UnaryOperations(ILOpCode op, object value, object expected) =>
        AssertExecutes(il =>
        {
            Load(il, value); il.OpCode(op); Load(il, expected); il.OpCode(ILOpCode.Ceq);
        }, 1);

    [Theory]
    [InlineData(ILOpCode.Shl)]
    [InlineData(ILOpCode.Shr)]
    [InlineData(ILOpCode.Shr_un)]
    public async Task ShiftsUseOperandWidthAndMaskedCount(ILOpCode op)
    {
        foreach (int count in new[] { 0, 1, 31, 32, 63, 64, -1 })
        {
            const int left = -1234567;
            int expected = op switch
            {
                ILOpCode.Shl => left << count,
                ILOpCode.Shr => left >> count,
                _ => unchecked((int)((uint)left >> count))
            };
            await AssertExecutes(il => { il.LoadConstantI4(left); il.LoadConstantI4(count); il.OpCode(op); }, expected);
            const long longLeft = -1234567890123456789;
            long longExpected = op switch
            {
                ILOpCode.Shl => longLeft << count,
                ILOpCode.Shr => longLeft >> count,
                _ => unchecked((long)((ulong)longLeft >> count))
            };
            await AssertExecutes(il =>
            {
                il.LoadConstantI8(longLeft); il.LoadConstantI4(count); il.OpCode(op);
                il.LoadConstantI8(longExpected); il.OpCode(ILOpCode.Ceq);
            }, 1);
        }
    }

    [Theory]
    [InlineData(ILOpCode.Ceq, -1, -1, 1)]
    [InlineData(ILOpCode.Cgt, -1, 1, 0)]
    [InlineData(ILOpCode.Cgt_un, -1, 1, 1)]
    [InlineData(ILOpCode.Clt, -1, 1, 1)]
    [InlineData(ILOpCode.Clt_un, -1, 1, 0)]
    [InlineData(ILOpCode.Ceq, long.MaxValue, long.MaxValue, 1)]
    [InlineData(ILOpCode.Cgt, -1L, 1L, 0)]
    [InlineData(ILOpCode.Cgt_un, -1L, 1L, 1)]
    [InlineData(ILOpCode.Clt, -1L, 1L, 1)]
    [InlineData(ILOpCode.Clt_un, -1L, 1L, 0)]
    [InlineData(ILOpCode.Ceq, 1.25, 1.25, 1)]
    [InlineData(ILOpCode.Cgt, -1.25, 1.25, 0)]
    [InlineData(ILOpCode.Cgt_un, -1.25, 1.25, 0)]
    [InlineData(ILOpCode.Clt, -1.25, 1.25, 1)]
    [InlineData(ILOpCode.Clt_un, -1.25, 1.25, 1)]
    [InlineData(ILOpCode.Ceq, double.NaN, 1.0, 0)]
    [InlineData(ILOpCode.Cgt, double.NaN, 1.0, 0)]
    [InlineData(ILOpCode.Clt, double.NaN, 1.0, 0)]
    [InlineData(ILOpCode.Cgt_un, double.NaN, 1.0, 1)]
    [InlineData(ILOpCode.Clt_un, 1.0, double.NaN, 1)]
    [InlineData(ILOpCode.Ceq, -0.0, 0.0, 1)]
    [InlineData(ILOpCode.Cgt_un, double.PositiveInfinity, double.PositiveInfinity, 0)]
    [InlineData(ILOpCode.Clt_un, double.NegativeInfinity, double.PositiveInfinity, 1)]
    public Task Comparisons(ILOpCode op, object left, object right, int expected) =>
        AssertExecutes(il => { Load(il, left); Load(il, right); il.OpCode(op); }, expected);

    [Theory]
    [InlineData(ILOpCode.Conv_i1, 255, -1)]
    [InlineData(ILOpCode.Conv_i1, 0x100000080L, -128)]
    [InlineData(ILOpCode.Conv_u1, -1, 255)]
    [InlineData(ILOpCode.Conv_i2, 65535, -1)]
    [InlineData(ILOpCode.Conv_i2, 0x100008000L, -32768)]
    [InlineData(ILOpCode.Conv_u2, -1L, 65535)]
    [InlineData(ILOpCode.Conv_i4, 0x1ffffffffL, -1)]
    [InlineData(ILOpCode.Conv_u4, 0x1ffffffffL, -1)]
    [InlineData(ILOpCode.Conv_i4, int.MinValue, int.MinValue)]
    [InlineData(ILOpCode.Conv_u4, int.MinValue, int.MinValue)]
    public Task NarrowIntegerConversions(ILOpCode op, object value, int expected) =>
        AssertExecutes(il => { Load(il, value); il.OpCode(op); }, expected);

    [Theory]
    [InlineData(ILOpCode.Conv_i8, -1, -1L)]
    [InlineData(ILOpCode.Conv_u8, -1, 4294967295L)]
    [InlineData(ILOpCode.Conv_i8, long.MinValue, long.MinValue)]
    [InlineData(ILOpCode.Conv_u8, -1L, -1L)]
    public Task WideIntegerConversions(ILOpCode op, object value, long expected) =>
        AssertExecutes(il =>
        {
            Load(il, value); il.OpCode(op); il.LoadConstantI8(expected); il.OpCode(ILOpCode.Ceq);
        }, 1);

    [Theory]
    [InlineData(ILOpCode.Conv_r4, 16777217, 16777216.0)]
    [InlineData(ILOpCode.Conv_r4, 16777217L, 16777216.0)]
    [InlineData(ILOpCode.Conv_r4, 0.1, (double)(float)0.1)]
    [InlineData(ILOpCode.Conv_r8, int.MinValue, -2147483648.0)]
    [InlineData(ILOpCode.Conv_r8, long.MinValue, -9223372036854775808.0)]
    [InlineData(ILOpCode.Conv_r8, 1.25, 1.25)]
    [InlineData(ILOpCode.Conv_r_un, -1, 4294967295.0)]
    [InlineData(ILOpCode.Conv_r_un, -1L, 18446744073709551616.0)]
    public Task FloatingPointConversions(ILOpCode op, object value, double expected) =>
        AssertExecutes(il =>
        {
            Load(il, value); il.OpCode(op); il.LoadConstantR8(expected); il.OpCode(ILOpCode.Ceq);
        }, 1);

    [Theory]
    [InlineData(42)]
    [InlineData(long.MaxValue)]
    [InlineData(1.25)]
    public Task PopDiscardsTheValue(object value) =>
        AssertExecutes(il => { Load(il, value); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(7); }, 7);

    [Theory]
    [InlineData(ILOpCode.Pop)]
    [InlineData(ILOpCode.Sub)]
    [InlineData(ILOpCode.Mul)]
    [InlineData(ILOpCode.Not)]
    [InlineData(ILOpCode.Ceq)]
    [InlineData(ILOpCode.Conv_i8)]
    public void RejectsStackUnderflow(ILOpCode op)
    {
        byte[] image = Program(il => il.OpCode(op));
        Assert.Contains("underflow", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void RejectsMixedIntegerWidths()
    {
        byte[] image = Program(il => { il.LoadConstantI4(1); il.LoadConstantI8(1); il.OpCode(ILOpCode.Sub); });
        Assert.Contains("type mismatch", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Theory]
    [InlineData(ILOpCode.Conv_i4)]
    [InlineData(ILOpCode.Conv_u8)]
    public void FloatingPointToIntegerConversionsRemainUnsupported(ILOpCode op)
    {
        byte[] image = Program(il => { il.LoadConstantR8(1.25); il.OpCode(op); });
        Assert.Contains("Double", Assert.Throws<NotSupportedException>(() => Emit(image)).Message);
    }

    [Fact]
    public void IntegerReturnRequiresExactlyOneInt32()
    {
        Assert.Contains("underflow", Assert.Throws<InvalidProgramException>(() => Emit(Program(_ => { }))).Message);
        Assert.Contains("type mismatch", Assert.Throws<InvalidProgramException>(() =>
            Emit(Program(il => il.LoadConstantI8(1)))).Message);
        Assert.Contains("empty evaluation stack", Assert.Throws<InvalidProgramException>(() =>
            Emit(Program(il => { il.LoadConstantI4(1); il.LoadConstantI4(2); }))).Message);
    }

    [Fact]
    public async Task HelloWorldWithIntegerEntryPointExecutes()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                public static int Main()
                {
                    System.Console.WriteLine("Hello World!");
                    return 0;
                }
            }
            """);
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
        var result = await Repl.Node.ExecuteAsync(wasm);
        Assert.Equal("Hello World!\n", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task LongConsoleOutputUsesTheTestRewriter()
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile("System.Console.WriteLine(1L); System.Console.WriteLine(long.MinValue);"));
        using var assembly = AssemblyCompiler.Read(stream);
        byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
        Assert.Equal("1\n-9223372036854775808\n", (await Repl.Node.ExecuteAsync(wasm)).Output);
    }

    private static void Load(InstructionEncoder il, object value)
    {
        switch (value)
        {
            case int number: il.LoadConstantI4(number); break;
            case long number: il.LoadConstantI8(number); break;
            case double number: il.LoadConstantR8(number); break;
            default: throw new NotSupportedException(value.GetType().Name);
        }
    }

    private static async Task AssertExecutes(Action<InstructionEncoder> emit, int expected)
    {
        byte[] image = Program(emit);
        // Execute the very same IL on .NET as well as through our WASM backend.
        Assert.Equal(expected, (int)Assembly.Load(image).EntryPoint!.Invoke(null, null)!);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Empty(result.Output);
        Assert.Equal(expected, result.ExitCode);
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly);
    }

    private static byte[] Program(Action<InstructionEncoder> emit)
    {
        // Reserve a tiny body using Roslyn, then replace only its IL. This avoids constant folding
        // and tests real MSIL instructions without implementing locals or calls just for the tests.
        string padding = string.Concat(Enumerable.Repeat("System.Console.WriteLine(0);", 8));
        byte[] image = Frontend.Roslyn.Compile($"public static class Program {{ public static int Main() {{ {padding} return 0; }} }}");
        using var stream = new MemoryStream(image);
        using var pe = new PEReader(stream);
        var entry = (MethodDefinitionHandle)MetadataTokens.EntityHandle(pe.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress);
        int rva = pe.GetMetadataReader().GetMethodDefinition(entry).RelativeVirtualAddress;
        var section = pe.PEHeaders.SectionHeaders.Single(section => rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
        int header = section.PointerToRawData + rva - section.VirtualAddress;
        Assert.Equal(2, image[header] & 3);
        int length = image[header] >> 2;
        var bytes = new BlobBuilder();
        var il = new InstructionEncoder(bytes);
        emit(il);
        il.OpCode(ILOpCode.Ret);
        Assert.True(bytes.Count <= length);
        image[header] = (byte)((bytes.Count << 2) | 2); // Keep the tiny header length exact; no fall-through padding.
        image.AsSpan(header + 1, length).Clear();
        bytes.ToArray().CopyTo(image, header + 1);
        return image;
    }
}
