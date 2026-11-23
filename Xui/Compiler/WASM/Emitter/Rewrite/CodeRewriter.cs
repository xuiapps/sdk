using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.WASM.Emitter.Rewrite;

/// <summary>Optionally replaces MSIL instructions and supplies their WASM types and imports.</summary>
/// <remarks>
/// An emission begins with BeginModule, collects requirements, defines types and imports, then emits code.
/// Collect and TryRewrite receive the cursor immediately after the opcode and must consume the same
/// operands when handling an instruction. Return false to leave it to the normal emitter.
/// A declined cursor is discarded. A declined rewrite must not write code or change the evaluation stack.
/// Instances may retain module state and must not be shared by concurrent emissions.
/// </remarks>
public abstract class CodeRewriter
{
    /// <summary>Starts an emission; reset any state from a previous module here.</summary>
    public virtual void BeginModule(Module source) { }

    /// <summary>Collects requirements once per reachable instruction during source reachability; true replaces its managed dependencies.</summary>
    public virtual bool Collect(Instruction instruction, ref ILReader reader) => false;

    /// <summary>Defines any types needed by the collected replacements.</summary>
    public virtual void DefineTypes(ref TypeWriter types) { }

    /// <summary>Imports functions needed by the collected replacements, retaining their assigned indices.</summary>
    public virtual void ImportFunctions(ref ImportWriter imports) { }

    /// <summary>Emits a replacement and updates the MSIL stack; returns false to use normal emission.</summary>
    /// <remarks>The emitter and its writer are borrowed; do not dispose the writer.</remarks>
    public virtual bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset) => false;
}
