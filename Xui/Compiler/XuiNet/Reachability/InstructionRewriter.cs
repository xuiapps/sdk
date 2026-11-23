using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Collects a host replacement once per reachable instruction; true suppresses original dependencies.</summary>
/// <remarks>The reader starts after the opcode. A declined reader is discarded. Replacement dependencies
/// belong to the host; the graph retains a Rewrite node identifying this boundary.</remarks>
public delegate bool InstructionRewriter(MethodIdentity method, int offset, Instruction instruction, ref ILReader reader);
