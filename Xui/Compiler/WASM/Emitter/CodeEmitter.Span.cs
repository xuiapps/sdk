using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TrySpanAccess(Instruction.Token instruction, int offset)
    {
        var access = SpanAccess.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
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
        var receiver = Pop(instruction.Code, offset);
        if (receiver.Identity != access.Owner || !receiver.IsByReference)
            throw new NotSupportedException("Span operations require a linear-memory span address.");
        LinearAddressValue();
        if (access.Operation is "get_Length" or "get_IsEmpty")
        {
            writer.I32Const(4).I32Add().I32Load();
            if (access.Operation == "get_IsEmpty") writer.I32Eqz();
            stack.Push(new(SignatureTypeCode.Int32));
        }
        else
        {
            writer.LocalSet(Frame.AddressScratch).LocalGet(Frame.I32Scratch)
                .LocalGet(Frame.AddressScratch).I32Const(4).I32Add().I32Load().I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException);
            writer.End().LocalGet(Frame.AddressScratch).I32Load().LocalGet(Frame.I32Scratch)
                .I32Const(ElementSize(access.Owner.Arguments!.Values[0])).I32Mul().I32Add();
            var element = EnumInfo.Storage(access.Owner.Arguments.Values[0]);
            if (StorageModel.IsReference(method.Storage.Bind(element)))
                LinearReferenceAddress();
            else LinearValueAddress();
            stack.Push(new(element.Code, element.Identity, true, element.Array));
        }
        return true;
    }

    private void ReadSpanAddress(SpanAccess access, OpCode code, int offset)
    {
        var receiver = Pop(code, offset);
        if (receiver.Identity != access.Owner || !receiver.IsByReference)
            throw new NotSupportedException("Span operations require a linear-memory span address.");
        LinearAddressValue();
    }

    private void EmitSpanSlice(SpanAccess access, OpCode code, int offset)
    {
        if (access.Signature.Parameters.Length == 2)
        {
            Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(method.GcI32);
        }
        Pop(SignatureTypeCode.Int32, code, offset); writer.LocalSet(Frame.I32Scratch);
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(Frame.AddressScratch).I32Const(4).I32Add().I32Load().LocalSet(method.CopyLength);
        writer.LocalGet(Frame.I32Scratch).LocalGet(method.CopyLength).I32GtU().If();
        ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        writer.LocalGet(method.CopyLength).LocalGet(Frame.I32Scratch).I32Sub();
        if (access.Signature.Parameters.Length == 1) writer.LocalSet(method.GcI32);
        else
        {
            writer.LocalGet(method.GcI32).I32LtU().If();
            ThrowException(ExceptionKind.ArgumentOutOfRangeException); writer.End();
        }
        FrameAddress(Frame.Values[offset]);
        writer.LocalGet(Frame.AddressScratch).I32Load().LocalGet(Frame.I32Scratch)
            .I32Const(ElementSize(access.Owner.Arguments!.Values[0])).I32Mul().I32Add().I32Store();
        FrameAddress(Frame.Values[offset] + 4); writer.LocalGet(method.GcI32).I32Store();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
    }

    private void EmitSpanCopy(SpanAccess access, OpCode code, int offset)
    {
        PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), code, offset);
        writer.LocalSet(method.CopyDestinationIndex);
        ReadSpanAddress(access, code, offset);
        writer.LocalTee(method.CopySourceIndex).I32Const(4).I32Add().I32Load().LocalSet(method.CopyLength);
        writer.LocalGet(method.CopyLength).LocalGet(method.CopyDestinationIndex).I32Const(4).I32Add().I32Load().I32GtU().If();
        bool trying = access.Operation == "TryCopyTo";
        if (trying) writer.I32Const(0).LocalSet(method.GcI32);
        else ThrowException(ExceptionKind.ArgumentException);
        writer.Else();
        writer.LocalGet(method.CopySourceIndex).I32Load().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).I32Load().LocalSet(method.CopyDestinationIndex);
        var element = access.Owner.Arguments!.Values[0];
        var storage = method.Storage.Bind(element);
        bool references = StorageModel.IsReference(storage) || storage.Identity is { } id && HasReferences(id);
        if (!references)
        {
            writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.CopySourceIndex)
                .LocalGet(method.CopyLength).I32Const(ElementSize(element)).I32Mul().MemoryCopy();
        }
        else CopySpanReferences(element, storage);
        if (trying) writer.I32Const(1).LocalSet(method.GcI32);
        writer.End();
        if (trying) { writer.LocalGet(method.GcI32); stack.Push(new(SignatureTypeCode.Int32)); }
    }

    // Memmove direction, but copy references into destination-owned slots instead of copying handles.
    private void CopySpanReferences(SignatureType element, LocalVariable storage)
    {
        int size = ElementSize(element);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.CopySourceIndex).I32GtU().If();
        writer.I32Const(-size).LocalSet(Frame.I32Scratch);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().I32Const(size).I32Mul().LocalSet(Frame.AddressScratch);
        writer.LocalGet(method.CopySourceIndex).LocalGet(Frame.AddressScratch).I32Add().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(Frame.AddressScratch).I32Add().LocalSet(method.CopyDestinationIndex);
        writer.Else().I32Const(size).LocalSet(Frame.I32Scratch).End();
        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        if (StorageModel.IsReference(storage)) CopySpanLeaf(storage, 0);
        else foreach (var leaf in method.Storage.Shape(storage.Identity!.Value).Leaves) CopySpanLeaf(leaf.Storage, leaf.MemoryOffset);
        writer.LocalGet(method.CopySourceIndex).LocalGet(Frame.I32Scratch).I32Add().LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.CopyDestinationIndex).LocalGet(Frame.I32Scratch).I32Add().LocalSet(method.CopyDestinationIndex);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
    }

    private void CopySpanLeaf(LocalVariable storage, int byteOffset)
    {
        writer.LocalGet(method.CopyDestinationIndex).I32Const(byteOffset).I32Add();
        if (StorageModel.IsReference(storage)) writer.I32Load();
        writer.LocalGet(method.CopySourceIndex).I32Const(byteOffset).I32Add();
        LoadRaw(storage);
        if (StorageModel.IsReference(storage)) writer.TableSet(ReferenceTable(storage));
        else StoreRaw(storage);
    }
}
