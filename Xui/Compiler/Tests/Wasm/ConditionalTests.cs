using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Rewrite;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class ConditionalTests
{
    [Theory]
    [InlineData("If", -1, -1)]
    [InlineData("If", 2, 12)]
    [InlineData("IfElse", -1, -11)]
    [InlineData("IfElse", 2, 12)]
    [InlineData("ElseIf", -1, -1)]
    [InlineData("ElseIf", 0, 0)]
    [InlineData("ElseIf", 2, 1)]
    [InlineData("EarlyReturn", -1, 1)]
    [InlineData("EarlyReturn", 0, 10)]
    [InlineData("EarlyReturn", 2, 3)]
    [InlineData("ConditionalValue", -1, 20)]
    [InlineData("ConditionalValue", 2, 10)]
    [InlineData("StackPrefix", -1, 25)]
    [InlineData("StackPrefix", 2, 15)]
    public async Task FixturesExecuteBothArms(string fixture, int value, int expected)
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "Fixtures", "CodeFlow", fixture + ".cs"));
        source += $"\npublic static class Program {{ public static int Main() => {fixture}.Run({value}); }}";
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var wasm = Emit(Frontend.Roslyn.Compile(source, optimizationLevel: optimization));
            Assert.Equal(expected, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
        }
    }

    [Theory]
    [InlineData(0, 12)]
    [InlineData(1, 8)]
    public async Task BlockConsumesOneInputAndProducesTwoResults(int condition, int expected)
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var otherwise = il.DefineLabel(); var join = il.DefineLabel();
            il.LoadConstantI4(1); il.LoadConstantI4(2); il.LoadConstantI4(condition);
            il.Branch(ILOpCode.Brfalse_s, otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(3); il.LoadConstantI4(4); il.Branch(ILOpCode.Br_s, join);
            il.MarkLabel(otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI4(5); il.LoadConstantI4(6);
            il.MarkLabel(join); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        });
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var graph = Analyzer.Analyze(assembly.ManifestModule, assembly.ManifestModule.EntryPoint);
        var block = Assert.Single(StructuredBlocks.Build(graph).Children.OfType<ConditionalBlock>());
        Assert.Equal(1, block.Preserved.Depth);
        Assert.Equal(new StackEntry[] { StackType.Int32 }, block.Parameters);
        Assert.Equal(new StackEntry[] { StackType.Int32, StackType.Int32 }, block.Results);
        var wasm = new WasmEmitter().Emit(assembly);
        Assert.Equal(expected, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
        Assert.Contains("(param i32) (result i32 i32)", await WasmTools.ToWatAsync(wasm));
    }

    [Theory]
    [InlineData(0, -5)]
    [InlineData(1, 10)]
    public async Task ReadsCountAsBlockInputsEvenWhenDepthDoesNotDecrease(int condition, int expected)
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var otherwise = il.DefineLabel(); var join = il.DefineLabel();
            il.LoadConstantI4(5); il.LoadConstantI4(condition); il.Branch(ILOpCode.Brfalse, otherwise);
            il.OpCode(ILOpCode.Dup); il.OpCode(ILOpCode.Add); il.Branch(ILOpCode.Br, join);
            il.MarkLabel(otherwise); il.OpCode(ILOpCode.Neg);
            il.MarkLabel(join); il.OpCode(ILOpCode.Ret);
        });
        byte[] wasm = Emit(image);
        Assert.Equal(expected, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
        Assert.Contains("(param i32) (result i32)", await WasmTools.ToWatAsync(wasm));
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(1, 6)]
    public async Task BlockSignaturesPreserveMixedTypesAndResultOrder(int condition, int expected)
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var otherwise = il.DefineLabel(); var join = il.DefineLabel();
            il.LoadConstantI4(1); il.LoadConstantR8(2.5); il.LoadConstantI4(condition);
            il.Branch(ILOpCode.Brfalse_s, otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI8(5); il.LoadConstantR8(1.25); il.Branch(ILOpCode.Br_s, join);
            il.MarkLabel(otherwise);
            il.OpCode(ILOpCode.Pop); il.LoadConstantI8(7); il.LoadConstantR8(2.5);
            il.MarkLabel(join); il.OpCode(ILOpCode.Pop); il.OpCode(ILOpCode.Conv_i4); il.OpCode(ILOpCode.Add); il.OpCode(ILOpCode.Ret);
        });
        byte[] wasm = Emit(image);
        Assert.Equal(expected, (await Repl.Node.ExecuteAsync(wasm)).ExitCode);
        Assert.Contains("(param f64) (result i64 f64)", await WasmTools.ToWatAsync(wasm));
    }

    [Fact]
    public async Task UnreachableUnsupportedInstructionsAreOmitted()
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var live = il.DefineLabel(); il.Branch(ILOpCode.Br_s, live);
            il.OpCode(ILOpCode.Ldtoken); il.Token(MetadataTokens.TypeDefinitionHandle(1)); il.OpCode(ILOpCode.Pop);
            il.MarkLabel(live); il.LoadConstantI4(7); il.OpCode(ILOpCode.Ret);
        });
        Assert.Equal(7, (await Repl.Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    [Fact]
    public void LoopWithoutAContinuationIsRejected()
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var repeat = il.DefineLabel(); il.MarkLabel(repeat); il.Branch(ILOpCode.Br, repeat);
        });
        Assert.Contains("Code flow jumps unsupported", Assert.Throws<NotSupportedException>(() => Emit(image)).Message);
    }

    [Fact]
    public void MismatchedArmTypesFailBeforeEmission()
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var otherwise = il.DefineLabel(); var join = il.DefineLabel();
            il.LoadConstantI4(0); il.Branch(ILOpCode.Brfalse_s, otherwise);
            il.LoadConstantI4(1); il.Branch(ILOpCode.Br_s, join);
            il.MarkLabel(otherwise); il.LoadConstantR8(1);
            il.MarkLabel(join); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
        });
        Assert.Contains("Stack mismatch", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public async Task TopLevelConditionalRetainsUnusedEntryPointArguments()
    {
        var image = Frontend.Roslyn.Compile("""
            static int Value() => 2;
            int value = Value();
            if (value > 0) return 7;
            else return 8;
            """);
        Assert.Equal(7, (await Repl.Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    public static IEnumerable<object[]> FloatingBranches()
    {
        foreach (string name in new[] { "Beq", "Bne_un", "Bge", "Bge_un", "Bgt", "Bgt_un", "Ble", "Ble_un", "Blt", "Blt_un" })
        foreach (string suffix in new[] { "", "_s" })
        foreach (double left in new[] { 1.0, 2.0, 3.0, double.NaN })
        {
            bool expected = name switch
            {
                "Beq" => left == 2,
                "Bne_un" => left != 2,
                "Bge" => left >= 2,
                "Bge_un" => !(left < 2),
                "Bgt" => left > 2,
                "Bgt_un" => !(left <= 2),
                "Ble" => left <= 2,
                "Ble_un" => !(left > 2),
                "Blt" => left < 2,
                "Blt_un" => !(left >= 2),
                _ => throw new InvalidOperationException()
            };
            yield return [Enum.Parse<ILOpCode>(name + suffix), left, expected];
        }
    }

    [Theory]
    [MemberData(nameof(FloatingBranches))]
    public async Task FloatingBranchesPreserveOrderedAndUnorderedComparisons(ILOpCode code, double left, bool expected)
    {
        var image = Frontend.Msil.Compile(il =>
        {
            var taken = il.DefineLabel();
            il.LoadConstantR8(left); il.LoadConstantR8(2); il.Branch(code, taken);
            il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
            il.MarkLabel(taken); il.LoadConstantI4(1); il.OpCode(ILOpCode.Ret);
        });
        Assert.Equal(expected ? 1 : 0, (await Repl.Node.ExecuteAsync(Emit(image))).ExitCode);
    }

    [Theory]
    [InlineData(ILOpCode.Blt, true)]
    [InlineData(ILOpCode.Blt_un, false)]
    [InlineData(ILOpCode.Bge, false)]
    [InlineData(ILOpCode.Bge_un, true)]
    public async Task IntegerComparisonsRetainSignednessAtBothWidths(ILOpCode code, bool expected)
    {
        foreach (bool wide in new[] { false, true })
        {
            var image = Frontend.Msil.Compile(il =>
            {
                var taken = il.DefineLabel();
                if (wide) { il.LoadConstantI8(-1); il.LoadConstantI8(1); }
                else { il.LoadConstantI4(-1); il.LoadConstantI4(1); }
                il.Branch(code, taken);
                il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
                il.MarkLabel(taken); il.LoadConstantI4(1); il.OpCode(ILOpCode.Ret);
            });
            Assert.Equal(expected ? 1 : 0, (await Repl.Node.ExecuteAsync(Emit(image))).ExitCode);
        }
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
