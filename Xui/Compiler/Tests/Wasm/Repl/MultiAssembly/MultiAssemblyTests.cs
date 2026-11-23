using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.Reachability;
using Xunit;
using Xunit.Abstractions;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.Wasm.Repl.MultiAssembly;

public class MultiAssemblyTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThreeAssembliesExecuteInOneModule()
    {
        var images = CompileFixtures();
        using var numbers = Read(images.Numbers);
        using var reports = Read(images.Reports);
        using var application = Read(images.Application);
        // Deliberately supply the caller before its dependencies. No dependency is an implicit root.
        var compilation = new Xui.Compiler.XuiNet.Compilation(application, reports, numbers);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(application, new ConsoleWriteLineRewriter());
        var result = await Node.ExecuteAsync(wasm);
        Assert.Equal("From Application.exe\nFrom Reports.dll\n41\n42\n", result.Output);
        Assert.Equal(42, result.ExitCode);

        var methods = emitter.Dependencies!.Methods;
        Assert.Equal(3, methods.Count);
        Assert.Equal(3, methods.Select(method => method.DeclaringType.Module).Distinct().Count());
        Assert.DoesNotContain(emitter.Dependencies.Nodes.OfType<Dependency.Type>(), type => type.Identity.ToString() == "Numbers.Unused");
        Assert.Equal(3, compilation.Assemblies.Count);
        string wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
        string sources = string.Join("\n\n", new[] { "Numbers", "Reports", "Program" }.Select(name =>
            $"### {name}.cs\n\nSource: [{name}.cs](Fixtures/MultiAssembly/{name}.cs)\n\n```csharp\n{Source(name).TrimEnd()}\n```"));
        string retained = string.Join("\n", methods.Select(method => $"- `{method.DeclaringType.Module.Name}`: `{method}`"));
        string markdown = $"""
            # Three assemblies, one WASM module

            `Application.exe` calls `Reports.dll` and `Numbers.dll`; `Reports.dll` also calls `Numbers.dll`.
            Each DLL is compiled separately. Only `Application.Main` is a root.

            ## Source

            {sources}

            ## Retained methods

            {retained}

            The unused class and the `long` overload are omitted. Console calls in both assemblies use the test host rewrite.

            ## Output

            ```text
            {result.Output.TrimEnd()}
            ```

            ## Exit Code

            ```text
            {result.ExitCode}
            ```

            ## WAT

            ```wat
            {wat}
            ```
            """ + "\n";
        output.WriteLine(markdown);
        Snapshot.AssertMatches("ThreeAssemblies", markdown);
    }

    [Fact]
    public void MissingImplementationFailsAtTheCallSite()
    {
        var images = CompileFixtures();
        using var reports = Read(images.Reports);
        using var application = Read(images.Application);
        _ = new Xui.Compiler.XuiNet.Compilation(application, reports);
        var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(application, new ConsoleWriteLineRewriter()));
        Assert.Contains("Call at IL_", error.Message);
        Assert.Contains("Numbers.Arithmetic", error.Message);
        Assert.Contains("implementation assembly", error.Message);
    }

    [Fact]
    public void DifferentAssemblyVersionCannotSilentlySatisfyAReference()
    {
        var images = CompileFixtures();
        using var wrongNumbers = Read(Frontend.Roslyn.Compile(Source("Numbers").Replace("1.0.0.0", "2.0.0.0"),
            OutputKind.DynamicallyLinkedLibrary, "Numbers"));
        using var reports = Read(images.Reports);
        using var application = Read(images.Application);
        _ = new Xui.Compiler.XuiNet.Compilation(application, reports, wrongNumbers);
        var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(application, new ConsoleWriteLineRewriter()));
        Assert.Contains("Assembly identity mismatch for Numbers", error.Message);
    }

    private static (byte[] Numbers, byte[] Reports, byte[] Application) CompileFixtures()
    {
        byte[] numbers = Frontend.Roslyn.Compile(Source("Numbers"), OutputKind.DynamicallyLinkedLibrary, "Numbers");
        byte[] reports = Frontend.Roslyn.Compile(Source("Reports"), OutputKind.DynamicallyLinkedLibrary, "Reports",
            additionalReferences: [MetadataReference.CreateFromImage(numbers)]);
        byte[] application = Frontend.Roslyn.Compile(Source("Program"), assemblyName: "Application",
            additionalReferences: [MetadataReference.CreateFromImage(numbers), MetadataReference.CreateFromImage(reports)]);
        return (numbers, reports, application);
    }

    private static Model.Assembly Read(byte[] image)
    {
        using var stream = new MemoryStream(image);
        return AssemblyCompiler.Read(stream);
    }

    private static string Source(string name, [CallerFilePath] string file = "") =>
        File.ReadAllText(Path.Combine(Path.GetDirectoryName(file)!, "..", "..", "..", "Fixtures", "MultiAssembly", name + ".cs"));
}
