using Writer = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public void WriteXmlExceptionDefaults(Writer writer)
    {
        if (XmlExceptionsEnabled)
            writer.I32Const(0).I32Const(0).RefNull(ValueTypeCode.ExternRef).RefNull(ValueTypeCode.ExternRef);
    }
}
