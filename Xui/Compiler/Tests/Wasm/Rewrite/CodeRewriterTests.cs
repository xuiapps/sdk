using System.Reflection.Metadata;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Rewrite;

public class CodeRewriterTests
{
    [Fact]
    public void DefaultRewriterDoesNotSubstituteConsoleWriteLine()
    {
        var error = Assert.Throws<NotSupportedException>(() => Emit("System.Console.WriteLine(1);"));
        Assert.Contains("Call at IL_0001", error.Message);
    }

    [Fact]
    public async Task NoOpRewriterLeavesNormalEmissionIntact()
    {
        byte[] wasm = Emit("return;");
        Assert.DoesNotContain("console-log", await WasmTools.ToWatAsync(wasm));
        Assert.Equal("", (await Repl.Node.ExecuteAsync(wasm)).Output);
    }

    [Fact]
    public void TestRewriterDoesNotClaimOtherConsoleMethods()
    {
        var error = Assert.Throws<NotSupportedException>(() =>
            Emit("System.Console.Write(1);", new ConsoleWriteLineRewriter()));
        Assert.Contains("Call at IL_0001", error.Message);
    }

    [Fact]
    public async Task RewriterCanBeReusedAcrossModules()
    {
        var rewriter = new ConsoleWriteLineRewriter();
        Assert.Equal("1\nfirst\n", (await Repl.Node.ExecuteAsync(
            Emit("System.Console.WriteLine(1); System.Console.WriteLine(\"first\");", rewriter))).Output);
        Assert.Equal("second\n", (await Repl.Node.ExecuteAsync(
            Emit("System.Console.WriteLine(\"second\");", rewriter))).Output);
    }

    [Fact]
    public async Task RewriterReplacesConstantsAndPreservesDeclinedOperands()
    {
        // The short constant is inspected then declined; its operand must remain for normal emission.
        // The full-width constant is replaced before the built-in opcode emitter runs.
        byte[] wasm = Emit("System.Console.WriteLine(-42); System.Console.WriteLine(123456);",
            new ConstantRewriter());
        Assert.Equal("-42\n99\n", (await Repl.Node.ExecuteAsync(wasm)).Output);
    }

    private sealed class ConstantRewriter : ConsoleWriteLineRewriter
    {
        public override bool Collect(Instruction instruction, ref ILReader reader)
        {
            if (instruction.Code == OpCode.Ldc_i4_s)
            {
                _ = instruction.ReadShortInt(ref reader);
                return false;
            }
            if (instruction.Code == OpCode.Ldc_i4)
            {
                _ = instruction.ReadInt(ref reader);
                return true;
            }
            return base.Collect(instruction, ref reader);
        }

        public override bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset)
        {
            if (instruction.Code == OpCode.Ldc_i4_s)
            {
                _ = instruction.ReadShortInt(ref reader);
                return false;
            }
            if (instruction.Code == OpCode.Ldc_i4)
            {
                _ = instruction.ReadInt(ref reader);
                emitter.Writer.I32Const(99);
                emitter.Push(new MsilStackEntry(SignatureTypeCode.Int32));
                return true;
            }
            return base.TryRewrite(instruction, ref reader, emitter, offset);
        }
    }

    private static byte[] Emit(string source, CodeRewriter? rewriter = null)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source));
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, rewriter);
    }
}
