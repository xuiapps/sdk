using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;
using Xunit.Abstractions;

namespace Xui.Compiler.Tests.XuiNet.CodeFlow;

public class GraphSnapshotTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("NestedConditionals")]
    [InlineData("For")]
    [InlineData("WhileWithBreak")]
    [InlineData("WhileWithContinue")]
    [InlineData("If")]
    [InlineData("IfElse")]
    [InlineData("ElseIf")]
    [InlineData("While")]
    [InlineData("DoWhile")]
    [InlineData("Nested")]
    [InlineData("NestedLoops")]
    [InlineData("EarlyReturn")]
    [InlineData("ConditionalValue")]
    [InlineData("StackPrefix")]
    [InlineData("MixedStacks")]
    public void InstructionsReferenceCanonicalStackStates(string fixture)
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "CodeFlow", fixture + ".cs"));
        var text = new StringBuilder($"# {fixture} instruction graph\n\nSource: [{fixture}.cs](Fixtures/CodeFlow/{fixture}.cs)\n\n```csharp\n{source.TrimEnd()}\n```\n");
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary,
                "Graph." + fixture + optimization, optimization));
            using var assembly = AssemblyCompiler.Read(stream);
            var module = assembly.ManifestModule;
            var method = module.Methods.Single(handle => module.GetMethod(handle).Name == "Run");
            var graph = Analyzer.Analyze(module, method);
            Assert.Same(graph.Stacks.Empty, graph.Entry.Before);
            text.AppendLine($"\n## {optimization}\n\n### Instructions\n");
            text.AppendLine("| Offset | Instruction | Successors | Before | After | Lowest |\n| --- | --- | --- | --- | --- | --- |");
            foreach (var instruction in graph.Instructions)
            {
                if (instruction.Before is not null)
                {
                    Assert.NotNull(instruction.After);
                    Assert.NotNull(instruction.Lowest);
                    foreach (var successor in instruction.Successors) Assert.Same(instruction.After, successor.Before);
                }
                text.AppendLine($"| IL_{instruction.Offset:x4} | `{instruction.Code.ToString().ToLowerInvariant().Replace('_', '.')}` | " +
                    $"{string.Join(", ", instruction.Successors.Select(node => $"IL_{node.Offset:x4}"))} | " +
                    $"{State(instruction.Before)} | {State(instruction.After)} | {State(instruction.Lowest)} |");
            }
            text.AppendLine("\n### Stack tree\n\n| State | Parent | Top type | Depth | Children |\n| --- | --- | --- | --- | --- |");
            foreach (var state in graph.Stacks.States)
                text.AppendLine($"| S{state.Id} | {(state.Parent is null ? "—" : State(state.Parent))} | {state.Entry?.ToString() ?? "empty"} | {state.Depth} | " +
                    $"{string.Join(", ", state.Children.Select(child => $"S{child.Id}"))} |");
        }
        output.WriteLine(text.ToString());
        Snapshot.AssertMatches(fixture, text.ToString());
    }

    private static string State(StackState? state) => state is null ? "unreachable" : $"S{state.Id}";
    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
