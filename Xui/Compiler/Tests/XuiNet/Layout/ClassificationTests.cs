using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.Layout;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.Layout;

public class ClassificationTests
{
    [Theory]
    [InlineData("ReferenceFields")]
    [InlineData("Layouts")]
    [InlineData("PointVector")]
    public void ClassificationAgreesWithClrAndDoesNotDependOnPointerSize(string fixture)
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Structs", fixture + ".cs"));
        byte[] image = Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary);
        var managed = Assembly.Load(image);
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var narrow = new LayoutResolver(module, TargetLayout.Wasm32);
        var wide = new LayoutResolver(module, TargetLayout.X64);
        foreach (var handle in module.Types)
        {
            var type = module.GetType(handle);
            string name = type.Namespace.Length == 0 ? type.Name : type.Namespace + "." + type.Name;
            var managedType = managed.GetType(name);
            if (managedType is null || !managedType.IsValueType) continue;
            bool references = (bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!
                .MakeGenericMethod(managedType).Invoke(null, null)!;
            var expected = references ? StorageKind.ContainsManagedReferences : StorageKind.LinearMemory;
            Assert.Equal(expected, narrow.Classify(handle));
            Assert.Equal(expected, wide.Classify(handle));
            Assert.Equal(expected, narrow.Classify(handle));
            // Classification must not invent a byte layout for reference fields.
            if (references) Assert.Throws<NotSupportedException>(() => narrow.Resolve(handle));
        }
    }

    [Theory]
    [InlineData("public struct Sample<T> { public T Value; }", "Generic classification")]
    [InlineData("public struct Sample { public System.DateTime Value; }", "External value-type classification")]
    [InlineData("public class Sample { public int Value; }", "Only value types")]
    public void UnresolvedTypesAreNotAssumedToBeReferenceFree(string source, string reason)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = module.Types.Single(h => module.GetType(h).Name.StartsWith("Sample", StringComparison.Ordinal));
        var resolver = new LayoutResolver(module, TargetLayout.Wasm32);
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => resolver.Classify(handle)).Message);
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => resolver.Classify(handle)).Message);
    }

    [Theory]
    [InlineData("public ref struct Sample { public ref int Value; }", StorageKind.ContainsManagedReferences)]
    [InlineData("public ref struct Sample { public string Value; }", StorageKind.ContainsManagedReferences)]
    [InlineData("public ref struct Sample { public int Value; }", StorageKind.LinearMemory)]
    [InlineData("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)] public struct Sample { public int Value; }", StorageKind.LinearMemory)]
    public void ClassificationIsIndependentOfRefStructAndLayoutKind(string source, StorageKind expected)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = module.Types.Single(h => module.GetType(h).Name == "Sample");
        Assert.Equal(expected, new LayoutResolver(module, TargetLayout.Wasm32).Classify(handle));
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
