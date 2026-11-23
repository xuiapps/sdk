namespace Xui.Compiler.WASM.Emitter.Rewrite;

/// <summary>Leaves every instruction to the normal emitter and contributes no types or imports.</summary>
public sealed class NoOpCodeRewriter : CodeRewriter
{
    /// <summary>The stateless default rewriter.</summary>
    public static NoOpCodeRewriter Instance { get; } = new();
}
