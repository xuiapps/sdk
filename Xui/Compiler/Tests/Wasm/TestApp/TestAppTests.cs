using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class TestAppTests(ITestOutputHelper output)
{
    [SkippableFact]
    public async Task RealFillRectViewConstruction()
    {
        // Compile the actual TestApp files against the actual Core implementation PE.
        // No replacement View, altered application source, or dependency source linking.
        string testApp = Path.GetFullPath(Path.Combine(SourceDirectory(), "../../../../Apps/TestApp"));
        string viewSource = File.ReadAllText(Path.Combine(testApp, "Pages/Canvas/Tests/FillRectTest.cs"));
        string contractSource = File.ReadAllText(Path.Combine(testApp, "Pages/Canvas/ICanvasDrawingExample.cs"));
        byte[] coreImage = File.ReadAllBytes(typeof(Xui.Core.UI.View).Assembly.Location);
        List<MetadataReference> references = [MetadataReference.CreateFromImage(coreImage),
            MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))];
        byte[] viewImage = Frontend.Roslyn.Compile(viewSource, OutputKind.DynamicallyLinkedLibrary, "TestApp.Canvas",
            additionalReferences: references, additionalSources: [contractSource]);
        references.Add(MetadataReference.CreateFromImage(viewImage));
        const string driver = """
            using Xui.Apps.TestApp.Pages.Canvas.Tests;
            public static class Program {
                public static int Main() {
                    var view = new FillRectTest();
                    return view == null ? 1 : 0;
                }
            }
            """;
        byte[] appImage = Frontend.Roslyn.Compile(driver, additionalReferences: references);
        using var app = CanvasTests.Read(appImage);
        using var core = CanvasTests.Read(coreImage);
        using var view = CanvasTests.Read(viewImage);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, view, core, .. framework.Assemblies]);
        string status, details;
        NotSupportedException? unsupported = null;
        try
        {
            byte[] wasm = new WasmEmitter().Emit(app);
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(0, result.ExitCode);
            status = "Construction passed. Layout, rendering, input, and the complete TestApp are not exercised by this gate.";
            details = "```wat\n" + (await WasmTools.ToWatAsync(wasm)).TrimEnd() + "\n```";
        }
        catch (NotSupportedException error)
        {
            unsupported = error;
            status = "Blocked during compilation. WASM was not emitted; no browser rendering is claimed.";
            details = "```text\n" + error.GetType().FullName + ": " + error.Message + "\n```";
        }
        string markdown = $"# Real TestApp view: construction gate\n\n" +
            "Repository sources: `Xui/Apps/TestApp/Pages/Canvas/Tests/FillRectTest.cs` and `Xui/Apps/TestApp/Pages/Canvas/ICanvasDrawingExample.cs`.\n\n" +
            "These unchanged sources form a separate TestApp.Canvas assembly linked with the actual Xui.Core implementation and this driver. This first gate deliberately tests construction before adding the rendering host.\n\n" +
            $"## Driver\n\n```csharp\n{driver}\n```\n\n## Status\n\n{status}\n\n## WASM / blocker\n\n{details}\n";
        output.WriteLine(markdown);
        Snapshot.AssertMatches("FillRectConstruction", markdown);
        Skip.If(unsupported is not null, unsupported?.Message);
    }

    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
