using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Imports only the scalar math helpers retained by reachable methods.</summary>
internal sealed class MathRuntime
{
    public bool UsesPow { get; set; }
    public bool UsesSin { get; set; }
    public bool UsesCos { get; set; }
    public bool UsesRemainder { get; set; }
    public FunctionIndex Remainder { get; private set; }
    private TypeIndex binaryType, unaryType;
    public FunctionIndex Pow { get; private set; }
    public FunctionIndex Sin { get; private set; }
    public FunctionIndex Cos { get; private set; }

    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesPow || UsesRemainder) binaryType = writer.FunctionType([F64, F64], F64);
        if (UsesSin || UsesCos) unaryType = writer.FunctionType([F64], F64);
    }

    public void Import(ref ImportWriter writer)
    {
        if (UsesPow) Pow = writer.Function("xui-math", "pow", binaryType);
        if (UsesRemainder) Remainder = writer.Function("xui-math", "remainder", binaryType);
        if (UsesSin) Sin = writer.Function("xui-math", "sin", unaryType);
        if (UsesCos) Cos = writer.Function("xui-math", "cos", unaryType);
    }
}
