using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryInterpolation(Instruction.Token instruction, int offset)
    {
        var call = method.InterpolationCalls.GetValueOrDefault(offset) ?? InterpolatedStringHandler.Read(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (call is null) return false;
        var helpers = method.Storage.Interpolation;
        if (call.Operation == "Create")
        {
            var handler = Pop(instruction.Code, offset);
            if (handler.Identity != call.Owner || !handler.IsByReference)
                throw new InvalidProgramException("String.Create requires a handler address.");
            LinearAddressValue(); writer.LocalSet(Frame.AddressScratch);
            Pop(instruction.Code, offset); writer.Drop(); // Like CoreLib, Create uses the provider already captured at construction.
            writer.LocalGet(Frame.AddressScratch).I32Const(1).Call(helpers.Finish);
            stack.Push(new(SignatureTypeCode.String));
            return true;
        }
        bool construct = call.Operation == ".ctor";
        if (construct)
        {
            if (call.HasProvider)
            {
                Pop(instruction.Code, offset);
                writer.LocalTee(method.GcOwner).RefIsNull().I32Eqz().If();
                writer.LocalGet(method.GcOwner).GlobalGet(method.Storage.InvariantCultures[InvariantCulture.Resolve(source)]).RefEq().I32Eqz().If();
                ThrowException(Xui.Compiler.XuiNet.DotNet.Exceptions.ExceptionKind.NotSupportedException);
                writer.End().End();
            }
            // These are capacity hints, not observable text. Allocate lazily on the first append.
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.Drop();
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.Drop();
        }
        else if (call.IsFormattedInteger)
        {
            Pop(SignatureTypeCode.String, instruction.Code, offset); writer.LocalSet(method.GcExtern);
            var storage = method.Storage.Bind(call.Signature.Parameters[0]);
            PopAssignable(storage, instruction.Code, offset); Narrow(storage);
            writer.LocalSet(storage.StackType == SignatureTypeCode.Int64 ? method.GcI64 : method.GcI32);
        }
        else if (call.IsFloatingPoint)
        {
            if (call.Signature.Parameters.Length == 2) Pop(SignatureTypeCode.String, instruction.Code, offset);
            else writer.RefNull(ValueTypeCode.ExternRef);
            writer.LocalSet(method.GcExtern);
            PopAssignable(method.Storage.Bind(call.Signature.Parameters[0]), instruction.Code, offset);
            if (call.IsSingle) writer.F32DemoteF64().F64PromoteF32();
            writer.LocalSet(method.GcF64);
        }
        else if (call.Signature.Parameters.Length == 1)
        {
            var type = method.Storage.Bind(call.Signature.Parameters[0]).TypeCode;
            PopAssignable(method.Storage.Bind(call.Signature.Parameters[0]), instruction.Code, offset);
            if (type == SignatureTypeCode.String) writer.LocalSet(method.GcExtern);
            else if (type is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64) writer.LocalSet(method.GcI64);
            else writer.LocalSet(method.GcI32);
        }
        if (instruction.Code == OpCode.Newobj)
        {
            FrameAddress(Frame.Values[offset]);
        }
        else
        {
            var receiver = Pop(instruction.Code, offset);
            if (receiver.Identity != call.Owner || !receiver.IsByReference)
                throw new NotSupportedException("Interpolation requires a stack-backed handler address.");
            LinearAddressValue();
        }
        writer.LocalSet(Frame.AddressScratch);
        if (construct)
        {
            writer.LocalGet(Frame.AddressScratch).I32Load().RefNull(ValueTypeCode.EqRef).TableSet(Runtime.ObjectReferences!.Value);
            writer.LocalGet(Frame.AddressScratch).I32Const(4).I32Add().I32Const(0).I32Store();
            if (instruction.Code == OpCode.Newobj)
            {
                writer.LocalGet(Frame.AddressScratch);
                stack.Push(new(SignatureTypeCode.TypeHandle, call.Owner));
            }
        }
        else if (call.Operation is "ToString" or "ToStringAndClear")
        {
            writer.LocalGet(Frame.AddressScratch).I32Const(call.Operation == "ToStringAndClear" ? 1 : 0).Call(helpers.Finish);
            stack.Push(new(SignatureTypeCode.String));
        }
        else
        {
            if (call.IsEnum)
            {
                var storage = method.Storage.Bind(call.Signature.Parameters[0]);
                writer.LocalGet(storage.TypeCode is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 ? method.GcI64 : method.GcI32);
                FormatEnum(call.Signature.Parameters[0].Identity!.Value, storage);
                writer.LocalSet(method.GcExtern).LocalGet(Frame.AddressScratch).LocalGet(method.GcExtern).Call(helpers.AppendString);
                return true;
            }
            if (call.IsFormattedInteger)
            {
                FormatInterpolatedInteger(call.Signature.Parameters[0].Code);
                writer.LocalSet(method.GcExtern).LocalGet(Frame.AddressScratch).LocalGet(method.GcExtern).Call(helpers.AppendString);
                return true;
            }
            if (call.IsFloatingPoint)
            {
                writer.LocalGet(method.GcF64).LocalGet(method.GcExtern);
                FormatFloatingPoint(call.IsSingle);
                writer.LocalSet(method.GcExtern).LocalGet(Frame.AddressScratch).LocalGet(method.GcExtern).Call(helpers.AppendString);
                return true;
            }
            var type = call.Signature.Parameters[0].Code;
            writer.LocalGet(Frame.AddressScratch);
            if (type == SignatureTypeCode.String) writer.LocalGet(method.GcExtern).Call(helpers.AppendString);
            else if (type == SignatureTypeCode.Char) writer.LocalGet(method.GcI32).Call(helpers.AppendChar);
            else if (type == SignatureTypeCode.Boolean)
            {
                writer.LocalGet(method.GcI32).If();
                EmitString("True"); writer.LocalSet(method.GcExtern);
                writer.Else(); EmitString("False"); writer.LocalSet(method.GcExtern);
                writer.End().LocalGet(method.GcExtern).Call(helpers.AppendString);
            }
            else
            {
                bool signed = type is SignatureTypeCode.SByte or SignatureTypeCode.Int16 or SignatureTypeCode.Int32 or SignatureTypeCode.Int64;
                if (type is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64) writer.LocalGet(method.GcI64);
                else
                {
                    writer.LocalGet(method.GcI32);
                    if (signed) writer.I64ExtendI32S(); else writer.I64ExtendI32U();
                }
                writer.I32Const(signed ? 1 : 0).Call(helpers.AppendInteger);
            }
        }
        return true;
    }
}
