using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Exports;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class RealCatalogueTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Theory]
    [InlineData("Xui.Apps.TestApp.Pages.Layout.BasicsExample", 3)]
    [InlineData("Xui.Apps.TestApp.Pages.Grid.GridExample", 12)]
    [InlineData("Xui.Apps.TestApp.Pages.Layout.FlexboxExample", 3)]
    [InlineData("Xui.Apps.TestApp.Pages.Canvas.CanvasTestsExample", 22)]
    [InlineData("Xui.Apps.TestApp.Examples.CanvasAnimationsExample", 3)]
    [InlineData("Xui.Apps.TestApp.Pages.Gestures.GesturesExample", 6)]
    [InlineData("Xui.Apps.TestApp.Examples.TextLayoutExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.TextMetricsExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.ViewCollectionAlignmentExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.DeviceInfoExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.HapticsExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.LiveVibrationExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.RecordedVibrationExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.TextBoxExample", 1)]
    [InlineData("Xui.Apps.TestApp.Examples.DesignSystem.DesignSystemExample", 1)]
    [InlineData("Xui.Apps.TestApp.Pages.Market.MarketExample", 1)]
    [InlineData("HardwareCubeExample", 1)]
    public async Task ProductionPageRendersEverySection(string type, int expectedSections)
    {
        var images = Frontend.MobileProjects.Images;
        string directory = Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Browser");
        string source = File.ReadAllText(Path.Combine(directory, "Interactive.cs"))
            .Replace("BrowserMenu.CreateRoot(Metrics)", $"CataloguePage.Create<{type}>(Metrics)")
            .Replace("session.Phone.Begin(", "session.Phone.BeginComposed(")
            .Replace("session.Phone.End(", "session.Phone.EndComposed(")
            .Replace("public static class Program", "public static partial class Program");
        source += """
            public static partial class Program {
                public static string FrameChecked(BrowserSession s, double w, double h, long t) {
                    try { Frame(s,w,h,t); return null; } catch(System.Exception error) { return error.Message; }
                }
                public static int Sections(BrowserSession s) => CataloguePage.Sections(s.Window.RootView);
                public static void SelectSection(BrowserSession s, int index) => CataloguePage.Select(s.Window.RootView,index);
            }
            """;
        bool gpu = type == "HardwareCubeExample";
        if (gpu)
        {
            source = source.Replace("Platform = new BrowserPlatform(Context);", "Platform = new BrowserPlatform(Context); var gpu = new BrowserGpuServices(); Platform.AdditionalServices = gpu; Context.GpuDevice = gpu.Device;")
                .Replace("session.Phone.BeginComposed(", "session.Context.BeginGpuFrame(); session.Phone.BeginComposed(")
                .Replace("session.Frames++;", "session.Context.EndGpuFrame(); session.Frames++;");
            source += """
                public static partial class Program { public static void DropContent(BrowserSession session) => session.Window.RootView.Content=null; }
                public sealed class HardwareCubeExample : global::Example {
                    public HardwareCubeExample() { Title="3D Hardware Rendering"; Content=new Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest.GpuHardwareCubeTestView { Id="ThreeDSurface" }; }
                }
                """;
        }
        bool textInput = type.EndsWith("TextBoxExample", StringComparison.Ordinal);
        byte[] pe = Frontend.Roslyn.Compile(source, allowUnsafe: gpu,
            additionalReferences: images.Values.Select(image => MetadataReference.CreateFromImage(image)).Concat([
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll")),
                MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location)]),
            additionalSources: [File.ReadAllText(Path.Combine(directory, "CanvasContext.cs")), File.ReadAllText(Path.Combine(directory, "Platform.cs")), .. (gpu ? new[] { File.ReadAllText(Path.Combine(directory, "Gpu.cs")), File.ReadAllText(Path.Combine(directory, "GpuContext.cs")) } : []), .. (textInput ? new[] { File.ReadAllText(Path.Combine(directory, "TextInput.cs")) } : [])]);
        var assemblies = images.Values.Select(CanvasTests.Read).ToArray();
        using var app = CanvasTests.Read(pe);
        using var framework = new Frontend.Framework(typeof(IServiceProvider).Assembly.GetName().Name + ".dll", "System.Linq.dll", "System.Collections.dll");
        var emitter = new WasmEmitter();
        try
        {
            _ = new Xui.Compiler.XuiNet.Compilation([app, .. assemblies, .. framework.Assemblies]);
            string[] textExports = textInput ? ["TextClient", "TextValue", "TextStart", "TextEnd", "TextSecure", "TextSelect", "TextInsert", "TextKey", "TextError"] : [];
            var wasm = emitter.Emit(app, new CanvasCommandRewriter(), namedExports: ExportTests.Methods(app, [.. textExports, .. (gpu ? new[] {"DropContent"} : []),
                "Create", "Frame", "FrameChecked", "Pointer", "NeedsFrame", "ScreenX", "ScreenY", "ScreenScale", "TargetX", "TargetY", "HasView", "Sections", "SelectSection", "SetTime", "RequestFrame", "Frames", "PointerEvents", "Animations", "FramesPerSecond", "IsWarmingUp"]), exportMemory: gpu);
            string artifactDirectory = Path.Combine(AppContext.BaseDirectory, "TestApp");
            Directory.CreateDirectory(artifactDirectory);
            File.WriteAllLines(Path.Combine(artifactDirectory,type.Split('.').Last()+".methods.txt"), emitter.Methods.Select(pair => $"{pair.Key.Value}: {pair.Value}"));
            string artifact = Path.Combine(artifactDirectory, type.Split('.').Last() + ".html");
            string host = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Wasm/TestApp/MobileHost.js"));
            if (gpu)
            {
                host = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Wasm/Gpu/BrowserGpuHost.js")) + "\n" + host;
                host = host.Replace("const { exports: api } = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasHost });",
                    "let api; const gpuHost=createGpuHost(()=>api.memory); globalThis.xuiGpuHost=gpuHost; ({ exports: api } = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasHost, gpu:gpuHost.imports }));")
                    .Replace("canvasHost.dispose?.();", "canvasHost.dispose?.(); gpuHost.dispose();");
            }
            if (textInput) host = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Wasm/TestApp/BrowserTextInputHost.js")) + "\n" + host;
            bool imagesPage = type.EndsWith("CanvasTestsExample", StringComparison.Ordinal);
            string[] assets = imagesPage ? ["test-512.png"] : type.EndsWith("DesignSystemExample", StringComparison.Ordinal) ? ["ColorWheel.png", "ColorTriangle.png"] : [];
            File.WriteAllText(artifact, BrowserInteractivePage.Html(wasm, host, assets.Length == 0 ? null : BrowserAssets.Script(true, assets), composition: true));
            output.WriteLine("Browser artifact: " + artifact);
            await ExportHost.RunAsync(wasm, BrowserAssets.Script(false, assets) + "\n" + BrowserCanvasHost.JavaScript + (gpu ? "\n" + File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Wasm/Gpu/BrowserGpuHost.js")) : "") + "\nconst expectedSections = " + expectedSections + ";\n" + """
                const commands=[];
                let api;
                const gpuHost=typeof createGpuHost==='function'?createGpuHost(()=>api.memory,{recordOnly:true}):null;
                globalThis.xuiGpuHost=gpuHost;
                ({exports:api}=await WebAssembly.instantiate(module,{...globalThis.xuiRuntimeImports,canvas:canvasImports(null,commands),gpu:gpuHost?.imports??{}}));
                const session=api.Create(); let ticks=0n;
                const frame=()=>{commands.length=0;assert.equal(api.FrameChecked(session,900,950,ticks),null);ticks+=166667n;};
                for(let i=0;i<150;i++)frame();
                assert.equal(api.Sections(session),expectedSections);
                for(let section=0;section<expectedSections;section++) {
                    api.SelectSection(session,section);
                    for(let i=0;i<60;i++)frame();
                    assert.ok(commands.length>50,`Section ${section} must draw`);
                    assert.equal(commands.filter(c=>c[0]==='Save').length,commands.filter(c=>c[0]==='Restore').length);
                    if(expectedSections===22 && section===20) assert.ok(commands.some(c=>c[0]==='DrawImage'));
                    if(expectedSections===22 && section===21) assert.ok(commands.some(c=>c[0]==='FillPattern'));
                }
                if(gpuHost){assert.ok(gpuHost.stats.draws>0);assert.ok(commands.some(c=>c[0]==='GpuDraw'));gpuHost.dispose();assert.equal(gpuHost.stats.resources,0);}
                if(api.TextInsert) {
                    const focus=id=>{
                        const scale=api.ScreenScale(session),x=api.ScreenX(session)+api.TargetX(session,id)*scale,y=api.ScreenY(session)+api.TargetY(session,id)*scale;
                        api.Pointer(session,2,1,x,y,1);api.Pointer(session,4,1,x,y,0);frame();
                    };
                    focus('NameBox'); api.TextInsert(session,'a\u0301🙂');
                    assert.equal(api.TextValue(session),'a\u0301🙂');
                    api.TextKey(session,8,0,0);assert.equal(api.TextValue(session),'a\u0301');
                    api.TextKey(session,8,0,0);assert.equal(api.TextValue(session),'');
                    api.TextKey(session,9,0,0);assert.equal(api.TextSecure(session),1);
                    api.TextInsert(session,'secret');assert.equal(api.TextValue(session),'secret');frame();
                    focus('NumberBox');api.TextSelect(session,0,api.TextValue(session).length);
                    api.TextInsert(session,'12.5');api.TextInsert(session,'x');assert.equal(api.TextValue(session),'12.5');frame();
                }
                """);
            output.WriteLine($"{type}: {expectedSections} sections; {wasm.Length} WASM bytes; {emitter.Dependencies!.Methods.Count} methods.");
        }
        catch(Exception failure)
        {
            foreach(System.Collections.DictionaryEntry entry in failure.Data) output.WriteLine($"{entry.Key}: {entry.Value}");
            foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(failure.Message, @"wasm-function\[(\d+)\]"))
                if(emitter.Methods.TryGetValue(new(uint.Parse(match.Groups[1].Value)),out var method)) output.WriteLine($"{match.Value}: {method}");
            throw;
        }
        finally { foreach(var assembly in assemblies) assembly.Dispose(); }
    }
}
