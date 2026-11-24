using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryDelegateOperation(Instruction.Token instruction, int offset)
    {
        var token = MetadataTokens.EntityHandle(instruction.Value);
        if (DelegateInfo.Operation(source, token, method.Identity) is not { } operation) return false;
        var blob = token.Kind == HandleKind.MemberReference ? source.GetMemberReference((MemberReferenceHandle)token).Signature
            : source.GetMethod((MethodDefinitionHandle)token).Signature;
        var signature = SignatureReader.Method(source, blob);
        PopAssignable(method.Storage.Bind(signature.Parameters[1]), instruction.Code, offset);
        writer.LocalSet(method.GcSource);
        PopAssignable(method.Storage.Bind(signature.Parameters[0]), instruction.Code, offset);
        writer.LocalSet(method.GcOwner);
        var storage = method.Storage;
        if (operation is "Combine" or "Remove")
        {
            writer.LocalGet(method.GcOwner).RefIsNull().I32Eqz().LocalGet(method.GcSource).RefIsNull().I32Eqz().I32And().If();
            writer.LocalGet(method.GcOwner).RefCast(storage.DelegateType).StructGet(storage.DelegateType, new((uint)Xui.Compiler.WASM.Runtime.DelegateField.Descriptor))
                .LocalGet(method.GcSource).RefCast(storage.DelegateType).StructGet(storage.DelegateType, new((uint)Xui.Compiler.WASM.Runtime.DelegateField.Descriptor)).RefEq().I32Eqz().If();
            ThrowException(XuiNet.DotNet.Exceptions.ExceptionKind.ArgumentException);
            writer.End().End();
        }
        var helper = operation switch { "Combine" => DelegateHelper.Combine, "Remove" => DelegateHelper.Remove, _ => DelegateHelper.Equal };
        writer.LocalGet(method.GcOwner).LocalGet(method.GcSource).Call(storage.DelegateFunctions[helper].Function);
        if (operation == "op_Inequality") writer.I32Eqz();
        var result = storage.Bind(signature.Result);
        stack.Push(new(result.StackType, result.Identity));
        return true;
    }
}
