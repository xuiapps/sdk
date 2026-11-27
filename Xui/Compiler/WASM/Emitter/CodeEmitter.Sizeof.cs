using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitSizeof(Instruction.Token instruction, int offset)
    {
        writer.I32Const(SizeOfType(method, source, instruction.Value));
        stack.Push(new(SignatureTypeCode.Int32));
    }

    private static int SizeOfType(MethodPlan method, Module source, int token)
    {
        var type = SignatureReader.StorageType(source, MetadataTokens.EntityHandle(token), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        var storage = method.Storage.Bind(type);
        if (storage.IsByReference || StorageModel.IsReference(storage) || storage.UsesGc ||
            storage.Identity is { } identity && method.Storage.HasStackReferences(identity))
            throw new NotSupportedException($"sizeof currently requires a reference-free value type: {type}.");
        return ElementSize(method.Storage, type);
    }
}
