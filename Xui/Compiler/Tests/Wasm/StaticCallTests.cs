using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm;

public class StaticCallTests
{
    [Theory]
    [InlineData("bool", "true", "value", "True")]
    [InlineData("char", "'A'", "(int)value", "65")]
    [InlineData("sbyte", "-12", "(int)value", "-12")]
    [InlineData("byte", "250", "(int)value", "250")]
    [InlineData("short", "-1234", "(int)value", "-1234")]
    [InlineData("ushort", "60000", "(int)value", "60000")]
    [InlineData("int", "-42", "value", "-42")]
    [InlineData("uint", "4000000000", "(long)value", "4000000000")]
    [InlineData("long", "long.MinValue", "value", "-9223372036854775808")]
    [InlineData("ulong", "10000000000", "(long)value", "10000000000")]
    [InlineData("float", "1.25f", "value", "1.25")]
    [InlineData("double", "2.5", "value", "2.5")]
    [InlineData("nint", "-7", "(int)value", "-7")]
    [InlineData("nuint", "8", "(int)value", "8")]
    [InlineData("string", "\"identity\"", "value", "identity")]
    public async Task PrimitiveParametersAndResultsRoundTrip(string type, string literal, string printed, string expected)
    {
        byte[] image = Frontend.Roslyn.Compile($$"""
            public static class Program
            {
                static {{type}} Identity({{type}} value) => value;
                public static int Main()
                {
                    {{type}} value = Identity({{literal}});
                    System.Console.WriteLine({{printed}});
                    return 0;
                }
            }
            """);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal(expected + "\n", result.Output);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task CallsPreserveArgumentOrderAndFloatStores()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                static double Combine(int a, float b, double c, int d)
                {
                    b += 0.5f;
                    return a * 1000 + b * 100 + c * 10 + d;
                }
                public static void Main() => System.Console.WriteLine(Combine(1, 2.5f, 3.25, 4));
            }
            """);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal("1336.5\n", result.Output);
    }

    [Fact]
    public async Task FloatArgumentsAndResultsRoundAtMethodBoundaries()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                static float One() => 1f;
                static double Combine(float a, float b) => (double)a + b;
                static float Sum(float a, float b) => a + b;
                public static void Main()
                {
                    System.Console.WriteLine(Combine(16777216f + One(), 33554432f + One()));
                    System.Console.WriteLine((double)Sum(16777216f, One()));
                }
            }
            """);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal("50331648\n16777216\n", result.Output);
    }

    [Fact]
    public async Task ParametersAndLocalsHaveIndependentIndices()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                static int Change(int a, int b)
                {
                    int copy = a;
                    a = b + 1;
                    System.Console.WriteLine(copy);
                    return a + copy + b;
                }
                public static int Main() => Change(7, 3);
            }
            """);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal("7\n", result.Output);
        Assert.Equal(14, result.ExitCode);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(255)]
    [InlineData(256)]
    public async Task ArgumentIndexFormsAndStores(int index)
    {
        string parameters = string.Join(", ", Enumerable.Range(0, index + 1).Select(i => $"int p{i}"));
        string arguments = string.Join(", ", Enumerable.Range(0, index + 1));
        byte[] image = Frontend.Roslyn.Compile($$"""
            public static class Program
            {
                static int Sum({{parameters}})
                {
                    p{{index}} = p0 + p1 + p2 + p3 + p{{index}};
                    return p{{index}};
                }
                public static int Main() => Sum({{arguments}});
            }
            """);
        await AssertReturnMatchesDotNet(image, index + 6);
        string wat = await WasmTools.ToWatAsync(Emit(image));
        Assert.Contains($"local.set {index}", wat);
    }

    [Fact]
    public Task NestedForwardCallsAndOverloads() => AssertReturnMatchesDotNet(Frontend.Roslyn.Compile("""
        public static class Program
        {
            public static int Main() => Twice(Add(2, 3)) + (int)Add(1L, 2L);
            static int Twice(int x) => Add(x, x);
            static int Add(int a, int b) => a + b;
            static long Add(long a, long b) => a + b + 100;
            static int Unused() => System.Math.Abs(-5);
        }
        """), 113);

    [Fact]
    public async Task RewriterAndStringLiteralsWorkInCallees()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                static string Text() => "callee";
                static void Print(string text) { System.Console.WriteLine(text); System.Console.WriteLine(true); }
                public static void Main() { Print(Text()); Print(Text()); }
            }
            """);
        byte[] wasm = Emit(image);
        Assert.Equal("callee\nTrue\ncallee\nTrue\n", (await Repl.Node.ExecuteAsync(wasm)).Output);
        string wat = await WasmTools.ToWatAsync(wasm);
        Assert.Equal(1, wat.Split("(data (;").Length - 1);
    }

    [Fact]
    public async Task RecursiveEdgesArePlannedOnce()
    {
        // Emit/print the recursive graph; do not execute a deliberately infinite recursion.
        byte[] wasm = Emit(Frontend.Roslyn.Compile("""
            public static class Program
            {
                public static int Main() => First(1);
                static int First(int x) => Second(x);
                static int Second(int x) => First(x);
            }
            """));
        string wat = await WasmTools.ToWatAsync(wasm);
        Assert.Equal(3, wat.Split("\n  (func (;").Length - 1);
    }

    [Theory]
    [InlineData("int", "7", "x", "7")]
    [InlineData("long", "5000000000L", "x", "5000000000")]
    [InlineData("float", "1.25f", "x", "1.25")]
    [InlineData("double", "2.5", "x", "2.5")]
    [InlineData("nint", "(nint)7", "(int)x", "7")]
    [InlineData("string", "\"dup\"", "x", "dup")]
    public async Task DupPreservesTheValueAndStackType(string type, string literal, string printed, string expected)
    {
        string assignment = printed.Replace("x", $"(x = {literal})");
        byte[] image = Frontend.Roslyn.Compile($$"""
            {{type}} x;
            System.Console.WriteLine({{assignment}});
            System.Console.WriteLine({{printed}});
            """);
        byte[] wasm = Emit(image);
        Assert.Contains("local.tee", await WasmTools.ToWatAsync(wasm));
        Assert.Equal(expected + "\n" + expected + "\n", (await Repl.Node.ExecuteAsync(wasm)).Output);
    }

    [Fact]
    public async Task DupInsideCalleeDoesNotOverwriteParametersOrLocals()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program
            {
                static int Work(int a, int b)
                {
                    int x;
                    System.Console.WriteLine(x = a + b);
                    return x + a + b;
                }
                public static int Main() => Work(2, 3);
            }
            """);
        byte[] wasm = Emit(image);
        Assert.Contains("local.tee", await WasmTools.ToWatAsync(wasm));
        var result = await Repl.Node.ExecuteAsync(wasm);
        Assert.Equal("5\n", result.Output);
        Assert.Equal(10, result.ExitCode);
    }

    [Fact]
    public void DupRejectsAnEmptyStack()
    {
        byte[] image = Frontend.Roslyn.Compile("public static class Program { public static int Main() => 1; }");
        ReplaceFirstOpcode(image, "Main", ILOpCode.Ldc_i4_1, ILOpCode.Dup);
        Assert.Contains("underflow", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void CallRejectsMissingArguments()
    {
        byte[] image = Frontend.Roslyn.Compile("public static class Program { static int F(int x) => x; public static int Main() => F(1); }");
        ReplaceFirstOpcode(image, "Main", ILOpCode.Ldc_i4_1, ILOpCode.Nop);
        Assert.Contains("underflow", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void CallRejectsWrongArgumentTypes()
    {
        byte[] image = Frontend.Roslyn.Compile("public static class Program { static double F(double x) => x; public static void Main() { System.Console.WriteLine(F(1.25)); } }");
        ReplaceFirstOpcode(image, "Main", ILOpCode.Ldc_r8, ILOpCode.Ldc_i8);
        Assert.Contains("type mismatch", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void ReturnRejectsWrongResultTypes()
    {
        byte[] image = Frontend.Roslyn.Compile("public static class Program { static double F() => 1.25; public static void Main() { System.Console.WriteLine(F()); } }");
        ReplaceFirstOpcode(image, "F", ILOpCode.Ldc_r8, ILOpCode.Ldc_i8);
        Assert.Contains("type mismatch", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public void ArgumentIndexIsChecked()
    {
        byte[] image = Frontend.Roslyn.Compile("public static class Program { static int F(int x) => x; public static int Main() => F(1); }");
        ReplaceFirstOpcode(image, "F", ILOpCode.Ldarg_0, ILOpCode.Ldarg_3);
        Assert.Contains("Invalid argument index", Assert.Throws<InvalidProgramException>(() => Emit(image)).Message);
    }

    [Fact]
    public Task PrimitiveRefParametersExecute() => AssertReturnMatchesDotNet(Frontend.Roslyn.Compile(
        "public static class Program { static int F(ref int x) => x; public static int Main() { int x = 1; return F(ref x); } }"), 1);

    [Theory]
    [InlineData("public static class Program { static Program() { System.Console.WriteLine(1); } public static int Main() => 0; }")]
    [InlineData("public static class Program { public static int Main(string[] args) => args.Length; }")]
    [InlineData("public static class Program { [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)] static int F() => 1; public static int Main() => F(); }")]
    [InlineData("public static class Program { [System.Runtime.CompilerServices.ModuleInitializer] public static void Init() { } public static int Main() => 1; }")]
    public void UnsupportedMethodFeaturesFailExplicitly(string source) =>
        Assert.Throws<NotSupportedException>(() => Emit(Frontend.Roslyn.Compile(source)));

    private static async Task AssertReturnMatchesDotNet(byte[] image, int expected)
    {
        Assert.Equal(expected, (int)Assembly.Load(image).EntryPoint!.Invoke(null, null)!);
        var result = await Repl.Node.ExecuteAsync(Emit(image));
        Assert.Equal(expected, result.ExitCode);
        Assert.Empty(result.Output);
    }

    private static byte[] Emit(byte[] image)
    {
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }

    private static void ReplaceFirstOpcode(byte[] image, string name, ILOpCode expected, ILOpCode replacement)
    {
        using var stream = new MemoryStream(image);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var method = metadata.MethodDefinitions.Select(metadata.GetMethodDefinition)
            .Single(method => metadata.GetString(method.Name) == name);
        int rva = method.RelativeVirtualAddress;
        var section = pe.PEHeaders.SectionHeaders.Single(section => rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize);
        int header = section.PointerToRawData + rva - section.VirtualAddress;
        int start = header + ((image[header] & 3) == 2 ? 1 : (image[header + 1] >> 4) * 4);
        Assert.Equal((byte)expected, image[start]);
        image[start] = (byte)replacement;
    }
}
