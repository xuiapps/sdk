using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>An instance-local budget for emitted method bodies, with reserve for cleanup.</summary>
internal sealed class ExecutionStackRuntime
{
    public const int SoftLimit = 192;
    public const int HardLimit = 256;
    public bool Enabled { get; set; }
    public GlobalIndex Depth { get; set; }
}
