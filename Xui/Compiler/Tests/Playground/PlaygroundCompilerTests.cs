using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xunit;

namespace Xui.Compiler.Tests.Playground;

public class PlaygroundCompilerTests
{
    [Theory]
    [InlineData("using System; Console.WriteLine(\"Hello World!\");", "Hello World!\n", 0)]
    [InlineData("using System; int sum = 0; for (int i = 1; i <= 5; i++) sum += i; Console.WriteLine(sum); return sum;", "15\n", 15)]
    [InlineData("using System; Console.WriteLine(true); Console.WriteLine(1234567890123L); Console.WriteLine(1.25f);", "True\n1234567890123\n1.25\n", 0)]
    public async Task EmbeddedReferencesCompileAndExecute(string source, string output, int exitCode)
    {
        var result = await Node.ExecuteAsync(PlaygroundCompiler.Compile(source));
        Assert.Equal(output, result.Output);
        Assert.Equal(exitCode, result.ExitCode);
    }

    [Fact]
    public async Task GenericVirtualCallsWorkWithReferenceAssemblyMetadata()
    {
        const string source = """
            using System;
            Box<int> box = new NumberBox(40);
            Console.WriteLine(box.Read());
            Console.WriteLine(box is NumberBox);
            return box.Read();
            class Box<T> { public T Value; public Box(T value) { Value = value; } public virtual T Read() => Value; }
            class NumberBox : Box<int> { public NumberBox(int value) : base(value) { } public override int Read() => base.Read() + 2; }
            """;
        Assert.Equal(new NodeResult("42\nTrue\n", 42), await Node.ExecuteAsync(PlaygroundCompiler.Compile(source)));
    }

    [Fact]
    public void SyntaxErrorsIncludeTheEditorLocation()
    {
        var error = Assert.Throws<InvalidOperationException>(() => PlaygroundCompiler.Compile("using System;\nConsole.WriteLine( ;"));
        Assert.Contains("Program.cs(2,", error.Message);
        Assert.Contains("error CS", error.Message);
    }

    [Fact]
    public void UnsupportedEmissionIsReportedWithoutExecutingManagedCode()
    {
        Assert.Throws<NotSupportedException>(() => PlaygroundCompiler.Compile("throw new System.Exception(\"must not execute\");"));
    }
}
