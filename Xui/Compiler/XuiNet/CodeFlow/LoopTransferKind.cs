namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>The destination of a structured loop transfer.</summary>
public enum LoopTransferKind
{
    /// <summary>Passes loop parameters to the next iteration.</summary>
    Continue,
    /// <summary>Passes loop results to the continuation.</summary>
    Break
}
