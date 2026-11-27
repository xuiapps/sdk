using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryManagedBuffer(Instruction.Token instruction, int offset)
    {
        var access = ManagedBuffer.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        var storage = method.Storage.Bind(access.Element);
        var address = method.Storage.Bind(access.Element.ByReference());
        if (access.Operation == "GetArrayDataReference")
        {
            PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), instruction.Code, offset);
            CheckExceptionNull(); writer.I32Const(0);
            stack.Push(new(address.TypeCode, address.Identity, true, address.Array));
        }
        else if (access.Operation == "Add")
        {
            PopAssignable(method.Storage.Bind(access.Signature.Parameters[1]), instruction.Code, offset);
            writer.LocalSet(method.CopyLength);
            PopAssignable(address, instruction.Code, offset); SaveAddress();
            writer.LocalGet(method.AddressOwner).LocalGet(method.AddressOwner).RefIsNull().If(method.Storage.AddressIndexType);
            writer.LocalGet(method.AddressSelector).LocalGet(method.CopyLength).I32Const(StorageModel.IsReference(storage) ? 1 : ElementSize(access.Element)).I32Mul().I32Add();
            writer.Else().LocalGet(method.AddressSelector).LocalGet(method.CopyLength).I32Const(ElementLeaves(storage)).I32Mul().I32Add().End();
            stack.Push(new(address.TypeCode, address.Identity, true, address.Array));
        }
        else
        {
            Pop(SignatureTypeCode.IntPtr, instruction.Code, offset); writer.LocalSet(method.CopyLength);
            PopAssignable(address, instruction.Code, offset); writer.LocalSet(method.SourceSelector).LocalSet(method.SourceOwner);
            PopAssignable(address, instruction.Code, offset); SaveAddress();
            writer.LocalGet(method.CopyLength).If();
            CheckSavedAddress(method.SourceOwner, method.SourceSelector);
            CheckSavedAddress(method.AddressOwner, method.AddressSelector);
            bool references = StorageModel.IsReference(storage) || storage.Identity is { } id && HasReferences(id);
            if (StorageModel.IsReference(storage))
            {
                MarkReferenceSlot(method.SourceOwner, method.SourceSelector);
                MarkReferenceSlot(method.AddressOwner, method.AddressSelector);
            }
            if (!references)
            {
                writer.LocalGet(method.SourceOwner).RefIsNull().LocalGet(method.AddressOwner).RefIsNull().I32And().If();
                writer.LocalGet(method.AddressSelector).LocalGet(method.SourceSelector).LocalGet(method.CopyLength)
                    .I32Const(ElementSize(access.Element)).I32Mul().MemoryCopy().Else();
            }
            CopySpanElements(access.Element, storage);
            if (!references) writer.End();
            writer.End();
        }
        return true;
    }

    private void MarkReferenceSlot(LocalIndex owner, LocalIndex selector)
    {
        writer.LocalGet(owner).RefIsNull().If().LocalGet(selector).I32Const(ReferenceSlotSelector).I32Or().LocalSet(selector).End();
    }
}
