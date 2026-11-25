using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private TableIndex ReferenceTable(LocalVariable type) => type.TypeCode == SignatureTypeCode.String
        ? Runtime.StringReferences!.Value : Runtime.ObjectReferences!.Value;

    private void EnterReferenceFrame()
    {
        var frame = Frame;
        if (frame.References.Count == 0) return;
        var pointer = Runtime.ReferenceStackPointer!.Value;
        writer.GlobalGet(pointer).LocalTee(frame.ReferenceBase).I32Const((int)StorageModel.ReferenceCapacity - frame.References.Count)
            .I32GtU().If().Unreachable().End();
        writer.LocalGet(frame.ReferenceBase).I32Const(frame.References.Count).I32Add().GlobalSet(pointer);
        for (int i = 0; i < frame.References.Count; i++)
        {
            FrameAddress(frame.References[i].Offset);
            writer.LocalGet(frame.ReferenceBase).I32Const(i).I32Add().I32Store();
        }
    }

    private void LeaveReferenceFrame()
    {
        var frame = Frame;
        if (frame.References.Count == 0) return;
        writer.LocalGet(frame.ReferenceBase).RefNull(ValueTypeCode.EqRef).I32Const(frame.References.Count).TableFill(Runtime.ObjectReferences!.Value);
        writer.LocalGet(frame.ReferenceBase).RefNull(ValueTypeCode.ExternRef).I32Const(frame.References.Count).TableFill(Runtime.StringReferences!.Value);
        writer.LocalGet(frame.ReferenceBase).GlobalSet(Runtime.ReferenceStackPointer!.Value);
    }

    // Address -> slot index -> GC-visible reference. Slot identity belongs to the destination storage.
    private void LoadReference(LocalVariable type) => writer.I32Load().TableGet(ReferenceTable(type));

    private void StoreReference(LocalVariable type)
    {
        var scratch = type.TypeCode == SignatureTypeCode.String ? method.GcExtern : Frame.ReferenceValue;
        writer.LocalSet(scratch).I32Load().LocalGet(scratch).TableSet(ReferenceTable(type));
    }

    private bool HasReferences(TypeIdentity type) => method.Storage.HasStackReferences(type);

    // Destination and source addresses on the WASM stack. Never copy reference-slot indices.
    private void CopyStruct(TypeIdentity type)
    {
        if (!HasReferences(type)) { writer.I32Const(Frame.ValueSize(type)).MemoryCopy(); return; }
        writer.LocalSet(Frame.CopySource).LocalSet(Frame.CopyDestination);
        foreach (var leaf in method.Storage.Shape(type).Leaves)
        {
            writer.LocalGet(Frame.CopyDestination).I32Const(leaf.MemoryOffset).I32Add();
            writer.LocalGet(Frame.CopySource).I32Const(leaf.MemoryOffset).I32Add();
            LoadRaw(leaf.Storage);
            StoreRaw(leaf.Storage);
        }
    }

    private void ClearStruct(TypeIdentity type)
    {
        if (!HasReferences(type)) { writer.I32Const(0).I32Const(Frame.ValueSize(type)).MemoryFill(); return; }
        writer.LocalSet(Frame.CopyDestination);
        foreach (var leaf in method.Storage.Shape(type).Leaves)
        {
            writer.LocalGet(Frame.CopyDestination).I32Const(leaf.MemoryOffset).I32Add();
            DefaultValue(leaf.Storage);
            StoreRaw(leaf.Storage);
        }
    }
}
