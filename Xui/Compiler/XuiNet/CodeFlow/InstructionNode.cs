using System.Collections.Immutable;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>One IL instruction and its outgoing graph edges and interned stack annotations.</summary>
/// <remarks>Operands remain in the original PE. Unreachable instructions have null stack annotations.</remarks>
public sealed class InstructionNode
{
    internal InstructionNode(int offset, int end, OpCode code)
    {
        Offset = offset;
        End = end;
        Code = code;
    }

    /// <summary>The instruction's absolute byte offset within the method.</summary>
    public int Offset { get; }
    /// <summary>The byte offset immediately following the instruction and its operands.</summary>
    public int End { get; }
    /// <summary>The original MSIL opcode, including short forms.</summary>
    public OpCode Code { get; }
    /// <summary>Possible next instructions. Conditional fallthrough precedes the taken edge.</summary>
    public ImmutableArray<InstructionNode> Successors { get; internal set; } = [];
    /// <summary>Whether this instruction can be reached from method entry through the analyzed edges.</summary>
    public bool IsReachable => Before is not null;
    /// <summary>Whether assignment at this instruction converts a native string into an object slot.</summary>
    public bool StoresStringAsObject { get; internal set; }
    /// <summary>The stack before execution; shared by all incoming edges of a reachable instruction.</summary>
    public StackState? Before { get; internal set; }
    /// <summary>The stack after execution, propagated to every outgoing edge.</summary>
    public StackState? After { get; internal set; }
    /// <summary>The shortest stack prefix reached while consuming operands, before pushing results.</summary>
    /// <remarks>This also records operands read by dup, even though its original value remains on the runtime stack.</remarks>
    public StackState? Lowest { get; internal set; }
}
