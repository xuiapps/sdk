using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.InteropServices;

namespace Xui.Compiler.Tests.Wasm.Gpu;

public class ShaderGenerationTests
{
    [Fact]
    public void ProductionShadersIncludeWebGlSources()
    {
        using var stream = new MemoryStream(Frontend.MobileProjects.Images["TestApp.Mobile"]);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var shaders = new Dictionary<string, string>();
        foreach (string name in new[] { "CubeShader", "FloorplanShader" })
        {
            var type = metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).Single(t => metadata.GetString(t.Name) == name);
            var constants = type.GetFields().Select(metadata.GetFieldDefinition)
                .Where(f => !f.GetDefaultValue().IsNil)
                .ToDictionary(f => metadata.GetString(f.Name), f => {
                    var value = metadata.GetConstant(f.GetDefaultValue());
                    var reader = metadata.GetBlobReader(value.Value);
                    return reader.ReadUTF16(reader.Length);
                });
            Assert.False(constants.ContainsKey("GlslError"), constants.GetValueOrDefault("GlslError"));
            string source = constants["GlslSource"];
            Assert.Equal("main", constants["GlslVertexEntry"]);
            Assert.Equal("main", constants["GlslFragmentEntry"]);
            Assert.StartsWith("#version 300 es", source);
            Snapshot.AssertFileMatches(name, ".glsl", source);
            shaders.Add(name, source);
        }
        string directory = Path.Combine(AppContext.BaseDirectory, "Gpu");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "shaders.json"), JsonSerializer.Serialize(shaders));
    }
    [Theory]
    [InlineData("Perspective", "smooth out")]
    [InlineData("Flat", "flat out")]
    [InlineData("Linear", "Non-perspective interpolation")]
    public void InterpolationSurvivesGenerationAndUnavailableWebGlDoesNotBreakNative(string mode, string expected)
    {
        string directory = Path.Combine(Frontend.SourceProject.XuiRoot, "Apps/TestApp/Pages/ThreeD/Tests/GpuHardwareCubeTest");
        string source = File.ReadAllText(Path.Combine(directory, "CubeShader.cs"))
            .Replace("[Location(0)]", $"[Location(0), Interpolation(InterpolationMode.{mode})]");
        var references = Directory.EnumerateFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll")
            .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal))
            .Append(typeof(Xui.GPU.Shaders.Types.Float4).Assembly.Location).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("ShaderInterpolation" + mode,
            new[] { source, File.ReadAllText(Path.Combine(directory, "CubeMesh.cs")), "global using System; global using System.Collections.Generic;" }
                .Select(text => CSharpSyntaxTree.ParseText(text)), references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create(new Xui.CompileTime.ShaderProgramGenerator().AsSourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var generated, out var diagnostics);
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        string result = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        Assert.Contains(expected, result);
        if (mode == "Linear")
        {
            Assert.Contains("GlslError", result);
            Assert.DoesNotContain("GlslSource", result);
            Assert.Contains("noperspective", result);
            Assert.Contains("HlslSource", result); Assert.Contains("MslSource", result);
        }
        using var image = new MemoryStream();
        var emit = generated.Emit(image);
        Assert.True(emit.Success, string.Join("\n", emit.Diagnostics));
    }

}
