using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Optional monotonic elapsed-millisecond clock; its origin belongs to the host.</summary>
internal sealed class ClockRuntime
{
    public bool UsesTickCount64 { get; set; }
    public bool UsesTimestamp { get; set; }
    private TypeIndex type;
    public FunctionIndex TickCount64 { get; private set; }
    public FunctionIndex Timestamp { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesTickCount64 || UsesTimestamp) type = writer.FunctionType([], I64);
    }
    public void Import(ref ImportWriter writer)
    {
        if (UsesTickCount64) TickCount64 = writer.Function("xui-clock", "tick-count64", type);
        if (UsesTimestamp) Timestamp = writer.Function("xui-clock", "timestamp", type);
    }
}
