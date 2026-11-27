using System.Text.Json;
using System.Text.RegularExpressions;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.TestApp;

[Collection(nameof(ManagedConsoleCollection))]
public class DisplayNameTests
{
    [Fact]
    public async Task SharedCaptionScanPreservesRegexBehavior()
    {
        string[] names = ["", "a", "ABC", "abc", "Text Layout", "ArcToFlower", "aAaAaA", "PNGImage", "foo123Bar", "éA", "aÉ", "Ж😀aB", "a\0B"];
        string source = """
            using System;
            using Xui.Apps.TestApp.Pages;
            public static class Program { public static int Main() {
            """ + "foreach(var name in new string[] {" + string.Join(",", names.Select(name => JsonSerializer.Serialize(name))) + "}) Console.WriteLine(ExampleDisplayName.Format(name)); return 42; } }";
        string helper = File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Pages/ExampleDisplayName.cs"));
        byte[] image = Frontend.Roslyn.Compile(source, additionalSources: [helper]);
        string expected = string.Concat(names.Select(name => Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ") + "\n"));
        Assert.Equal(expected, GcTests.RunManaged(image).Output);
        using var app = CanvasTests.Read(image);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal(expected, result.Output);
        Assert.Equal(42, result.ExitCode);
    }
}
