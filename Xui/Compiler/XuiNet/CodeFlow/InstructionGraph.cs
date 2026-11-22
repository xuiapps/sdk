namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A method's instruction graph and its canonical tree of stack states.</summary>
public sealed class InstructionGraph
{
    /// <summary>Indexes IL instructions and edges without assigning stack states.</summary>
    public static InstructionGraph Read(ILReader reader) => GraphReader.Read(reader);

    internal InstructionGraph(IReadOnlyList<InstructionNode> instructions, StackTree stacks)
    {
        Instructions = instructions;
        Stacks = stacks;
    }

    /// <summary>Instructions in IL offset order, including unreachable instructions.</summary>
    public IReadOnlyList<InstructionNode> Instructions { get; }
    /// <summary>The method entry instruction, whose incoming stack is empty.</summary>
    public InstructionNode Entry => Instructions[0];
    /// <summary>The shared stack-state tree for every instruction in this graph.</summary>
    public StackTree Stacks { get; }
}
