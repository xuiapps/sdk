using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Retained native JavaScript string formatters.</summary>
internal sealed partial class FormattingRuntime
{
    public bool UsesInt32 { get; set; }
    private TypeIndex int32Type;
    public FunctionIndex Int32 { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        DefineCompositeTypes(ref writer);
        if (UsesInt32) int32Type = writer.FunctionType([I32], NonNullExternRef);
    }
    public void Import(ref ImportWriter writer)
    {
        ImportComposite(ref writer);
        if (UsesInt32) Int32 = writer.Function("xui-string", "int32-to-string", int32Type);
    }
}
