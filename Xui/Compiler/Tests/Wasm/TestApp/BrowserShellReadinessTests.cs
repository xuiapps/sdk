using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.Tests.Wasm.Repl;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class BrowserShellReadinessTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task BrowserCatalogueConstructsTheRealMobileShell()
    {
        var images = Frontend.MobileProjects.Images;
        byte[] driver = Frontend.Roslyn.Compile("""
            using Xui.Apps.TestApp;
            public static class Program {
                public static int Main() {
                    var metrics = new MobilePerformanceMetrics(LayoutFrameInstruments.CreateCountersOnly());
                    var shell = new MobileTestAppShell(metrics, new BrowserMenu());
                    return shell.Count;
                }
            }
            """, additionalReferences: images.Values.Select(image => MetadataReference.CreateFromImage(image)));
        var assemblies = images.Values.Select(CanvasTests.Read).ToArray();
        using var app = CanvasTests.Read(driver);
        using var framework = new Frontend.Framework(typeof(IServiceProvider).Assembly.GetName().Name + ".dll");
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            var emitter = new WasmEmitter();
            byte[] wasm;
            try { wasm = emitter.Emit(app); }
            catch (Exception failure) {
                foreach (System.Collections.DictionaryEntry entry in failure.Data) output.WriteLine($"{entry.Key}: {entry.Value}");
                throw;
            }
            output.WriteLine($"WASM bytes: {wasm.Length}; retained methods: {emitter.Dependencies!.Methods.Count}");
            var result = await Node.ExecuteAsync(wasm);
            Assert.Equal(1, result.ExitCode);
            Assert.Empty(result.Output);

            string frameworkInputs = string.Join(", ", framework.Assemblies.Select(assembly => $"`{assembly.Identity.Name}`"));
            string projects = string.Join("\n", images.Keys.Select(name => $"- `{name}`"));
            Snapshot.AssertMatches("BrowserShellReadiness", $"# Browser TestApp catalogue: construction\n\nStatus: **Real mobile shell constructs in WASM and returns successfully in Node.**\n\n## Inputs\n\nComplete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded. Core/emulator use the documented Canvas compiler profile.\n\n{projects}\n\nFramework implementation PEs: {frameworkInputs}.\n\n## Entry\n\n```csharp\nvar metrics = new MobilePerformanceMetrics(LayoutFrameInstruments.CreateCountersOnly());\nvar shell = new MobileTestAppShell(metrics, new BrowserMenu());\nreturn shell.Count;\n```\n\n## Output\n\nNo console output.\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\nThe shell reports one navigation child. This executes the real home/header, rating controls, Canvas-only catalogue, and navigation constructors, including static initializers and BCL collection code. Optional async/GPU/resource showcases remain in the separate full-menu probe.\n\nThis is a construction gate, not yet a rendered or interactive full-shell browser test. The next gate must supply Canvas measurement/drawing, arrange and render the shell, then attach the real window/event-routing lifecycle.\n\n## WASM\n\nEmitted, validated and executed by Node with the shared JavaScript runtime. Full application-sized WAT is omitted here; focused instruction/feature snapshots retain their WAT.\n");
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
