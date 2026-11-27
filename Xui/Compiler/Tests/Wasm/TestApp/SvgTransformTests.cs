using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

[Collection(nameof(ManagedConsoleCollection))]
public class SvgTransformTests
{
    [Fact]
    public async Task TransformReaderMatchesExistingRegexAndEveryFloorplanTransform()
    {
        string asset = Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Assets/Floorplan/Office.svg");
        string[] transforms = XDocument.Load(asset).Descendants().Attributes("transform").Select(a => a.Value).Distinct().ToArray();
        Assert.NotEmpty(transforms);
        string[] cases = ["", "translate(1,2)", "scale(-.5)rotate(90 4 8)", "MATRIX(1,0,0,1,4,6)",
            "translate () scale() rotate( )", "1translate(2) scale (3)", "???étranslate(2)\0rotate(3)",
            "a(b(c)d)e(4)", "a(()b(3)", "a()b(1)", "a( b(1)", "a(x", "a(\n1\n)", "translate(1e-2 -3.5)", .. transforms];
        string source = """
            using System;
            using Xui.Apps.TestApp.Pages.Floorplan;
            public static class Program { public static int Main() {
            """ + "foreach(var value in new string[] {" + string.Join(",", cases.Select(value => JsonSerializer.Serialize(value))) + "}) {" + """
                var reader=new SvgTransformReader(value);
                while(reader.MoveNext()) { Console.WriteLine(reader.Name); Console.WriteLine(reader.Arguments.ToString()); }
            } return 42; } }
            """;
        string helper = "using System;\n" + File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Pages/Floorplan/SvgTransformReader.cs"));
        byte[] image = Frontend.Roslyn.Compile(source, additionalSources: [helper]);
        string expected = string.Concat(cases.SelectMany(value => Regex.Matches(value, @"([a-zA-Z]+)\(([^)]+)\)")
            .Select(match => match.Groups[1].Value + "\n" + match.Groups[2].Value + "\n")));
        Assert.Equal(expected, GcTests.RunManaged(image).Output);
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal(expected, result.Output);
        Assert.Equal(42, result.ExitCode);
    }
}
