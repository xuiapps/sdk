using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryObjectHash(Instruction.Token instruction, int offset)
    {
        if (!ObjectHash.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        if (method.ConstrainedCalls.TryGetValue(offset, out var constrained))
        {
            if (constrained.ReferenceReceiver is null) return false;
            EmitIndirect(OpCode.Ldind_ref, offset, store: false);
        }
        bool virtualCall = instruction.Code == OpCode.Callvirt;
        var value = Pop(instruction.Code, offset);
        var storage = method.Storage;
        if (value.TypeCode == SignatureTypeCode.String)
        {
            if (!virtualCall) throw new NotSupportedException("Nonvirtual Object.GetHashCode on an unwrapped string is not implemented.");
            writer.LocalTee(method.GcExtern).RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
            writer.LocalGet(method.GcExtern).Call(storage.Primitives.StringHash);
        }
        else
        {
            if (!IsObjectReference(value)) throw new NotSupportedException("Object.GetHashCode requires a managed object receiver.");
            writer.LocalTee(method.GcSource).RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
            writer.Block(storage.EmptyBlock);
            if (virtualCall)
            {
                if (storage.StringObjects.Shape is { } text)
                {
                    writer.LocalGet(method.GcSource).RefTest(text.Type).If();
                    writer.LocalGet(method.GcSource).RefCast(text.Type).StructGet(text.Type, new(0))
                        .GlobalGet(storage.Classes.Resolve(text.Identity).Descriptor).RefEq().If();
                    writer.LocalGet(method.GcSource).RefCast(text.Type).StructGet(text.Type, new(1))
                        .Call(storage.Primitives.StringHash).LocalSet(method.GcI32).Br(new(2)).End().End();
                }
                foreach (var (box, shape) in storage.Boxes)
                {
                    writer.LocalGet(method.GcSource).RefTest(shape.Type).If();
                    writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new(0))
                        .GlobalGet(storage.Classes.Resolve(box.Identity).Descriptor).RefEq().If();
                    var payload = storage.Bind(box.Storage);
                    writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new(1));
                    if (payload.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
                    var scratch = GcScratch(payload);
                    writer.LocalSet(scratch);
                    EmitPrimitiveHash(payload, scratch);
                    writer.LocalSet(method.GcI32).Br(new(2)).End().End();
                }
                foreach (var (receiver, implementation) in storage.ObjectHashTargets)
                {
                    var shape = storage.Shape(receiver);
                    writer.LocalGet(method.GcSource).RefTest(shape.Type).If();
                    writer.LocalGet(method.GcSource).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                        .GlobalGet(storage.Classes.Resolve(receiver).Descriptor).RefEq().If();
                    writer.LocalGet(method.GcSource).Call(methods[implementation].Function)
                        .LocalSet(method.GcI32).Br(new(2)).End().End();
                }
            }
            writer.LocalGet(method.GcSource).ExternConvertAny().Call(storage.Primitives.ObjectHash).LocalSet(method.GcI32);
            writer.End().LocalGet(method.GcI32);
        }
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
