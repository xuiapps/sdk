using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Weak recursion counts for managed monitor objects on one JavaScript agent.</summary>
internal sealed class MonitorRuntime
{
    public const string InvalidLockTakenMessage = "Argument must be initialized to false";
    public bool Enabled { get; set; }
    private TypeIndex type;
    public FunctionIndex Change { get; private set; }

    public void DefineTypes(ref TypeWriter writer)
    {
        if (Enabled) type = writer.FunctionType([EqRef, I32], I32);
    }

    public void Import(ref ImportWriter writer)
    {
        if (Enabled) Change = writer.Function("xui-monitor", "change", type);
    }
}
