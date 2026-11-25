using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryManagedReference(Instruction.Token instruction, int offset)
    {
        var access = ManagedReference.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        var address = method.Storage.Bind(access.Element.ByReference());
        if (access.Operation == "NullRef")
        {
            writer.RefNull(ValueTypeCode.EqRef).I32Const(-1);
            stack.Push(new(address.TypeCode, address.Identity, true, address.Array));
        }
        else
        {
            PopAssignable(address, instruction.Code, offset);
            writer.I32Const(-1).I32Eq().LocalSet(method.GcI32).RefIsNull().LocalGet(method.GcI32).I32And();
            stack.Push(new(SignatureTypeCode.Int32));
        }
        return true;
    }
}
