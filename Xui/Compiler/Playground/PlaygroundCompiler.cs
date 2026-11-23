using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;

namespace Xui.Compiler.Playground;

/// <summary>Compiles editor source through Roslyn, the PE reader, and the Xui WASM emitter.</summary>
public static class PlaygroundCompiler
{
    private static readonly MetadataReference[] References = [Reference("System.Runtime"), Reference("System.Console")];

    /// <summary>Returns a standalone WASM module with the playground's console imports.</summary>
    public static byte[] Compile(string source)
    {
        var compilation = CSharpCompilation.Create("Playground",
            [CSharpSyntaxTree.ParseText(source, path: "Program.cs")], References,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release, deterministic: true, concurrentBuild: false));
        using var pe = new MemoryStream();
        var result = compilation.Emit(pe);
        if (!result.Success)
            throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        pe.Position = 0;
        using var assembly = AssemblyCompiler.Read(pe);
        return new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
    }

    private static MetadataReference Reference(string name)
    {
        using var stream = typeof(PlaygroundCompiler).Assembly.GetManifestResourceStream($"Playground.Reference.{name}.dll")
            ?? throw new InvalidOperationException($"Missing compiler reference: {name}.");
        return MetadataReference.CreateFromStream(stream);
    }
}
