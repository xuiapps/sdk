using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class BrowserInteractionTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task RealMobileShellRoutesTouchAndAnimatesNavigation()
    {
        var images = Frontend.MobileProjects.Images;
        string directory = Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Browser");
        byte[] driver = Frontend.Roslyn.Compile(File.ReadAllText(Path.Combine(directory, "Interactive.cs")),
            additionalReferences: images.Values.Select(image => MetadataReference.CreateFromImage(image)).Concat([
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll")),
                MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location)]),
            additionalSources: [File.ReadAllText(Path.Combine(directory, "CanvasContext.cs")), File.ReadAllText(Path.Combine(directory, "Platform.cs"))]);
        var assemblies = images.Values.Select(CanvasTests.Read).ToArray();
        using var app = CanvasTests.Read(driver);
        using var framework = new Frontend.Framework(typeof(IServiceProvider).Assembly.GetName().Name + ".dll");
        var emitter = new WasmEmitter();
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            byte[] wasm = emitter.Emit(app, new CanvasCommandRewriter(), namedExports: ExportTests.Methods(app,
                "Create", "Frame", "Pointer", "NeedsFrame", "ScreenX", "ScreenY", "ScreenScale", "Frames", "PointerEvents", "Animations", "TargetX", "TargetY", "HasView", "SetTime", "RequestFrame", "FramesPerSecond", "IsWarmingUp"));
            output.WriteLine($"WASM bytes: {wasm.Length}; methods: {emitter.Dependencies!.Methods.Count}");
            await ExportHost.RunAsync(wasm, BrowserCanvasHost.JavaScript + """
                const commands=[];
                const { exports: api } = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas:canvasImports(null,commands) });
                function warmup(hz) {
                    const s=api.Create();
                    api.RequestFrame(s,0n); api.Frame(s,900,950,0n);
                    assert.equal(api.IsWarmingUp(s),1);
                    for(let i=1;i<=hz*2+2;i++) { commands.length=0; api.Frame(s,900,950,BigInt(Math.round(i*10000000/hz))); }
                    assert.equal(api.IsWarmingUp(s),0);
                    assert.equal(api.FramesPerSecond(s),hz);
                    assert.ok(commands.some(c=>c[0]==='FillText' && c[5]===String(hz)), 'FPS badge must render the measured value');
                    assert.equal(api.NeedsFrame(s),0,'warmup must stop requesting frames');
                    // Unsolicited ticks and a long idle interval must not become slow-frame samples.
                    api.Frame(s,900,950,300000000n);
                    assert.equal(api.FramesPerSecond(s),hz);
                    return s;
                }
                warmup(30); warmup(120);
                const session=warmup(60);
                let ticks=300000000n;
                const frame=()=>{ ticks+=166667n; commands.length=0; api.Frame(session,900,950,ticks); };
                api.SetTime(session,ticks);

                const x=api.ScreenX(session)+api.TargetX(session,'CanvasDrawing')*api.ScreenScale(session);
                const y=api.ScreenY(session)+api.TargetY(session,'CanvasDrawing')*api.ScreenScale(session);
                assert.equal(api.Pointer(session,2,1,x,y,1),1);
                api.RequestFrame(session,ticks); frame();
                assert.equal(api.Pointer(session,4,1,x,y,0),1);
                for(let i=2;i<100;i++) frame();
                assert.equal(api.PointerEvents(session),2);
                assert.equal(api.HasView(session,'Back'),1,'tap must navigate to the example page');
                assert.ok(commands.some(c=>c[0]==='FillText' && c[5]==='Back'));
                const bx=api.ScreenX(session)+api.TargetX(session,'Back')*api.ScreenScale(session);
                const by=api.ScreenY(session)+api.TargetY(session,'Back')*api.ScreenScale(session);
                api.Pointer(session,2,2,bx,by,1); api.Pointer(session,4,2,bx,by,0);
                for(let i=100;i<200;i++) frame();
                assert.equal(api.HasView(session,'Back'),0,'back must remove the example page');
                assert.equal(api.HasView(session,'CanvasDrawing'),1);
                assert.ok(commands.some(c=>c[0]==='FillText' && c[5]==='Xui SDK Examples'));
                assert.equal(api.NeedsFrame(session),0,'animations must settle');
                """);
            string host = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Wasm/TestApp/MobileHost.js"));
            var errors = new List<Exception>();
            try { Snapshot.AssertFileMatches("BrowserInteractive", ".html", BrowserInteractivePage.Html(wasm, host)); }
            catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
            try { Snapshot.AssertMatches("BrowserInteractive", $"""
                # Interactive TestApp in the middleware phone frame

                [Open standalone page](BrowserInteractive.html). JavaScript, WASM and Inter fonts are embedded; no server or .NET browser runtime is required.

                The real Window, RootView, EventRouter, design root, mobile shell, buttons and NavigationView execute in WASM. Browser pointer events become TouchEventRef values; requestAnimationFrame supplies the platform clock and Window.OnAnimationFrame. The host explicitly calls RootView.Measure/Arrange/Render. The middleware supplies phone geometry/chrome and the fixture maps screen coordinates into the logical safe area.

                The browser catalogue exposes Canvas Drawing (the unchanged FillRectTest), with press feedback, animated navigation and Back. Optional diagnostics and the full async/GPU catalogue remain outside this preview. This is not the complete EmulatedWindow/runtime composition.

                Sources: [Interactive.cs](Fixtures/TestApp/Browser/Interactive.cs), [Platform.cs](Fixtures/TestApp/Browser/Platform.cs), [CanvasContext.cs](Fixtures/TestApp/Browser/CanvasContext.cs), [BrowserMenu.cs](Fixtures/TestApp/Mobile/BrowserMenu.cs), [PhoneHost.cs](Fixtures/TestApp/Mobile/PhoneHost.cs).

                ## Execution

                ```text
                Node: 30/60/120 FPS warmup and label updates, idle exclusion, persistent window, touch down/up, Canvas Drawing navigation, Back navigation and animation completion passed.
                WASM bytes: {wasm.Length}
                Retained managed methods: {emitter.Dependencies!.Methods.Count}
                ```

                ## Exit Code

                ```text
                0
                ```
                """ + "\n"); }
            catch (Xunit.Sdk.XunitException error) { errors.Add(error); }
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
