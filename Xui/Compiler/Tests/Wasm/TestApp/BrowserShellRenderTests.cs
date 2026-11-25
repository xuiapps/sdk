using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class BrowserShellRenderTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task RealMobileShellMeasuresArrangesAndRenders()
    {
        var images = Frontend.MobileProjects.Images;
        string directory = Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Browser");
        string source = File.ReadAllText(Path.Combine(directory, "Program.cs"));
        byte[] driver = Frontend.Roslyn.Compile(source, additionalReferences: images.Values.Select(image => MetadataReference.CreateFromImage(image))
            .Concat(new[] { MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll")), MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location) }),
            additionalSources: [File.ReadAllText(Path.Combine(directory, "CanvasContext.cs")), File.ReadAllText(Path.Combine(directory, "Platform.cs"))]);
        var assemblies = images.Values.Select(CanvasTests.Read).ToArray();
        using var app = CanvasTests.Read(driver);
        using var framework = new Frontend.Framework(typeof(IServiceProvider).Assembly.GetName().Name + ".dll");
        var emitter = new WasmEmitter();
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            byte[] wasm = emitter.Emit(app, new CanvasCommandRewriter());
            output.WriteLine($"WASM bytes: {wasm.Length}; methods: {emitter.Dependencies!.Methods.Count}");
            var result = await CanvasHost.Record(wasm, BrowserCanvasHost.JavaScript);
            Assert.Equal(0, result.ExitCode);
            using var json = JsonDocument.Parse(result.Commands);
            var commands = json.RootElement.EnumerateArray().ToArray();
            foreach (string label in new[] { "Xui SDK Examples", "Canvas", "Canvas Drawing", "fps" })
                Assert.Contains(commands, command => command[0].GetString() == "FillText" && command[5].GetString() == label);
            Assert.Equal(commands.Count(c => c[0].GetString() == "Save"), commands.Count(c => c[0].GetString() == "Restore"));
            Assert.DoesNotContain(emitter.Dependencies.Methods, method => method.ToString() == "Xui.Apps.TestApp.MobilePerformanceOverlay.RenderCore");
            var errors = new List<Exception>();
            try { Snapshot.AssertMatches("BrowserShellRender", $"# TestApp mobile shell: first frame\n\n[Open standalone page](BrowserShell.html). Real Window/RootView activation and TestAppDesignRoot supply the mobile theme. The browser catalogue contains Canvas Drawing; the optional diagnostics overlay is omitted. Layout and drawing execute in WASM; Canvas text metrics and drawing cross explicit host imports. This first-frame test does not dispatch input.\n\nSources: [Program.cs](Fixtures/TestApp/Browser/Program.cs), [CanvasContext.cs](Fixtures/TestApp/Browser/CanvasContext.cs), [Platform.cs](Fixtures/TestApp/Browser/Platform.cs), [BrowserMenu.cs](Fixtures/TestApp/Mobile/BrowserMenu.cs).\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Drawing commands\n\n```json\n{result.Commands}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n"); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
            try { Snapshot.AssertFileMatches("BrowserShell", ".html", BrowserShellPage.Html(wasm)); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
            if (errors.Count != 0) throw new AggregateException(errors);
        }
        catch (Exception failure)
        {
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(failure.Message, @"wasm-function\[(\d+)\]"))
                if (emitter.Methods.TryGetValue(new(uint.Parse(match.Groups[1].Value)), out var method)) output.WriteLine($"{match.Value}: {method}");
            foreach (System.Collections.DictionaryEntry entry in failure.Data) output.WriteLine($"{entry.Key}: {entry.Value}");
            throw;
        }
        finally { foreach (var assembly in assemblies) assembly.Dispose(); }
    }
}
