using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class MobileTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task PhoneHostRunsPersistentFramesAndPointerEvents()
    {
        string fixture = Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp");
        byte[] coreImage = Frontend.CoreCanvasProfile.Image;
        List<MetadataReference> references = [MetadataReference.CreateFromImage(coreImage)];
        byte[] emulatorImage = Frontend.SourceProject.Compile("Middleware/Emulator", "Xui.Middleware.Emulator", references,
            [File.ReadAllText(Path.Combine(fixture, "Mobile/PhoneHost.cs"))]);
        references.Add(MetadataReference.CreateFromImage(emulatorImage));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll")));
        byte[] viewImage = Frontend.Roslyn.Compile(File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Pages/Canvas/Tests/FillRectTest.cs")),
            OutputKind.DynamicallyLinkedLibrary, "TestApp.Canvas", additionalReferences: references,
            additionalSources: [File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Pages/Canvas/ICanvasDrawingExample.cs"))]);
        references.Add(MetadataReference.CreateFromImage(viewImage));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Collections.dll")));
        byte[] image = Frontend.Roslyn.Compile(File.ReadAllText(Path.Combine(fixture, "Mobile/Program.cs")), assemblyName: "TestApp.MobileHost",
            additionalReferences: references, additionalSources: [File.ReadAllText(Path.Combine(fixture, "CanvasContext.cs"))]);
        using var app = CanvasTests.Read(image); using var core = CanvasTests.Read(coreImage);
        using var emulator = CanvasTests.Read(emulatorImage); using var view = CanvasTests.Read(viewImage);
        using var framework = new Frontend.Framework();
        using var component = CanvasTests.Read(File.ReadAllBytes(typeof(IServiceProvider).Assembly.Location));
        using var collections = CanvasTests.Read(File.ReadAllBytes(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Collections.dll")));
        _ = new Xui.Compiler.XuiNet.Compilation([app, core, emulator, view, component, collections, .. framework.Assemblies]);
        byte[] wasm;
        try
        {
            wasm = new WasmEmitter().Emit(app, new CanvasCommandRewriter(), namedExports: ExportTests.Methods(app,
                "Create", "Frame", "Pointer", "NeedsFrame", "ScreenX", "ScreenY", "ScreenScale", "MarkerX", "MarkerY", "Frames", "PointerEvents", "Animations"));
        }
        catch (NotSupportedException error) { foreach (System.Collections.DictionaryEntry entry in error.Data) output.WriteLine($"{entry.Key}: {entry.Value}"); throw; }
        await ExportHost.RunAsync(wasm, ViewCanvasHost.JavaScript + """
            const commands = [];
            const { exports: api } = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasImports(null, commands) });
            const session = api.Create();
            api.Frame(session, 900, 900, 0n);
            assert.equal(api.Frames(session), 1);
            assert.equal(api.NeedsFrame(session), 0);
            const x = api.ScreenX(session), y = api.ScreenY(session), scale = api.ScreenScale(session);
            assert.equal(api.Pointer(session, 2, 7, 0, 0, 1), 0);
            assert.equal(api.Pointer(session, 2, 7, x + 200*scale, y + 400*scale, 1), 1);
            assert.ok(Math.abs(api.MarkerX(session) - 200) < .001);
            assert.ok(Math.abs(api.MarkerY(session) - 400) < .001);
            api.Pointer(session, 3, 7, x + 220*scale, y + 420*scale, 1);
            api.Frame(session, 900, 900, 160000n);
            api.Pointer(session, 4, 7, x + 220*scale, y + 420*scale, 0);
            assert.equal(api.NeedsFrame(session), 1);
            for (let i = 1; i <= 70; i++) api.Frame(session, 900, 900, BigInt((16 + i * 16) * 10000));
            assert.equal(api.NeedsFrame(session), 0);
            assert.equal(api.PointerEvents(session), 3);
            assert.ok(api.Animations(session) > 0);
            api.Frame(session, 390, 740, 20000000n);
            assert.ok(api.ScreenScale(session) < scale);
            """);
        string host = File.ReadAllText(Path.Combine(SourceDirectory(), "MobileHost.js"));
        string wat = await WasmTools.ToWatAsync(wasm);
        string markdown = $"# Persistent phone preview\n\n[Open standalone preview](MobilePhone.html)\n\nThe white browser surface hosts the emulator middleware's real PhoneFrameGeometry and PhoneChromeRenderer, with TestApp's unchanged FillRectTest inside. An explicit fixture subclass adds a draggable contact marker and one-second fade. **This is not the complete MobileTestAppShell or RootView/EventRouter.** See [full-shell readiness](MobileShellReadiness.md).\n\nSources: [Program.cs](Fixtures/TestApp/Mobile/Program.cs), [PhoneHost.cs](Fixtures/TestApp/Mobile/PhoneHost.cs), [CanvasContext.cs](Fixtures/TestApp/CanvasContext.cs).\n\n## Execution\n\n```text\nNode: persistent session, frames, pointer mapping, capture identity, fade completion, resize passed.\nBrowser: requestAnimationFrame, DPR-aware resize, pointer capture/cancel, idle scheduling and cleanup.\n```\n\nThe browser holds one opaque managed session, schedules frames only after invalidation or while View requests animation, and releases capture on up/cancel. Coordinates are logical CSS pixels; conversion into device coordinates runs in WASM. Source-profile immutable defaults avoid unsupported static initialization; normal emulator builds retain their cached defaults.\n\n## Exit Code\n\n```text\n0\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n";
        var errors = new List<Exception>();
        try { Snapshot.AssertMatches("MobilePhone", markdown); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
        try { Snapshot.AssertFileMatches("MobilePhone", ".html", MobilePage.Html(wasm, host)); } catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
        if (errors.Count != 0) throw new AggregateException(errors);
    }
    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
