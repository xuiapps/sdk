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
        writer.LocalGet(new LocalIndex((uint)parameters.Length + index.Value));
        if (local.Identity is { } identity && !method.Storage.IsClass(identity))
        {
            if (local.UsesGc) CloneGc(identity);
            else CopyValue(identity, offset);
        }
        if (local.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        stack.Push(new MsilStackEntry(local.StackType, local.Identity));
    }

    private void StoreLocal(LocalIndex index, OpCode code, int offset)
    {
        var local = GetLocal(index, code, offset);
        PopAssignable(local, code, offset);
        if (local.UsesGc && local.Identity is { } gcIdentity && !method.Storage.IsClass(gcIdentity))
            StoreGcLocal(new LocalIndex((uint)parameters.Length + index.Value), gcIdentity);
        else if (!local.UsesGc && local.Identity is { } identity)
        {
            var frame = Frame;
            writer.LocalSet(frame.AddressScratch);
            writer.LocalGet(new LocalIndex((uint)parameters.Length + index.Value)).LocalGet(frame.AddressScratch)
                .I32Const(frame.ValueSize(identity)).MemoryCopy();
        }
        else
        {
            Narrow(local);
            writer.LocalSet(new LocalIndex((uint)parameters.Length + index.Value));
        }
        initializedLocals[(int)index.Value] = true;
    }
}
