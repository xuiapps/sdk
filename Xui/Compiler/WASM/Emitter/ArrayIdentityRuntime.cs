using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Weak host metadata distinguishes managed array element types with identical WASM storage.</summary>
internal sealed class ArrayIdentityRuntime
{
    public bool Enabled { get; set; }
    private TypeIndex registerType, testType;
    public FunctionIndex Register { get; private set; }
    public FunctionIndex Test { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        if (!Enabled) return;
        registerType = writer.FunctionType([ExternRef, I32]);
        testType = writer.FunctionType([ExternRef, I32], I32);
    }
    public void Import(ref ImportWriter writer)
    {
        if (!Enabled) return;
        Register = writer.Function("xui-object", "register-array", registerType);
        Test = writer.Function("xui-object", "is-array", testType);
    }
}
