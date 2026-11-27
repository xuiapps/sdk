namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Runs a protected region and selects its normal continuation after cleanup.</summary>
public sealed class ExceptionDispatch(ExceptionBlock region, SwitchBlock selection) : FlowNode
{
    public ExceptionBlock Region { get; } = region;
    public SwitchBlock Selection { get; } = selection;
}
