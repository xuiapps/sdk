using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private MemoryFrame Frame => method.Frame ?? throw new NotSupportedException("This method has no linear-memory struct frame.");

    private void EnterFrame()
    {
        if (method.Frame is not { } frame) return;
        var pointer = Runtime.StackPointer!.Value;
        writer.GlobalGet(pointer).LocalTee(frame.Base).I32Const(MemoryFrame.Capacity - frame.Size)
            .I32GtU().If().Unreachable().End();
        writer.LocalGet(frame.Base).I32Const(frame.Size).I32Add().GlobalSet(pointer);
        foreach (var (index, address) in frame.ScalarLocals)
            if (initLocals) { FrameAddress(address); writer.I32Const(0).I32Const(MemoryFrame.ScalarSize(locals[index])).MemoryFill(); }
        foreach (var (index, address) in frame.ScalarArguments)
        {
            FrameAddress(address); writer.LocalGet(new((uint)index)); StoreRaw(parameters[index]);
        }
        for (int i = 0; i < frame.Locals.Length; i++)
        {
            if (frame.Locals[i] < 0) continue;
            FrameAddress(frame.Locals[i]);
            writer.LocalSet(new LocalIndex((uint)(method.Signature.WasmParameterCount + i)));
            if (initLocals)
                writer.LocalGet(new LocalIndex((uint)(method.Signature.WasmParameterCount + i))).I32Const(0)
                    .I32Const(frame.ValueSize(locals[i].Identity!.Value)).MemoryFill();
        }
        EnterReferenceFrame();
        foreach (var (index, displacement) in frame.ReferenceArguments)
        {
            FrameAddress(displacement);
            writer.LocalGet(new((uint)index));
            StoreReference(parameters[index]);
        }
        // The incoming address names an evaluation-stack snapshot. Copy it again so
        // aliases into the parameter remain stable when starg replaces its value.
        for (int i = 0; i < frame.Arguments.Length; i++)
        {
            if (frame.Arguments[i] < 0) continue;
            FrameAddress(frame.Arguments[i]);
            writer.LocalGet(new((uint)i));
            CopyStruct(parameters[i].Identity!.Value);
            FrameAddress(frame.Arguments[i]);
            writer.LocalSet(new((uint)i));
        }
    }

    private void LeaveFrame()
    {
        if (method.Frame is { } frame) { LeaveReferenceFrame(); writer.LocalGet(frame.Base).GlobalSet(Runtime.StackPointer!.Value); }
    }

    private void FrameAddress(int displacement) => writer.LocalGet(Frame.Base).I32Const(displacement).I32Add();

    // A value load snapshots bytes so subsequent mutation of its source cannot change the loaded value.
    private void CopyValue(TypeIdentity identity, int offset)
    {
        var frame = Frame;
        writer.LocalSet(frame.AddressScratch);
        FrameAddress(frame.Values[offset]);
        writer.LocalGet(frame.AddressScratch);
        CopyStruct(identity);
        FrameAddress(frame.Values[offset]);
    }

    private void LoadLocalAddress(ushort index, OpCode code, int offset)
    {
        var local = GetLocal(new LocalIndex(index), code, offset);
        if (TryVariableScalarAddress(index, false, local)) return;
        if (TryVariableReferenceAddress(index, false, local)) return;
        if (local.Identity is null || method.Storage.IsClass(local.Identity.Value)) throw new NotSupportedException("Only struct local addresses are implemented.");
        if (!initLocals) throw new NotSupportedException("Addressable struct locals currently require initlocals.");
        writer.LocalGet(new LocalIndex((uint)method.Signature.WasmParameterCount + index));
        stack.Push(new MsilStackEntry(local.TypeCode, local.Identity, isByReference: true)
        { Location = local.UsesGc ? new(method.Storage.Shape(local.Identity!.Value), 0) : null });
    }

    private (LocalVariable Type, TypeIdentity Owner, int Offset) Field(int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        var resolvedField = MemberResolver.Field(source, handle, method.Identity.DeclaringType, method.Identity.Arguments);
        var field = resolvedField.DeclaringType.Module.GetField(resolvedField.Handle);
        if ((field.Attributes & FieldAttributes.Static) != 0) throw new NotSupportedException("Static field storage is not implemented.");
        var owner = resolvedField.DeclaringType;
        var layout = Frame.Layout(owner);
        var type = SignatureReader.Field(owner.Module, field.Signature, owner.Arguments);
        return (method.Storage.Bind(type), owner, layout.Fields[resolvedField.Handle].Offset);
    }

    private void PopOwner(TypeIdentity owner, bool requireAddress, OpCode code, int offset)
    {
        var instance = Pop(code, offset);
        if (instance.TypeCode != SignatureTypeCode.TypeHandle || instance.Identity != owner || (requireAddress && !instance.IsByReference))
            throw new InvalidProgramException($"Expected {(requireAddress ? "address of " : "")}{owner} for {code} at IL_{offset:x4}.");
    }

    private TypeIdentity ObjectType(int token)
    {
        var identity = SignatureReader.Resolve(source, MetadataTokens.EntityHandle(token), method.Identity.DeclaringType.Arguments, method.Identity.Arguments).Identity
            ?? throw new NotSupportedException("Primitive generic object-copy instructions are not implemented.");
        _ = Frame.Layout(identity);
        return identity;
    }

    private void LoadField(LocalVariable type, int offset)
    {
        if (StorageModel.IsReference(type)) LoadReference(type);
        else if (type.Identity is { } identity) CopyValue(identity, offset);
        else switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte: writer.I32Load8S(); break;
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Boolean: writer.I32Load8U(); break;
            case SignatureTypeCode.Int16: writer.I32Load16S(); break;
            case SignatureTypeCode.Char:
            case SignatureTypeCode.UInt16: writer.I32Load16U(); break;
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32:
            case SignatureTypeCode.IntPtr:
            case SignatureTypeCode.UIntPtr: writer.I32Load(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.I64Load(); break;
            case SignatureTypeCode.Single: writer.F32Load().F64PromoteF32(); break;
            case SignatureTypeCode.Double: writer.F64Load(); break;
            default: throw new NotSupportedException($"Field load for {type} is not implemented.");
        }
        stack.Push(new(type.StackType, type.Identity, array: type.Array));
    }

    private LocalIndex FieldScratch(LocalVariable type) => StorageModel.IsReference(type) ? GcScratch(type) : type.StackType switch
    {
        SignatureTypeCode.Int64 => Frame.I64Scratch,
        SignatureTypeCode.Double => Frame.F64Scratch,
        _ => Frame.I32Scratch
    };

    private void StoreField(LocalVariable type)
    {
        writer.LocalGet(FieldScratch(type));
        if (StorageModel.IsReference(type)) StoreReference(type);
        else if (type.Identity is { } identity) CopyStruct(identity);
        else switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte:
            case SignatureTypeCode.Byte:
            case SignatureTypeCode.Boolean: writer.I32Store8(); break;
            case SignatureTypeCode.Int16:
            case SignatureTypeCode.Char:
            case SignatureTypeCode.UInt16: writer.I32Store16(); break;
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32:
            case SignatureTypeCode.IntPtr:
            case SignatureTypeCode.UIntPtr: writer.I32Store(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.I64Store(); break;
            case SignatureTypeCode.Single: writer.F32DemoteF64().F32Store(); break;
            case SignatureTypeCode.Double: writer.F64Store(); break;
            default: throw new NotSupportedException($"Field store for {type} is not implemented.");
        }
    }
}
