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
        using var framework = new Frontend.Framework(typeof(IServiceProvider).Assembly.GetName().Name + ".dll");
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            var error = Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app));

            string path = error.Data["Xui.DependencyPath"]?.ToString() ?? "";
            Assert.Contains("System.Resources.MissingManifestResourceException", error.Message);
            Assert.Contains("System.TimeSpan.FromSeconds", path);
            Assert.Contains("System.ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN", path);
            string frameworkInputs = string.Join(", ", framework.Assemblies.Select(assembly => $"`{assembly.Identity.Name}`"));
            string projects = string.Join("\n", images.Keys.Select(name => $"- `{name}`"));
            Snapshot.AssertMatches("MobileShellReadiness", $"# Full TestApp mobile shell: compiler readiness\n\nStatus: **WASM not emitted.** This is an expected-unsupported regression, not an execution test.\n\n## Inputs\n\nThe complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded so the fixture can select the mobile shell directly. Core/emulator use the documented Canvas compiler profile.\n\n{projects}\n\nFramework implementation PEs: {frameworkInputs}.\n\n## Entry\n\n```csharp\nvar metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());\nvar shell = new MobileTestAppShell(metrics);\nreturn shell.Count;\n```\n\n## Compiler failure\n\n```text\n{error.Message}\n```\n\n## Dependency path\n\n```text\n{path}\n```\n\nGenerated event subscriptions now execute using reference CompareExchange in the single-agent runtime. The probe also supplies IServiceProvider's implementation assembly. Dependency discovery next reaches MobileFrameRate's TimeSpan.FromSeconds call: its NaN guard pulls in System.SR resource lookup and an unsupported MissingManifestResourceException catch. This is dependency discovery, not a runtime NaN failure. Static initialization, timing/services, collections, text measurement, and full event routing still require further probes.\n\n## WAT\n\nNot emitted.\n");
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
