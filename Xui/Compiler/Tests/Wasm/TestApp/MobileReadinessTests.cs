using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class MobileReadinessTests
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
        using var framework = new Frontend.Framework();
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app));
            Assert.Contains("System.Action", error.Message);
            string path = error.Data["Xui.DependencyPath"]?.ToString() ?? "";
            Assert.Contains("System.Action", path);
            string projects = string.Join("\n", images.Keys.Select(name => $"- `{name}`"));
            Snapshot.AssertMatches("MobileShellReadiness", $"# Full TestApp mobile shell: compiler readiness\n\nStatus: **WASM not emitted.** This is an expected-unsupported regression, not an execution test.\n\n## Inputs\n\nThe complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded so the fixture can select the mobile shell directly. Core/emulator use the documented Canvas compiler profile.\n\n{projects}\n\n## Entry\n\n```csharp\nvar metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());\nvar shell = new MobileTestAppShell(metrics);\nreturn shell.Count;\n```\n\n## Compiler failure\n\n```text\n{error.Message}\n```\n\n## Dependency path\n\n```text\n{path}\n```\n\nClosed generic method specialization and owner/selector byrefs are supported, including the SetProtectedChild pattern. Dependency discovery now reaches a runtime-provided delegate constructor: its implementation is not IL and needs compiler/runtime delegate support. Static initialization, collections, services, text measurement, and full event routing still require further probes.\n\n## WAT\n\nNot emitted.\n");
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
