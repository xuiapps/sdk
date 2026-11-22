using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdnull()
    {
        writer.RefNull(ValueTypeCode.EqRef);
        stack.Push(new MsilStackEntry(SignatureTypeCode.Object) { IsNull = true });
    }
}
