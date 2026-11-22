using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class AssemblySnapshotTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("Assemblies", "Contracts", OutputKind.DynamicallyLinkedLibrary, 5)]
    [InlineData("Assemblies", "ControlFlow", OutputKind.DynamicallyLinkedLibrary, 5)]
    [InlineData("Assemblies", "Inheritance", OutputKind.DynamicallyLinkedLibrary, 5)]
    [InlineData("Assemblies", "HelloWorldMain", OutputKind.ConsoleApplication, 2)]
    [InlineData("Structs", "PointVector", OutputKind.ConsoleApplication, 4)]
    [InlineData("Structs", "Layouts", OutputKind.DynamicallyLinkedLibrary, 12)]
    [InlineData("Structs", "ReferenceFields", OutputKind.DynamicallyLinkedLibrary, 10)]
    [InlineData("Gc", "Objects", OutputKind.ConsoleApplication, 6)]
    [InlineData("Gc", "Unit", OutputKind.ConsoleApplication, 3)]
    [InlineData("Polymorphism", "Units", OutputKind.ConsoleApplication, 7)]
    [InlineData("Generics", "Boxes", OutputKind.ConsoleApplication, 3)]
    public void AssemblyDepthFirstDumpMatchesMarkdown(string folder, string fixture, OutputKind outputKind, int typeCount)
    {
        string directory = SourceDirectory();
        string source = File.ReadAllText(Path.Combine(directory, "..", "..", "Fixtures", folder, fixture + ".cs")).ReplaceLineEndings("\n");
        byte[] image = Frontend.Roslyn.Compile(source, outputKind, "XuiNet." + fixture);
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        Assert.Equal(typeCount, assembly.ManifestModule.Types.Count); // Includes <Module>.
        string markdown = AssemblyMarkdown.Write(assembly, fixture, $"Fixtures/{folder}/{fixture}.cs");
        output.WriteLine(markdown);

        Snapshot.AssertMatches(fixture, markdown);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
