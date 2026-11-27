using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

internal sealed class TaskSchedulerRuntime
{
    public MethodIdentity? Callback { get; set; }
    private TypeIndex type;
    public FunctionIndex Post { get; private set; }
    public void DefineTypes(ref TypeWriter writer) { if (Callback is not null) type = writer.FunctionType([FuncRef, EqRef]); }
    public void Import(ref ImportWriter writer) { if (Callback is not null) Post = writer.Function("xui-task", "post", type); }
}
