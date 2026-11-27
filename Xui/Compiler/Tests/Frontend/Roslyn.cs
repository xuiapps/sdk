using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xui.Compiler.Tests.Frontend;

internal static class Roslyn
{
    public static byte[] Compile(string source, OutputKind outputKind = OutputKind.ConsoleApplication,
        string assemblyName = "HelloWorld", OptimizationLevel optimizationLevel = OptimizationLevel.Release,
        IEnumerable<MetadataReference>? additionalReferences = null, IEnumerable<string>? additionalSources = null,
        IEnumerable<ResourceDescription>? resources = null, bool allowUnsafe = false)
    {
        var references = new[]
        {
            "System.Private.CoreLib.dll",
            "System.Runtime.dll",
            "System.Console.dll"
        }.Select(
            name => MetadataReference.CreateFromFile(
                Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), name)));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { source }.Concat(additionalSources ?? []).Select(text => CSharpSyntaxTree.ParseText(text)),
            references.Concat(additionalReferences ?? []),
            new CSharpCompilationOptions(
                outputKind,
                allowUnsafe: allowUnsafe,
                optimizationLevel: optimizationLevel,
                deterministic: true)
        );

        using var pe = new MemoryStream();
        var result = compilation.Emit(pe, manifestResources: resources);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        }

        return pe.ToArray();
    }
}
