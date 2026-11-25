using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class BrowserShellReadinessTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void BrowserCatalogueReportsRemainingCompilerGap()
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
            var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app));

            string path = error.Data["Xui.DependencyPath"]?.ToString() ?? "Reachability completed; failure occurred during method planning/emission.";
            string failure = error.Message;
            if (error.Data["Xui.Method"] is { } method) failure += $"\nMethod: {method}";
            if (error.Data["Xui.Instruction"] is { } instruction) failure += $"\nInstruction: {instruction}";
            output.WriteLine(failure + "\n" + path);

            string frameworkInputs = string.Join(", ", framework.Assemblies.Select(assembly => $"`{assembly.Identity.Name}`"));
            string projects = string.Join("\n", images.Keys.Select(name => $"- `{name}`"));
            Snapshot.AssertMatches("BrowserShellReadiness", $"# Browser TestApp catalogue: compiler readiness\n\nStatus: **WASM not emitted.** This is an expected-unsupported regression, not an execution test.\n\n## Inputs\n\nThe complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded so the fixture can select the mobile shell directly. Core/emulator use the documented Canvas compiler profile.\n\n{projects}\n\nFramework implementation PEs: {frameworkInputs}.\n\n## Entry\n\n```csharp\nvar metrics = new MobilePerformanceMetrics(LayoutFrameInstruments.CreateCountersOnly());\nvar shell = new MobileTestAppShell(metrics, new BrowserMenu());\nreturn shell.Count;\n```\n\n## Compiler failure\n\n```text\n{failure}\n```\n\n## Dependency path\n\n```text\n{path}\n```\n\nThis probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.\n\n## WAT\n\nNot emitted.\n");
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
