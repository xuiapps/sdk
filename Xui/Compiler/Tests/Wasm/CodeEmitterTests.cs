using Xui.Compiler.Tests.Wasm.Rewrite;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class CodeEmitterTests
{
    [Fact]
    public void CallsConsumeTwoStackedStrings()
    {
        var (image, offset) = Compile("System.Console.WriteLine(\"first\"); System.Console.WriteLine(\"second\");");
        // Change ldstr/call/ldstr/call into ldstr/ldstr/call/call.
        byte[] call = image.AsSpan(offset + 5, 5).ToArray();
        image.AsSpan(offset + 10, 5).CopyTo(image.AsSpan(offset + 5, 5));
        call.CopyTo(image, offset + 10);
        Assert.NotEmpty(Emit(image));
    }

    [Fact]
    public void CallRejectsMissingArgument()
    {
        var (image, offset) = Compile("System.Console.WriteLine(\"hello\");");
        // Replace the initial ldstr with a call, leaving no argument on the stack.
        image.AsSpan(offset + 5, 5).CopyTo(image.AsSpan(offset, 5));
        var error = Assert.Throws<InvalidProgramException>(() => Emit(image));
        Assert.Contains("underflow", error.Message);
        Assert.Contains("IL_0000", error.Message);
    }

    [Fact]
    public void VoidReturnRejectsUnconsumedValue()
    {
        var (image, offset) = Compile("System.Console.WriteLine(\"hello\");");
        // Replace the call with nops; they must preserve the pushed string until ret.
        image.AsSpan(offset + 5, 5).Clear();
        var error = Assert.Throws<InvalidProgramException>(() => Emit(image));
        Assert.Contains("requires an empty evaluation stack", error.Message);
        Assert.Contains("IL_000a", error.Message);
    }

    [Theory]
    [InlineData("1", "1", 1, 1, "2\n", "i32.add")]
    [InlineData("2147483647", "1", 5, 1, "-2147483648\n", "i32.add")]
    [InlineData("1.25f", "2.5f", 5, 5, "3.75\n", "f64.add")]
    [InlineData("1.25", "2.5", 9, 9, "3.75\n", "f64.add")]
    [InlineData("1.25f", "2.5", 5, 9, "3.75\n", "f64.add")]
    public async Task AddExecutesInNode(string left, string right, int leftSize, int rightSize,
        string expected, string wasmInstruction)
    {
        // Roslyn folds constant addition. Rewrite load/call/load/call into load/load/add/call
        // so this test exercises the MSIL add instruction without requiring locals or method calls.
        var (image, offset) = CompileImage($"System.Console.WriteLine({left}); System.Console.WriteLine({right});");
        Assert.Equal((byte)ILOpCode.Call, image[offset + leftSize]);
        Assert.Equal((byte)ILOpCode.Call, image[offset + leftSize + 5 + rightSize]);
        byte[] secondLoad = image.AsSpan(offset + leftSize + 5, rightSize).ToArray();
        byte[] call = image.AsSpan(offset + leftSize + 5 + rightSize, 5).ToArray();
        image.AsSpan(offset + leftSize, rightSize + 10).Clear();
        secondLoad.CopyTo(image, offset + leftSize);
        image[offset + leftSize + rightSize] = (byte)ILOpCode.Add;
        call.CopyTo(image, offset + leftSize + rightSize + 1);

        byte[] wasm = Emit(image);
        Assert.Contains(wasmInstruction, await WasmTools.ToWatAsync(wasm));
        Assert.Equal(expected, (await Repl.Node.ExecuteAsync(wasm)).Output);
    }

    [Fact]
    public async Task IntegerConstantFormsExecuteInNode()
    {
        int[] values = [-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, -128, 127, -129, 128, int.MinValue, int.MaxValue];
        string source = string.Join("\n", values.Select(value => $"System.Console.WriteLine({value});"));
        Assert.Equal(string.Join("\n", values) + "\n",
            (await Repl.Node.ExecuteAsync(Emit(Frontend.Roslyn.Compile(source)))).Output);
    }

    [Fact]
    public void AddRejectsMissingOperand()
    {
        var (image, offset) = Compile("System.Console.WriteLine(\"hello\");");
        image[offset] = (byte)ILOpCode.Add;
        image.AsSpan(offset + 1, 4).Clear();
        var error = Assert.Throws<InvalidProgramException>(() => Emit(image));
        Assert.Contains("underflow", error.Message);
    }

    [Fact]
    public void CallRejectsWrongArgumentType()
    {
        var (image, offset) = Compile("System.Console.WriteLine(\"hello\");");
        image.AsSpan(offset, 5).Clear();
        image[offset] = (byte)ILOpCode.Ldc_i4_1;
        var error = Assert.Throws<InvalidProgramException>(() => Emit(image));
        Assert.Contains("type mismatch", error.Message);
    }

    private static (byte[] Image, int Offset) Compile(string source)
    {
        var (image, offset) = CompileImage(source);
        Assert.Equal((byte)ILOpCode.Ldstr, image[offset]);
        Assert.Equal((byte)ILOpCode.Call, image[offset + 5]);
        return (image, offset);
    }

    private static (byte[] Image, int Offset) CompileImage(string source)
    {
        byte[] image = Frontend.Roslyn.Compile(source);
        using var stream = new MemoryStream(image);
        using var pe = new PEReader(stream);
        var entryPoint = (MethodDefinitionHandle)MetadataTokens.EntityHandle(
            pe.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress);
        int rva = pe.GetMetadataReader().GetMethodDefinition(entryPoint).RelativeVirtualAddress;
        var section = pe.PEHeaders.SectionHeaders.Single(section =>
            rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
        int header = section.PointerToRawData + rva - section.VirtualAddress;
        Assert.Equal(2, image[header] & 3); // Tiny method header; IL starts one byte later.
        return (image, header + 1);
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }
}
