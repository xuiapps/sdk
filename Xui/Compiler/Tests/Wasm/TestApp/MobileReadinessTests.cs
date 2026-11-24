using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class MobileReadinessTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void FullMobileShellReportsRemainingCompilerGap()
    {
        var images = Frontend.MobileProjects.Images;
        byte[] driver = Frontend.Roslyn.Compile("""
            using Xui.Apps.TestApp;
            public static class Program {
                public static int Main() {
                    var metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());
                    var shell = new MobileTestAppShell(metrics);
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

            string path = error.Data["Xui.DependencyPath"]?.ToString() ?? "";
            output.WriteLine(error.Message + "\n" + path);
            Assert.Contains("Default equality is not implemented for Xui.Core.UI.View", error.Message);
            string frameworkInputs = string.Join(", ", framework.Assemblies.Select(assembly => $"`{assembly.Identity.Name}`"));
            string projects = string.Join("\n", images.Keys.Select(name => $"- `{name}`"));
            Snapshot.AssertMatches("MobileShellReadiness", $"# Full TestApp mobile shell: compiler readiness\n\nStatus: **WASM not emitted.** This is an expected-unsupported regression, not an execution test.\n\n## Inputs\n\nThe complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded so the fixture can select the mobile shell directly. Core/emulator use the documented Canvas compiler profile.\n\n{projects}\n\nFramework implementation PEs: {frameworkInputs}.\n\n## Entry\n\n```csharp\nvar metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());\nvar shell = new MobileTestAppShell(metrics);\nreturn shell.Count;\n```\n\n## Compiler failure\n\n```text\n{error.Message}\n```\n\n## Dependency path\n\n```text\n{path}\n```\n\nGenerated event subscriptions and TimeSpan factories now pass dependency discovery. Duration conversion emits WASM tick arithmetic with managed NaN/range failures and English messages, without retaining System.SR or ResourceManager for those guards. The rating panel's string interpolation now uses mutable UTF-16 GC buffers and integer formatting without retaining the BCL handler body. Closed generic interface methods now specialize their implementations and use distinct dispatch slots, including the demo navigation call. Default string/scalar equality now specializes without BCL factories. The current full-catalogue blocker is default View equality reached by the diagnostic HashSet<View>. Optional floorplan preloading also retains known unsupported Lazy/async dependencies. Static initialization, clock/services, collections, text measurement, and full event routing still require further probes.\n\n## WAT\n\nNot emitted.\n");
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
