using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBox(Instruction.Token instruction, int offset)
    {
        var token = MetadataTokens.EntityHandle(instruction.Value);
        var box = BoxingType.Read(source, token, method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        var type = method.Storage.Bind(SignatureReader.StorageType(source, token,
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments));
        PopAssignable(type, instruction.Code, offset);
        if (box is not { } value)
        {
            stack.Push(new(type.StackType, type.Identity, array: type.Array));
            return;
        }
        var shape = method.Storage.Box(value);
        var scratch = GcScratch(type);
        StorePrimitive(type, scratch);
        writer.GlobalGet(method.Storage.Classes.Resolve(value.Identity).Descriptor).LocalGet(scratch);
        if (type.TypeCode == SignatureTypeCode.Single) writer.F32DemoteF64();
        writer.StructNew(shape.Type);
        stack.Push(new(SignatureTypeCode.Object));
    }
}
