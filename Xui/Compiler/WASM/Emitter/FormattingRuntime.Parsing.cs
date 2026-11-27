using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class FormattingRuntime
{
    public bool UsesDoubleParsing { get; set; }
    public bool UsesUInt32Parsing { get; set; }
    private TypeIndex parseDoubleType, parseUInt32Type;
    public FunctionIndex ParseDouble { get; private set; }
    public FunctionIndex ParseUInt32 { get; private set; }
    private void DefineParsingTypes(ref TypeWriter writer)
    {
        if (UsesDoubleParsing) parseDoubleType = writer.FunctionType([ExternRef, I32], [F64, I32]);
        if (UsesUInt32Parsing) parseUInt32Type = writer.FunctionType([ExternRef, I32], [I32, I32]);
    }
    private void ImportParsing(ref ImportWriter writer)
    {
        if (UsesDoubleParsing) ParseDouble = writer.Function("xui-string", "parse-double", parseDoubleType);
        if (UsesUInt32Parsing) ParseUInt32 = writer.Function("xui-string", "parse-uint32", parseUInt32Type);
    }
}
