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
    private bool TryObjectText(Instruction.Token instruction, int offset)
    {
        if (method.ConstrainedCalls.ContainsKey(offset) || !ObjectText.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        bool virtualCall = instruction.Code == OpCode.Callvirt;
        var value = Pop(instruction.Code, offset);
        var storage = method.Storage;
        if (value.TypeCode == SignatureTypeCode.String)
        {
            writer.LocalTee(method.GcExtern).RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
            if (virtualCall) writer.LocalGet(method.GcExtern); else EmitString("System.String");
        }
        else
        {
            if (!IsObjectReference(value)) throw new NotSupportedException("Object.ToString requires a managed object receiver.");
            writer.LocalTee(method.GcSource).RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
            writer.Block(storage.EmptyBlock);
            if (storage.StringObjects.Shape is { } text)
            {
                writer.LocalGet(method.GcSource).RefTest(text.Type).If();
                writer.LocalGet(method.GcSource).RefCast(text.Type).StructGet(text.Type, new(0))
                    .GlobalGet(storage.Classes.Resolve(text.Identity).Descriptor).RefEq().If();
                if (virtualCall) writer.LocalGet(method.GcSource).RefCast(text.Type).StructGet(text.Type, new(1));
                else EmitString("System.String");
                writer.LocalSet(method.GcExtern).Br(new(2)).End().End();
            }
            foreach (var (box, shape) in storage.Boxes)
            {
                writer.LocalGet(method.GcSource).RefTest(shape.Type).If();
                writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new(0))
                    .GlobalGet(storage.Classes.Resolve(box.Identity).Descriptor).RefEq().If();
                if (!virtualCall) EmitString(ObjectText.TypeName(box.Identity));
                else if (EnumInfo.UnderlyingType(box.Identity) is not null)
                {
                    writer.LocalGet(method.GcSource).RefCast(shape.Type).StructGet(shape.Type, new(1));
                    FormatEnum(box.Identity, storage.Bind(box.Storage));
                }
                else FormatBoxedScalar();
                writer.LocalSet(method.GcExtern).Br(new(2)).End().End();
            }
            if (storage.ExceptionsEnabled)
            {
                writer.LocalGet(method.GcSource).RefTest(storage.ExceptionType).If();
                if (virtualCall) { writer.LocalGet(method.GcSource); EmitExceptionText(); }
                else EmitExceptionTypeName();
                writer.LocalSet(method.GcExtern).Br(new(1)).End();
            }
            foreach (var target in storage.ObjectTextTargets)
            {
                var shape = storage.Shape(target.Type);
                writer.LocalGet(method.GcSource).RefTest(shape.Type).If();
                writer.LocalGet(method.GcSource).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                    .GlobalGet(storage.Classes.Resolve(target.Type).Descriptor).RefEq().If();
                if (virtualCall && target.Override is { } implementation)
                    writer.LocalGet(method.GcSource).Call(methods[implementation].Function);
                else EmitString(ObjectText.TypeName(target.Type));
                writer.LocalSet(method.GcExtern).Br(new(2)).End().End();
            }
            foreach (var array in storage.ObjectTextArrays)
            {
                writer.LocalGet(method.GcSource).ExternConvertAny().I32Const((int)array.Type.Value).Call(storage.ArrayIdentity.Test).If();
                EmitString(ObjectText.TypeName(new SignatureType(array.Signature.IsVector ? SignatureTypeCode.SZArray : SignatureTypeCode.Array, Array: array.Signature)));
                writer.LocalSet(method.GcExtern).Br(new(1)).End();
            }
            // Other compiler-owned carriers (for example exceptions or Type descriptors) need their own override surface.
            ThrowException(ExceptionKind.NotSupportedException);
            writer.End().LocalGet(method.GcExtern);
        }
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }

    private void FormatBoxedScalar()
    {
        var formatting = method.Storage.Formatting;
        writer.I32Const(1).Call(formatting.Start).LocalTee(method.FormatState).I32Const(0)
            .LocalGet(method.GcSource).Call(formatting.BoxValue).Call(formatting.Set);
        EmitString("{0}");
        writer.LocalGet(method.FormatState).Call(formatting.Finish).LocalSet(method.GcI32).LocalSet(method.GcExtern);
        writer.LocalGet(method.GcI32).If(); ThrowException(ExceptionKind.NotSupportedException); writer.End().LocalGet(method.GcExtern);
    }
}
