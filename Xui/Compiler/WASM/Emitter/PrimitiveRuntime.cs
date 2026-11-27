using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Optional host operations for native JavaScript primitive values.</summary>
internal sealed class PrimitiveRuntime
{
    public bool UsesStringHash { get; set; }
    public bool UsesObjectHash { get; set; }
    private TypeIndex objectHashType;
    public FunctionIndex ObjectHash { get; private set; }
    private TypeIndex stringHashType;
    public FunctionIndex StringHash { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        if (UsesObjectHash) objectHashType = writer.FunctionType([ExternRef], I32);
        if (UsesStringHash) stringHashType = writer.FunctionType([ExternRef], I32);
    }
    public void Import(ref ImportWriter writer)
    {
        if (UsesObjectHash) ObjectHash = writer.Function("xui-object", "identity-hash", objectHashType);
        if (UsesStringHash) StringHash = writer.Function("xui-string", "ordinal-hash", stringHashType);
    }
}
