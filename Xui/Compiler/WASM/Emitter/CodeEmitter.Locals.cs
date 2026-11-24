using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private LocalVariable GetLocal(LocalIndex index, OpCode code, int offset)
    {
        if (index.Value >= locals.Length)
            throw new InvalidProgramException($"Invalid local index {index.Value} for {code} at IL_{offset:x4}.");
        return locals[(int)index.Value];
    }

    private void LoadLocal(LocalIndex index, OpCode code, int offset)
    {
        var local = GetLocal(index, code, offset);
        if (!initializedLocals[(int)index.Value])
            throw new InvalidProgramException($"Uninitialized local {index.Value} for {code} at IL_{offset:x4}.");
        writer.LocalGet(new LocalIndex((uint)method.Signature.WasmParameterCount + index.Value));
        if (!local.IsByReference && local.Identity is { } identity && !method.Storage.IsClass(identity))
        {
            if (local.UsesGc) CloneGc(identity);
            else CopyValue(identity, offset);
        }
        if (local.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        stack.Push(new MsilStackEntry(local.StackType, local.Identity, local.IsByReference));
    }

    private void StoreLocal(LocalIndex index, OpCode code, int offset)
    {
        var local = GetLocal(index, code, offset);
        if (local.IsByReference && stack.Peek().Location is not null)
            throw new NotSupportedException("Managed-address locals cannot capture a GC field location.");
        PopAssignable(local, code, offset);
        if (local.IsByReference) writer.LocalSet(new LocalIndex((uint)method.Signature.WasmParameterCount + index.Value));
        else if (local.UsesGc && local.Identity is { } gcIdentity && !method.Storage.IsClass(gcIdentity))
            StoreGcLocal(new LocalIndex((uint)method.Signature.WasmParameterCount + index.Value), gcIdentity);
        else if (!local.UsesGc && local.Identity is { } identity)
        {
            var frame = Frame;
            writer.LocalSet(frame.AddressScratch);
            writer.LocalGet(new LocalIndex((uint)method.Signature.WasmParameterCount + index.Value)).LocalGet(frame.AddressScratch);
            CopyStruct(identity);
        }
        else
        {
            Narrow(local);
            writer.LocalSet(new LocalIndex((uint)method.Signature.WasmParameterCount + index.Value));
        }
        initializedLocals[(int)index.Value] = true;
    }
}
