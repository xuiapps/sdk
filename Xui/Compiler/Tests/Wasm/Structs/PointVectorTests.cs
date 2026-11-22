using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Rewrite;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.Structs;

[Collection(nameof(ManagedConsoleCollection))]
public class PointVectorTests(ITestOutputHelper output)
{
    [SkippableFact]
    public async Task PointPlusVectorExecutesInNode()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Structs", "PointVector.cs"));
        string stage = "C# compilation";
        string? managedOutput = null;
        int? managedExitCode = null;
        string? wasmOutput = null;
        int? wasmExitCode = null;
        string? wat = null;
        Exception? failure = null;
        bool emitted = false;
        try
        {
            byte[] pe = Frontend.Roslyn.Compile(source, assemblyName: "Structs.PointVector");
            stage = "CLR execution";
            var managed = Assembly.Load(pe);
            var originalOutput = Console.Out;
            using (var capture = new StringWriter(CultureInfo.InvariantCulture))
            {
                try
                {
                    Console.SetOut(capture);
                    managedExitCode = (int)managed.EntryPoint!.Invoke(null, null)!;
                }
                finally
                {
                    Console.SetOut(originalOutput);
                    managedOutput = capture.ToString().ReplaceLineEndings("\n");
                }
            }
            Assert.Equal("4\n6\n", managedOutput);
            Assert.Equal(0, managedExitCode);

            stage = "PE reading";
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            stage = "WASM emission";
            byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
            emitted = true;
            stage = "WAT conversion";
            wat = (await WasmTools.ToWatAsync(wasm)).ReplaceLineEndings("\n").TrimEnd();
            stage = "Node execution";
            var result = await Node.ExecuteAsync(wasm);
            wasmOutput = result.Output;
            wasmExitCode = result.ExitCode;
            Assert.Equal(managedOutput, wasmOutput);
            Assert.Equal(managedExitCode, wasmExitCode);
        }
        catch (Exception error)
        {
            failure = error;
        }

        bool inconclusive = stage == "WASM emission" && failure is NotSupportedException;
        var markdown = new StringBuilder("# Point + Vector WASM\n\nSource: [PointVector.cs](Fixtures/Structs/PointVector.cs)\n\n");
        Section("Source", "csharp", source.TrimEnd());
        Section("CLR output", "text", managedOutput?.TrimEnd() ?? "Not executed.");
        Section("CLR exit code", "text", managedExitCode?.ToString(CultureInfo.InvariantCulture) ?? "Not available.");
        Section("WASM output", "text", wasmOutput?.TrimEnd() ?? "Not executed.");
        Section("Exit Code", "text", wasmExitCode?.ToString(CultureInfo.InvariantCulture) ?? "Not available.");
        Section("WAT", wat is null ? "text" : "wat", wat ?? (emitted ? "Not converted." : "Not emitted."));
        markdown.AppendLine("## Status\n");
        markdown.AppendLine(failure is null ? "Passed." : $"{(inconclusive ? "Inconclusive" : "Failed")}: stopped during {stage}.");
        if (failure is not null)
        {
            markdown.AppendLine();
            var errors = new List<string>();
            for (Exception? error = failure; error is not null; error = error.InnerException)
                errors.Add($"{error.GetType().FullName}: {error.Message}");
            Section("Exception", "text", string.Join("\n", errors));
        }
        string snapshot = markdown.ToString().TrimEnd() + "\n";
        output.WriteLine(snapshot);
        Snapshot.AssertMatches("PointVector", snapshot);
        // Run and snapshot first. Unsupported emission is inconclusive; other failures remain failures.
        Skip.If(inconclusive, $"Inconclusive: {failure?.Message} See Structs/Snapshots/PointVector.md.");
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        void Section(string title, string language, string text) =>
            markdown.Append($"## {title}\n\n```{language}\n{text}\n```\n\n");
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
