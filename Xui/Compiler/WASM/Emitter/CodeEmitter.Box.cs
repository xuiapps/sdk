using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBox(Instruction.Token instruction, int offset)
    {
        var type = method.Storage.Bind(SignatureReader.Resolve(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments));
        if (!StorageModel.IsReference(type)) throw new NotSupportedException("Boxing value types is not implemented.");
        PopAssignable(type, instruction.Code, offset);
        stack.Push(new(type.StackType, type.Identity, array: type.Array));
    }
}
