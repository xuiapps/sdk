using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class RenderTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task RealViewLayoutAndRender(OptimizationLevel optimization)
    {
        string directory = SourceDirectory();
        string fixture = Path.GetFullPath(Path.Combine(directory, "../../Fixtures/TestApp"));
        string testApp = Path.GetFullPath(Path.Combine(directory, "../../../../Apps/TestApp"));
        byte[] coreImage = Frontend.CoreCanvasProfile.Image;
        List<MetadataReference> references = [MetadataReference.CreateFromImage(coreImage),
            MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll")),
            MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Collections.dll"))];
        byte[] viewImage = Frontend.Roslyn.Compile(File.ReadAllText(Path.Combine(testApp, "Pages/Canvas/Tests/FillRectTest.cs")),
            OutputKind.DynamicallyLinkedLibrary, "TestApp.Canvas", optimizationLevel: optimization, additionalReferences: references,
            additionalSources: [File.ReadAllText(Path.Combine(testApp, "Pages/Canvas/ICanvasDrawingExample.cs"))]);
        references.Add(MetadataReference.CreateFromImage(viewImage));
        byte[] image = Frontend.Roslyn.Compile(File.ReadAllText(Path.Combine(fixture, "Render.cs")), assemblyName: "TestApp.Render", optimizationLevel: optimization,
            additionalReferences: references, additionalSources: [File.ReadAllText(Path.Combine(fixture, "CanvasContext.cs"))]);
        using var app = CanvasTests.Read(image); using var view = CanvasTests.Read(viewImage); using var core = CanvasTests.Read(coreImage);
        using var framework = new Frontend.Framework();
        using var component = CanvasTests.Read(File.ReadAllBytes(typeof(IServiceProvider).Assembly.Location));
        using var collections = CanvasTests.Read(File.ReadAllBytes(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Collections.dll")));
        _ = new Xui.Compiler.XuiNet.Compilation([app, view, core, component, collections, .. framework.Assemblies]);
        try
        {
            var emitter = new WasmEmitter();
            byte[] wasm = emitter.Emit(app, new CanvasCommandRewriter());
            var result = await CanvasHost.Record(wasm, ViewCanvasHost.JavaScript);
            Assert.Equal(0, result.ExitCode);
            var managed = RunManaged(coreImage, viewImage, image);
            Assert.Equal(managed.ExitCode, result.ExitCode);
            using var actual = JsonDocument.Parse(result.Commands);
            using var expected = JsonDocument.Parse(managed.Commands);
            AssertCommands(expected.RootElement, actual.RootElement);
            Assert.Equal(5, actual.RootElement.EnumerateArray().Count(command => command[0].GetString() == "FillRect"));
            Assert.Equal("Save", actual.RootElement[0][0].GetString());
            Assert.Equal("Restore", actual.RootElement[actual.RootElement.GetArrayLength() - 1][0].GetString());
            Assert.Contains(emitter.Dependencies!.Methods, m => m.ToString() == "Xui.Core.UI.View.MeasureShell");
            Assert.Contains(emitter.Dependencies.Methods, m => m.ToString() == "Xui.Core.UI.View.ArrangeShell");
            Assert.Contains(emitter.Dependencies.Methods, m => m.ToString() == "Xui.Core.UI.View.RenderShell");
            if (optimization == OptimizationLevel.Debug) return;
            string driver = File.ReadAllText(Path.Combine(fixture, "Render.cs"));
            string drawing = File.ReadAllText(Path.Combine(testApp, "Pages/Canvas/Tests/FillRectTest.cs"));
            string wat = await WasmTools.ToWatAsync(wasm);
            string retained = string.Join("\n", emitter.Dependencies.Methods.Select(m => $"- `{m.DeclaringType.Module.Name}`: `{m}`"));
            string markdown = $"# TestApp: real View layout and rendering\n\n[Open standalone demo](FillRectRender.html)\n\nUses the explicit `XUI_COMPILER_CANVAS` Core profile: instrumentation calls are omitted, and default font weight/stretch are constructed directly. See [profile details](../README.md).\n\nSource: [Render.cs](Fixtures/TestApp/Render.cs), [CanvasContext.cs](Fixtures/TestApp/CanvasContext.cs). The test reads the unchanged TestApp `Pages/Canvas/Tests/FillRectTest.cs` from the repository.\n\n## Driver\n\n```csharp\n{driver.TrimEnd()}\n```\n\n## TestApp drawing code\n\n```csharp\n{drawing.TrimEnd()}\n```\n\n## Drawing commands\n\n```json\n{result.Commands}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## Retained methods\n\n{retained}\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n";
            string html = CanvasHost.Html("TestApp / FillRect", driver + "\n" + drawing, wasm, ViewCanvasHost.JavaScript);
            // Evaluate both snapshots so a new artifact produces both reviewable files.
            var errors = new List<Exception>();
            try { Snapshot.AssertMatches("FillRectRender", markdown); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
            try { Snapshot.AssertFileMatches("FillRectRender", ".html", html); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
            if (errors.Count != 0) throw new AggregateException(errors);
        }
        catch (NotSupportedException error) { foreach (System.Collections.DictionaryEntry item in error.Data) output.WriteLine($"{item.Key}: {item.Value}"); throw; }
    }

    private static (string Commands, int ExitCode) RunManaged(byte[] core, byte[] view, byte[] app)
    {
        var context = new AssemblyLoadContext("Canvas profile", isCollectible: true);
        try
        {
            context.LoadFromStream(new MemoryStream(core));
            context.LoadFromStream(new MemoryStream(view));
            var assembly = context.LoadFromStream(new MemoryStream(app));
            int result = (int)assembly.EntryPoint!.Invoke(null, null)!;
            var commands = assembly.GetType("TestAppCanvas.NativeCanvas")!.GetField("Commands")!.GetValue(null);
            return (JsonSerializer.Serialize(commands), result);
        }
        finally { context.Unload(); }
    }

    private static void AssertCommands(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.GetArrayLength(), actual.GetArrayLength());
        for (int i = 0; i < expected.GetArrayLength(); i++)
        {
            Assert.Equal(expected[i].GetArrayLength(), actual[i].GetArrayLength());
            for (int j = 0; j < expected[i].GetArrayLength(); j++)
            {
                if (expected[i][j].ValueKind == JsonValueKind.Number)
                {
                    double value = expected[i][j].GetDouble();
                    // CLR host NFloat is 64-bit; this compiler deliberately targets wasm32.
                    Assert.InRange(Math.Abs(value - actual[i][j].GetDouble()), 0, Math.Max(1, Math.Abs(value)) * 1e-6);
                }
                else Assert.Equal(expected[i][j].GetString(), actual[i][j].GetString());
            }
        }
    }

    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
