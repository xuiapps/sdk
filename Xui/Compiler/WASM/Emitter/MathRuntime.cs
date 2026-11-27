using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Imports only the scalar math helpers retained by reachable methods.</summary>
internal sealed class MathRuntime
{
    public bool UsesAtan2 { get; set; }
    public FunctionIndex Atan2 { get; private set; }
    public bool UsesCbrt { get; set; }
    public FunctionIndex Cbrt { get; private set; }
    public bool UsesPow { get; set; }
    public bool UsesSin { get; set; }
    public bool UsesExp { get; set; }
    public FunctionIndex Exp { get; private set; }
    public bool UsesLog { get; set; }
    public FunctionIndex Log { get; private set; }
    public bool UsesCos { get; set; }
    public bool UsesTan { get; set; }
    public FunctionIndex Tan { get; private set; }
    public bool UsesRemainder { get; set; }
    public FunctionIndex Remainder { get; private set; }
    private TypeIndex binaryType, unaryType;
    public FunctionIndex Pow { get; private set; }
    public FunctionIndex Sin { get; private set; }
    public FunctionIndex Cos { get; private set; }

    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesPow || UsesRemainder || UsesAtan2) binaryType = writer.FunctionType([F64, F64], F64);
        if (UsesSin || UsesCos || UsesTan || UsesLog || UsesExp || UsesCbrt) unaryType = writer.FunctionType([F64], F64);
    }

    public void Import(ref ImportWriter writer)
    {
        if (UsesCbrt) Cbrt = writer.Function("xui-math", "cbrt", unaryType);
        if (UsesAtan2) Atan2 = writer.Function("xui-math", "atan2", binaryType);
        if (UsesPow) Pow = writer.Function("xui-math", "pow", binaryType);
        if (UsesRemainder) Remainder = writer.Function("xui-math", "remainder", binaryType);
        if (UsesSin) Sin = writer.Function("xui-math", "sin", unaryType);
        if (UsesExp) Exp = writer.Function("xui-math", "exp", unaryType);
        if (UsesLog) Log = writer.Function("xui-math", "log", unaryType);
        if (UsesCos) Cos = writer.Function("xui-math", "cos", unaryType);
        if (UsesTan) Tan = writer.Function("xui-math", "tan", unaryType);
    }
}
