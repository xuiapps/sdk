using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Splits native strings in the host; the emitter copies results into a managed WASM array.</summary>
internal sealed class StringSplitRuntime
{
    public bool Enabled { get; set; }
    private TypeIndex splitType, charType, createType, setType, lengthType, getType;
    public FunctionIndex Split { get; private set; }
    public FunctionIndex Character { get; private set; }
    public FunctionIndex Create { get; private set; }
    public FunctionIndex Set { get; private set; }
    public FunctionIndex Length { get; private set; }
    public FunctionIndex Get { get; private set; }
    public void DefineTypes(ref TypeWriter writer)
    {
        if (!Enabled) return;
        splitType = writer.FunctionType([ExternRef, ExternRef, I32, I32, I32], ExternRef);
        charType = writer.FunctionType([I32], ExternRef);
        createType = writer.FunctionType([I32], ExternRef);
        setType = writer.FunctionType([ExternRef, I32, ExternRef]);
        lengthType = writer.FunctionType([ExternRef], I32);
        getType = writer.FunctionType([ExternRef, I32], ExternRef);
    }
    public void Import(ref ImportWriter writer)
    {
        if (!Enabled) return;
        Split = writer.Function("xui-string", "split", splitType);
        Character = writer.Function("xui-string", "character", charType);
        Create = writer.Function("xui-string", "vector", createType);
        Set = writer.Function("xui-string", "set", setType);
        Length = writer.Function("xui-string", "length", lengthType);
        Get = writer.Function("xui-string", "get", getType);
    }
}
