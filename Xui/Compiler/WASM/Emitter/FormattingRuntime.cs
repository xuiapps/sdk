using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Retained native JavaScript string formatters.</summary>
internal sealed partial class FormattingRuntime
{
    public bool UsesFormattedInteger { get; set; }
    private TypeIndex formattedIntegerType;
    public FunctionIndex FormattedInteger { get; private set; }
    public bool UsesInt32 { get; set; }
    private TypeIndex int32Type;
    public FunctionIndex Int32 { get; private set; }
    public bool UsesDecimal { get; set; }
    private TypeIndex decimalType;
    public FunctionIndex Decimal { get; private set; }
    public bool UsesDouble { get; set; }
    public bool UsesSingle { get; set; }
    private TypeIndex doubleType;
    public FunctionIndex Double { get; private set; }
    public FunctionIndex Single { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        DefineCompositeTypes(ref writer);
        DefineExceptionTextTypes(ref writer);
        DefineParsingTypes(ref writer);
        if (enums.Count > 0) enumType = writer.FunctionType([I64, ExternRef], NonNullExternRef);
        if (UsesFormattedInteger) formattedIntegerType = writer.FunctionType([I32, I64, ExternRef], [ExternRef, I32]);
        if (UsesDecimal) decimalType = writer.FunctionType([I32, I32, I64], NonNullExternRef);
        if (UsesInt32 || usesUInt32) int32Type = writer.FunctionType([I32], NonNullExternRef);
        if (usesInt64 || usesUInt64) int64Type = writer.FunctionType([I64], NonNullExternRef);
        if (UsesDouble || UsesSingle) doubleType = writer.FunctionType([F64, ExternRef], [ExternRef, I32]);
    }
    public void Import(ref ImportWriter writer)
    {
        ImportComposite(ref writer);
        ImportExceptionText(ref writer);
        ImportParsing(ref writer);
        if (enums.Count > 0) Enum = writer.Function("xui-string", "enum-to-string", enumType);
        if (UsesFormattedInteger) FormattedInteger = writer.Function("xui-string", "integer-to-string", formattedIntegerType);
        if (UsesDecimal) Decimal = writer.Function("xui-string", "decimal-to-string", decimalType);
        if (UsesInt32) Int32 = writer.Function("xui-string", "int32-to-string", int32Type);
        if (usesUInt32) uint32 = writer.Function("xui-string", "uint32-to-string", int32Type);
        if (usesInt64) int64 = writer.Function("xui-string", "int64-to-string", int64Type);
        if (usesUInt64) uint64 = writer.Function("xui-string", "uint64-to-string", int64Type);
        if (UsesDouble) Double = writer.Function("xui-string", "double-to-string", doubleType);
        if (UsesSingle) Single = writer.Function("xui-string", "single-to-string", doubleType);
    }
}
