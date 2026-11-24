using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Imports only the scalar math helpers retained by reachable methods.</summary>
internal sealed class MathRuntime
{
    public bool UsesPow { get; set; }
    private TypeIndex powType;
    public FunctionIndex Pow { get; private set; }

    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesPow) powType = writer.FunctionType([F64, F64], F64);
    }

    public void Import(ref ImportWriter writer)
    {
        if (UsesPow) Pow = writer.Function("xui-math", "pow", powType);
    }
}
