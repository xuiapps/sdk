using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Follows method-local control-flow edges and interns the stack types before and after each instruction.</summary>
public static class Analyzer
{
    /// <summary>Analyzes supported MSIL without reconstructing structured blocks or emitting WASM.</summary>
    /// <remarks>Reference joins widen to a shared same-module base class and requeue affected instructions.
    /// Other slots must match exactly. States remain canonical nodes in the method's stack tree.</remarks>
    public static InstructionGraph Analyze(Module module, MethodDefinitionHandle method)
        => Analyze(module, MethodIdentity.FromDefinition(module, method));

    /// <summary>Analyzes a method with its constructed declaring-type context.</summary>
    public static InstructionGraph Analyze(Module module, MethodIdentity method)
    {
        var definition = module.GetMethod(method.Handle);
        var body = definition.Body ?? throw new NotSupportedException("The method has no managed body.");
        if (body.ExceptionRegions.Length != 0)
            throw new NotSupportedException("Exception-region stack analysis is not implemented.");
        var analysis = new MethodAnalysis(module, body, Signature.Read(module, definition.Signature, method.DeclaringType), method.DeclaringType);
        var graph = GraphReader.Read(body.Instructions);
        var work = new Queue<InstructionNode>();
        graph.Entry.Before = graph.Stacks.Empty;
        work.Enqueue(graph.Entry);
        while (work.TryDequeue(out var instruction))
        {
            analysis.Apply(instruction);
            foreach (var successor in instruction.Successors)
            {
                if (successor.Before is null)
                {
                    successor.Before = instruction.After;
                    work.Enqueue(successor);
                }
                else if (!ReferenceEquals(successor.Before, instruction.After))
                {
                    StackState merged;
                    try { merged = StackMerge.Merge(successor.Before, instruction.After!); }
                    catch (InvalidProgramException error)
                    {
                        throw new InvalidProgramException($"Stack mismatch on edge IL_{instruction.Offset:x4} -> IL_{successor.Offset:x4}: " +
                            $"incoming S{instruction.After!.Id} (depth {instruction.After.Depth}), expected S{successor.Before.Id} (depth {successor.Before.Depth}).", error);
                    }
                    if (!ReferenceEquals(merged, successor.Before))
                    {
                        successor.Before = merged;
                        work.Enqueue(successor);
                    }
                }
            }
        }
        return graph;
    }
}
