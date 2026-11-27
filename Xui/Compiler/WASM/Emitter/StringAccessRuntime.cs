using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Native UTF-16 access; managed null and range checks are emitted before invoking the builtins.</summary>
internal sealed class StringAccessRuntime
{
    public bool UsesSearch { get; set; }
    public bool UsesReplace { get; set; }
    public bool UsesIgnoreCaseEquality { get; set; }
    private TypeIndex ignoreCaseType;
    public FunctionIndex IgnoreCaseEquality { get; private set; }
    private TypeIndex replaceType;
    public FunctionIndex Replace { get; private set; }
    private TypeIndex searchType;
    public FunctionIndex Search { get; private set; }
    public bool UsesLowerInvariant { get; set; }
    private TypeIndex lowerType;
    public FunctionIndex LowerInvariant { get; private set; }
    public bool UsesTextElements { get; set; }
    private TypeIndex textElementType;
    public FunctionIndex TextElement { get; private set; }
    public bool UsesCharFormatting { get; set; }
    private TypeIndex fromCharType;
    public FunctionIndex FromChar { get; private set; }
    public bool Enabled { get; set; }
    public bool UsesSpans { get; set; }
    private TypeIndex lengthType, charType, sliceType;
    public FunctionIndex Length { get; private set; }
    public FunctionIndex Character { get; private set; }
    public FunctionIndex Slice { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesIgnoreCaseEquality) ignoreCaseType = writer.FunctionType([ExternRef, ExternRef], I32);
        if (UsesReplace) replaceType = writer.FunctionType([ExternRef, ExternRef, ExternRef], NonNullExternRef);
        if (UsesSearch) searchType = writer.FunctionType([ExternRef, ExternRef, I32], I32);
        if (UsesLowerInvariant) lowerType = writer.FunctionType([ExternRef], NonNullExternRef);
        if (UsesTextElements) textElementType = writer.FunctionType([ExternRef], I32);
        if (UsesCharFormatting) fromCharType = writer.FunctionType([I32], NonNullExternRef);
        if (!Enabled) return;
        lengthType = writer.FunctionType([ExternRef], I32);
        charType = writer.FunctionType([ExternRef, I32], I32);
        sliceType = writer.FunctionType([ExternRef, I32, I32], NonNullExternRef);
    }
    public void Import(ref ImportWriter writer)
    {
        if (UsesIgnoreCaseEquality) IgnoreCaseEquality = writer.Function("xui-string", "equals-ordinal-ignore-case", ignoreCaseType);
        if (UsesReplace) Replace = writer.Function("xui-string", "replace-ordinal", replaceType);
        if (UsesSearch) Search = writer.Function("xui-string", "search-ordinal", searchType);
        if (UsesLowerInvariant) LowerInvariant = writer.Function("xui-string", "lower-invariant", lowerType);
        if (UsesTextElements) TextElement = writer.Function("xui-string", "text-element-length", textElementType);
        if (UsesCharFormatting) FromChar = writer.Function("wasm:js-string", "fromCharCode", fromCharType);
        if (!Enabled) return;
        Length = writer.Function("wasm:js-string", "length", lengthType);
        Character = writer.Function("wasm:js-string", "charCodeAt", charType);
        Slice = writer.Function("wasm:js-string", "substring", sliceType);
    }
}
