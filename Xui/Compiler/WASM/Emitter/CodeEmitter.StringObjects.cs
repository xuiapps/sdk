using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitStringObject()
    {
        var runtime = method.Storage.StringObjects;
        var shape = runtime.Shape!;
        var descriptor = method.Storage.Classes.Resolve(shape.Identity).Descriptor;
        writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If(method.Storage.ExceptionPayloadBlock)
            .RefNull(ValueTypeCode.EqRef).Else();
        writer.GlobalGet(descriptor).LocalGet(method.GcExtern).Call(runtime.Lookup).LocalTee(method.GcOwner).RefIsNull()
            .If(method.Storage.ExceptionPayloadBlock);
        writer.GlobalGet(descriptor).LocalGet(method.GcExtern)
            .GlobalGet(descriptor).LocalGet(method.GcExtern).StructNew(shape.Type).Call(runtime.Register)
            .Else().LocalGet(method.GcOwner).End().End();
    }

    private void EmitStringCast(OpCode code, int offset, bool throwing)
    {
        var value = Pop(code, offset);
        if (!value.IsByReference && value.TypeCode == SignatureTypeCode.String) { stack.Push(value); return; }
        if (!IsObjectReference(value)) throw new InvalidProgramException("String casts require an object reference.");
        var shape = method.Storage.StringObjects.Shape!;
        writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefIsNull().If();
        writer.RefNull(ValueTypeCode.ExternRef).LocalSet(method.GcExtern).Else();
        writer.LocalGet(method.GcOwner).RefTest(shape.Type).If();
        writer.LocalGet(method.GcOwner).RefCast(shape.Type).StructGet(shape.Type, new(0))
            .GlobalGet(method.Storage.Classes.Resolve(shape.Identity).Descriptor).RefEq().If();
        writer.LocalGet(method.GcOwner).RefCast(shape.Type).StructGet(shape.Type, new(1)).LocalSet(method.GcExtern);
        writer.Else(); StringCastMiss(throwing); writer.End().Else(); StringCastMiss(throwing); writer.End().End().LocalGet(method.GcExtern);
        stack.Push(new(SignatureTypeCode.String));

    }

    private void StringCastMiss(bool throwing)
    {
        if (throwing) ThrowException(ExceptionKind.InvalidCastException);
        else writer.RefNull(ValueTypeCode.ExternRef).LocalSet(method.GcExtern);
    }
}
