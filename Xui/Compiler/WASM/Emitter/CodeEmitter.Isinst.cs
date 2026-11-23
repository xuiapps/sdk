using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitIsinst(Instruction.Token instruction, int offset) => EmitTypeTest(instruction, offset, false);

    private void EmitTypeTest(Instruction.Token instruction, int offset, bool throwing)
    {
        var target = Xui.Compiler.XuiNet.DotNet.Signatures.SignatureReader.Resolve(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments).Identity
            ?? throw new NotSupportedException("Type tests currently require same-module classes.");
        var value = Pop(instruction.Code, offset);
        if (!value.IsNull && (value.IsByReference || value.Identity is not { } identity || !method.Storage.IsClass(identity)))
            throw new NotSupportedException("Class type tests currently accept only same-module class references.");
        if (throwing) writer.LocalTee(method.GcSource);
        writer.GlobalGet(method.Storage.Classes.Resolve(target).Descriptor).Call(method.Storage.InstanceTest);
        if (throwing)
        {
            writer.LocalSet(method.GcOwner).LocalGet(method.GcSource).RefIsNull().I32Eqz()
                .LocalGet(method.GcOwner).RefIsNull().I32And().If().Unreachable().End().LocalGet(method.GcOwner);
        }
        stack.Push(new(SignatureTypeCode.TypeHandle, target));
    }
}
