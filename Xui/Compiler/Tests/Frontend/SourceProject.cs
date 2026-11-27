using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Compiles repository sources as separate implementation PEs for compiler integration fixtures.</summary>
internal static class SourceProject
{
    public static string XuiRoot { get; } = GetRoot();
    private static string GetRoot([CallerFilePath] string file = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "../../.."));

    public static byte[] Compile(string relativeDirectory, string assemblyName, IEnumerable<MetadataReference> references,
        IEnumerable<string>? extraSources = null, IEnumerable<string>? exclude = null, bool generateShaders = false,
        IEnumerable<ResourceDescription>? resources = null)
    {
        string root = Path.Combine(XuiRoot, relativeDirectory);
        var options = new CSharpParseOptions(LanguageVersion.CSharp14, preprocessorSymbols: ["XUI_COMPILER_CANVAS"]);
        var excluded = new HashSet<string>(exclude ?? [], StringComparer.Ordinal);
        var sources = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj" or "Platforms" or "Properties") &&
                !excluded.Contains(Path.GetRelativePath(root, path)))
            .Order(StringComparer.Ordinal).Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path)).ToList();
        sources.Add(CSharpSyntaxTree.ParseText("""
            global using System; global using System.Collections.Generic; global using System.IO;
            global using System.Linq; global using System.Net.Http; global using System.Threading; global using System.Threading.Tasks;
            """, options));
        foreach (string source in extraSources ?? []) sources.Add(CSharpSyntaxTree.ParseText(source, options));
        var runtimeReferences = Directory.EnumerateFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll")
            .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) is "mscorlib.dll" or "netstandard.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(assemblyName, sources, runtimeReferences.Concat(references),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable, deterministic: true));
        Compilation complete = compilation;
        if (generateShaders)
            CSharpGeneratorDriver.Create([new Xui.CompileTime.ShaderProgramGenerator().AsSourceGenerator()], parseOptions: options)
                .RunGeneratorsAndUpdateCompilation(compilation, out complete, out _);
        using var output = new MemoryStream(); var result = complete.Emit(output, manifestResources: resources);
        if (!result.Success) throw new InvalidOperationException(assemblyName + ":\n" + string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        return output.ToArray();
    }
}
