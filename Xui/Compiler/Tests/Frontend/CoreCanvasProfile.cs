using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Compiles the complete Core source tree into its own implementation PE with instrumentation omitted and equivalent inline font defaults.</summary>
/// <remarks>Application sources are compiled separately against this PE. Normal Core builds do not define this profile.</remarks>
internal static class CoreCanvasProfile
{
    private static readonly Lazy<byte[]> image = new(Compile);
    public static byte[] Image => image.Value;

    private static byte[] Compile()
    {
        string root = Path.GetFullPath(Path.Combine(SourceDirectory(), "../../../Core/Core"));
        var options = new CSharpParseOptions(LanguageVersion.CSharp14, preprocessorSymbols: ["XUI_COMPILER_CANVAS"]);
        var sources = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Order(StringComparer.Ordinal).Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path)).ToList();
        sources.Add(CSharpSyntaxTree.ParseText("""
            global using System;
            global using System.Collections.Generic;
            global using System.IO;
            global using System.Linq;
            global using System.Net.Http;
            global using System.Threading;
            global using System.Threading.Tasks;
            """, options));
        var references = Directory.EnumerateFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll")
            .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) is "mscorlib.dll" or "netstandard.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Xui.Core", sources, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable, deterministic: true));
        using var output = new MemoryStream();
        var result = compilation.Emit(output);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        return output.ToArray();
    }

    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}
