using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

public class BrowserCanvasTests
{
    [Fact]
    public async Task TextMetricsRespectSavedFontStateAndGroupsBalance()
    {
        string context = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Browser/CanvasContext.cs"));
        byte[] image = Frontend.Roslyn.Compile("""
            using TestAppCanvas;
            using Xui.Core.Canvas;
            using Xui.Core.Canvas.Composition.Graphics;
            public static class Program { public static int Main() {
                var context = new CanvasContext();
                context.SetFont(new Font(20, "Inter"));
                double first=(double)context.MeasureText("ABCD").Line.Width;
                context.Save(); context.SetFont(new Font(40, "Inter"));
                double larger=(double)context.MeasureText("ABCD").Line.Width;
                context.Restore();
                double restored=(double)context.MeasureText("ABCD").Line.Width;
                using (var group=context.Group(new GroupOptions { Key=new object(), Bounds=(0,0,100,100), ClipToBounds=true, Background=new Color(0xffffffff) })) {
                    context.BeginPath(); context.RoundRect((1,1,98,98),new CornerRadius(2,3,4,5)); context.Stroke();
                    context.FillText("ABCD",(5,25));
                }
                return first==44 && larger==88 && restored==44 ? 42 : 1;
            } }
            """, additionalReferences: [MetadataReference.CreateFromImage(Frontend.CoreCanvasProfile.Image),
                MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.InteropServices.dll"))], additionalSources: [context]);
        using var app = CanvasTests.Read(image);
        using var core = CanvasTests.Read(Frontend.CoreCanvasProfile.Image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, core, .. framework.Assemblies]);
        var result = await CanvasHost.Record(new WasmEmitter().Emit(app, new CanvasCommandRewriter()), BrowserCanvasHost.JavaScript);
        Assert.Equal(42, result.ExitCode);
        using var json = JsonDocument.Parse(result.Commands);
        var commands = json.RootElement.EnumerateArray().ToArray();
        Assert.Equal(commands.Count(command => command[0].GetString() == "Save"), commands.Count(command => command[0].GetString() == "Restore"));
        Assert.Contains(commands, command => command[0].GetString() == "FillText" && command[5].GetString() == "ABCD");
        Assert.Contains(commands, command => command[0].GetString() == "RoundRectCorners");
    }
}
