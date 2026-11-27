using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TrySpanAccess(Instruction.Token instruction, int offset)
    {
        var access = method.SpanCalls.GetValueOrDefault(offset) ?? SpanAccess.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        if (access.Operation == "ToArray") { EmitSpanToArray(access, instruction.Code, offset); return true; }
        if (access.Operation is "Fill" or "Clear") { EmitSpanFill(access, instruction.Code, offset); return true; }
        if (access.Operation == "FromPointer") { EmitPointerSpan(access, instruction.Code, offset); return true; }
        if (access.Operation == "FromArray") { EmitArraySpan(access, instruction.Code, offset); return true; }
        if (access.Operation == "FromReference") { EmitReferenceSpan(access, instruction.Code, offset); return true; }
        if (access.Operation == "ToString") { EmitSpanString(access,instruction.Code,offset); return true; }
        if (access.Operation == "op_Implicit")
        {
            PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), instruction.Code, offset);
            CopyValue(access.Signature.Result.Identity!.Value, offset);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Signature.Result.Identity));
            return true;
        }
        if (access.Operation == "Slice") { EmitSpanSlice(access, instruction.Code, offset); return true; }
        if (access.Operation is "CopyTo" or "TryCopyTo") { EmitSpanCopy(access, instruction.Code, offset); return true; }
        if (access.Operation == "get_Item")
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(Frame.I32Scratch);
        }
        ReadSpanAddress(access, instruction.Code, offset);
        if (access.Operation is "get_Length" or "get_IsEmpty")
        {
            writer.I32Const(8).I32Add().I32Load();
            if (access.Operation == "get_IsEmpty") writer.I32Eqz();
            stack.Push(new(SignatureTypeCode.Int32));
        }
        else
        {
            writer.LocalSet(Frame.AddressScratch).LocalGet(Frame.I32Scratch)
                .LocalGet(Frame.AddressScratch).I32Const(8).I32Add().I32Load().I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException);
            writer.End();
            ReadSpanBacking(Frame.AddressScratch, method.AddressOwner, method.AddressSelector);
            var element = access.Owner.Arguments!.Values[0];
            var storage = method.Storage.Bind(element);
            ProjectSpanElement(method.AddressOwner, method.AddressSelector, Frame.I32Scratch, element, storage);
            stack.Push(new(storage.TypeCode, storage.Identity, true, storage.Array));
        }
        return true;
    }

    private void ReadSpanAddress(SpanAccess access, OpCode code, int offset)
    {
        var receiver = Pop(code, offset);
        if (receiver.Identity != access.Owner || !receiver.IsByReference)
            throw new NotSupportedException("Span operations require the declared span address.");
        LinearAddressValue();
    }

    private void ReadSpanBacking(LocalIndex span, LocalIndex owner, LocalIndex selector)
    {
        writer.LocalGet(span).I32Load().TableGet(Runtime.ObjectReferences!.Value).LocalSet(owner);
        writer.LocalGet(span).I32Const(4).I32Add().I32Load().LocalSet(selector);
    }

    private int ElementLeaves(LocalVariable element) => element.Identity is { IsValueType: true } identity
        ? Math.Max(1, method.Storage.Shape(identity).Leaves.Count) : 1;

    // Span selectors are byte addresses for linear backing and field indices for GC backing.
    // For a reference slot borrowed directly (Span(ref string/class)), a null owner
    // plus the high selector bit denotes a table index, rather than a byte address
    // containing that index. This preserves aliases across returns without a wrapper allocation.
    private const int ReferenceSlotSelector = int.MinValue;

    private void SpanSelector(LocalIndex owner, LocalIndex selector, LocalIndex index, SignatureType element)
    {
        writer.LocalGet(owner).RefIsNull().If(method.Storage.AddressIndexType);
        bool reference = StorageModel.IsReference(method.Storage.Bind(element));
        if (reference) writer.LocalGet(selector).I32Const(0).I32LtS().If(method.Storage.AddressIndexType)
            .LocalGet(selector).LocalGet(index).I32Add().Else();
        writer.LocalGet(selector).LocalGet(index).I32Const(ElementSize(element)).I32Mul().I32Add();
        if (reference) writer.End();
        writer.Else().LocalGet(selector).LocalGet(index).I32Const(ElementLeaves(method.Storage.Bind(element))).I32Mul().I32Add().End();
    }

    private void ProjectSpanElement(LocalIndex owner, LocalIndex selector, LocalIndex index, SignatureType element,
        LocalVariable leaf, int byteOffset = 0, int leafOffset = 0)
    {
        writer.LocalGet(owner).LocalGet(owner).RefIsNull().If(method.Storage.AddressIndexType);
        bool referenceElement = StorageModel.IsReference(method.Storage.Bind(element));
        if (referenceElement) writer.LocalGet(selector).I32Const(0).I32LtS().If(method.Storage.AddressIndexType)
            .LocalGet(selector).I32Const(int.MaxValue).I32And().LocalGet(index).I32Add().Else();
        writer.LocalGet(selector).LocalGet(index).I32Const(ElementSize(element)).I32Mul().I32Add().I32Const(byteOffset).I32Add();
        if (StorageModel.IsReference(leaf)) writer.I32Load();
        if (referenceElement) writer.End();
        writer.Else().LocalGet(selector).LocalGet(index).I32Const(ElementLeaves(method.Storage.Bind(element))).I32Mul().I32Add()
            .I32Const(leafOffset).I32Add().End();
    }

    private void EmitSpanSlice(SpanAccess access, OpCode code, int offset)
    {
        if (access.Signature.Parameters.Length == 2)
        {
            Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(method.GcI32);
        }
        Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(Frame.I32Scratch);
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(Frame.AddressScratch).I32Const(8).I32Add().I32Load().LocalSet(method.CopyLength);
        writer.LocalGet(Frame.I32Scratch).LocalGet(method.CopyLength).I32GtU().If();
        ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        writer.LocalGet(method.CopyLength).LocalGet(Frame.I32Scratch).I32Sub();
        if (access.Signature.Parameters.Length == 1) writer.LocalSet(method.GcI32);
        else
        {
            writer.LocalGet(method.GcI32).I32LtU().If();
            ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        }
        ReadSpanBacking(Frame.AddressScratch, method.AddressOwner, method.AddressSelector);
        FrameAddress(Frame.Values[offset]); writer.I32Load().LocalGet(method.AddressOwner).TableSet(Runtime.ObjectReferences!.Value);
        FrameAddress(Frame.Values[offset] + 4);
        SpanSelector(method.AddressOwner, method.AddressSelector, Frame.I32Scratch, access.Owner.Arguments!.Values[0]);
        writer.I32Store();
        FrameAddress(Frame.Values[offset] + 8); writer.LocalGet(method.GcI32).I32Store();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
    }

    private void EmitSpanCopy(SpanAccess access, OpCode code, int offset)
    {
        PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), code, offset);
        writer.LocalSet(method.CopyDestinationIndex);
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(method.CopySourceIndex).I32Const(8).I32Add().I32Load().LocalSet(method.CopyLength);
        writer.LocalGet(method.CopyLength).LocalGet(method.CopyDestinationIndex).I32Const(8).I32Add().I32Load().I32GtU().If();
        if (access.Operation == "TryCopyTo") writer.I32Const(0).LocalSet(method.GcI32);
        else ThrowException(ExceptionKind.ArgumentException);
        writer.Else();
        ReadSpanBacking(method.CopySourceIndex, method.SourceOwner, method.SourceSelector);
        ReadSpanBacking(method.CopyDestinationIndex, method.AddressOwner, method.AddressSelector);
        var element = access.Owner.Arguments!.Values[0];
        var storage = method.Storage.Bind(element);
        bool references = StorageModel.IsReference(storage) || storage.Identity is { } id && HasReferences(id);
        if (!references)
        {
            writer.LocalGet(method.SourceOwner).RefIsNull().LocalGet(method.AddressOwner).RefIsNull().I32And().If();
            writer.LocalGet(method.AddressSelector).LocalGet(method.SourceSelector)
                .LocalGet(method.CopyLength).I32Const(ElementSize(element)).I32Mul().MemoryCopy();
            writer.Else();
        }
        CopySpanElements(element, storage);
        if (!references) writer.End();
        if (access.Operation == "TryCopyTo") writer.I32Const(1).LocalSet(method.GcI32);
        writer.End();
        if (access.Operation == "TryCopyTo") { writer.LocalGet(method.GcI32); stack.Push(new(SignatureTypeCode.Int32)); }

    }

    private void CopySpanElements(SignatureType element, LocalVariable storage)
    {
        // Walk backwards only for overlapping destinations in the same backing owner.
        writer.LocalGet(method.SourceOwner).LocalGet(method.AddressOwner).RefEq()
            .LocalGet(method.AddressSelector).LocalGet(method.SourceSelector).I32GtU().I32And().If();
        writer.I32Const(-1).LocalSet(Frame.I32Scratch);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(Frame.AddressScratch);
        writer.Else().I32Const(1).LocalSet(Frame.I32Scratch).I32Const(0).LocalSet(Frame.AddressScratch).End();
        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        if (storage.Identity is { IsValueType: true } identity)
        {
            var leaves = method.Storage.Shape(identity).Leaves;
            for (int i = 0; i < leaves.Count; i++) CopySpanLeaf(element, leaves[i].Storage, leaves[i].MemoryOffset, i);
        }
        else CopySpanLeaf(element, storage, 0, 0);
        writer.LocalGet(Frame.AddressScratch).LocalGet(Frame.I32Scratch).I32Add().LocalSet(Frame.AddressScratch);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
    }

    private void CopySpanLeaf(SignatureType element, LocalVariable leaf, int byteOffset, int leafOffset)
    {
        ProjectSpanElement(method.AddressOwner, method.AddressSelector, Frame.AddressScratch, element, leaf, byteOffset, leafOffset);
        ProjectSpanElement(method.SourceOwner, method.SourceSelector, Frame.AddressScratch, element, leaf, byteOffset, leafOffset);
        writer.Call(method.Storage.ReferenceAccessor(leaf, false)).Call(method.Storage.ReferenceAccessor(leaf, true));
    }
}
