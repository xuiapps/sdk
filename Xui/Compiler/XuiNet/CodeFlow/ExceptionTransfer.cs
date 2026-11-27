namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Forwards a nested region's completed leave through its enclosing protected scopes.</summary>
public sealed class ExceptionTransfer(int target) : FlowNode
{
    public int Target { get; } = target;
}
