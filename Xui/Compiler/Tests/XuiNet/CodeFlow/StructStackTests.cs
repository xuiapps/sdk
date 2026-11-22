using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.XuiNet.CodeFlow;

public class StructStackTests(ITestOutputHelper output)
{
    [Fact]
    public void PointVectorMethodsRetainStructAndAddressTypes()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Structs", "PointVector.cs"));
        var text = new StringBuilder($"# Point + Vector typed stacks\n\nSource: [PointVector.cs](Fixtures/Structs/PointVector.cs)\n\n```csharp\n{source.TrimEnd()}\n```\n");
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, optimizationLevel: optimization));
            using var assembly = AssemblyCompiler.Read(stream);
            var module = assembly.ManifestModule;
            text.AppendLine($"\n## {optimization}");
            foreach (var handle in module.Methods)
            {
                var method = module.GetMethod(handle);
                var declared = SignatureReader.Method(module, method.Signature);
                var graph = Analyzer.Analyze(module, handle);
                string owner = module.GetType(method.DeclaringType).Name;
                text.AppendLine($"\n### {owner}.{method.Name}\n");
                text.AppendLine($"- Parameters: `{string.Join(", ", declared.Parameters)}`");
                text.AppendLine($"- Result: `{declared.Result}`");
                if (declared.HasThis) text.AppendLine($"- Receiver: `{owner}&`");
                text.AppendLine("\n| Offset | Instruction | Before | After | Lowest |\n| --- | --- | --- | --- | --- |");
                foreach (var node in graph.Instructions)
                {
                    Assert.True(node.IsReachable);
                    foreach (var successor in node.Successors) Assert.Same(node.After, successor.Before);
                    text.AppendLine($"| IL_{node.Offset:x4} | `{node.Code.ToString().ToLowerInvariant().Replace('_', '.')}` | S{node.Before!.Id} | S{node.After!.Id} | S{node.Lowest!.Id} |");
                }
                text.AppendLine("\n| State | Stack (bottom to top) |\n| --- | --- |");
                foreach (var state in graph.Stacks.States)
                {
                    var entries = new List<string>();
                    for (var prefix = state; prefix.Entry is { } entry; prefix = prefix.Parent!) entries.Add(entry.ToString());
                    entries.Reverse();
                    text.AppendLine($"| S{state.Id} | {(entries.Count == 0 ? "empty" : string.Join(", ", entries))} |");
                }
                if (method.Name == ".ctor")
                {
                    var receiver = new SignatureType(SignatureTypeCode.TypeHandle,
                        new TypeIdentity(module, method.DeclaringType), IsByReference: true);
                    Assert.Contains(graph.Stacks.States, state => state.Entry?.DeclaredType == receiver);
                }
            }
        }
        output.WriteLine(text.ToString());
        Snapshot.AssertMatches("PointVector", text.ToString());
    }

    [Fact]
    public void EqualSizedStructsAndIdenticalTokensInDifferentModulesRemainDistinct()
    {
        byte[] image = Frontend.Roslyn.Compile("public struct Point { public int X; } public struct Vector { public int X; }", OutputKind.DynamicallyLinkedLibrary);
        using var firstStream = new MemoryStream(image);
        using var secondStream = new MemoryStream(image);
        using var first = AssemblyCompiler.Read(firstStream);
        using var second = AssemblyCompiler.Read(secondStream);
        var point = first.ManifestModule.Types.Single(h => first.ManifestModule.GetType(h).Name == "Point");
        var vector = first.ManifestModule.Types.Single(h => first.ManifestModule.GetType(h).Name == "Vector");
        var tree = new StackTree();
        var p = StackEntry.From(new(SignatureTypeCode.TypeHandle, new TypeIdentity(first.ManifestModule, point)));
        var v = StackEntry.From(new(SignatureTypeCode.TypeHandle, new TypeIdentity(first.ManifestModule, vector)));
        var otherModule = StackEntry.From(new(SignatureTypeCode.TypeHandle, new TypeIdentity(second.ManifestModule, point)));
        Assert.Same(tree.Empty.Push(p), tree.Empty.Push(p));
        Assert.NotSame(tree.Empty.Push(p), tree.Empty.Push(v));
        Assert.NotSame(tree.Empty.Push(p), tree.Empty.Push(otherModule));
        Assert.NotSame(tree.Empty.Push(p), tree.Empty.Push(StackEntry.From(p.DeclaredType!.Value.ByReference())));
    }

    [Fact]
    public void SmallIntegerValuesNormalizeButTheirAddressesDoNot()
    {
        var tree = new StackTree();
        var small = new SignatureType(SignatureTypeCode.Int16);
        var integer = new SignatureType(SignatureTypeCode.Int32);
        Assert.Same(tree.Empty.Push(StackEntry.From(small)), tree.Empty.Push(StackEntry.From(integer)));
        Assert.NotSame(tree.Empty.Push(StackEntry.From(small.ByReference())), tree.Empty.Push(StackEntry.From(integer.ByReference())));
    }

    [Fact]
    public void StructResultsSurviveAConditionalJoin()
    {
        const string source = """
            public struct Point { public int X; }
            public static class Program
            {
                static Point Identity(Point value) => value;
                public static Point Choose(int condition, Point left, Point right) => Identity(condition > 0 ? left : right);
            }
            """;
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var method = module.Methods.Single(h => module.GetMethod(h).Name == "Choose");
        var graph = Analyzer.Analyze(module, method);
        var conditional = Assert.Single(StructuredBlocks.Build(graph).Children.OfType<ConditionalBlock>());
        var result = Assert.Single(conditional.Results);
        Assert.Equal(StackType.ValueType, result.Type);
        Assert.Equal("Point", result.DeclaredType!.Value.Identity!.Value.ToString());
    }

    [Fact]
    public void GenericSignaturesRemainUnsupported()
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile("public static class Program { public static T Identity<T>(T value) => value; }", OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var method = module.Methods.Single();
        Assert.Throws<NotSupportedException>(() => Analyzer.Analyze(module, method));
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
