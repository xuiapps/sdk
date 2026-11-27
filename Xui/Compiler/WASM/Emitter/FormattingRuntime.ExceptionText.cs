using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class FormattingRuntime
{
    public bool ExceptionText { get; set; }
    private TypeIndex exceptionFrameType, exceptionFinishType;
    public FunctionIndex ExceptionFrame { get; private set; }
    public FunctionIndex ExceptionFinish { get; private set; }

    private void DefineExceptionTextTypes(ref TypeWriter writer)
    {
        if (!ExceptionText) return;
        exceptionFrameType = writer.FunctionType([ExternRef, ExternRef, ExternRef, I32], ExternRef);
        exceptionFinishType = writer.FunctionType([ExternRef], ExternRef);
    }
    private void ImportExceptionText(ref ImportWriter writer)
    {
        if (!ExceptionText) return;
        ExceptionFrame = writer.Function("xui-string", "exception-frame", exceptionFrameType);
        ExceptionFinish = writer.Function("xui-string", "exception-finish", exceptionFinishType);
    }
}
