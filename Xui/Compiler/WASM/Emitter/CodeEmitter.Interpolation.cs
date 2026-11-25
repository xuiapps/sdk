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
        bool construct = call.Operation == ".ctor";
        if (construct)
        {
            // These are capacity hints, not observable text. Allocate lazily on the first append.
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.Drop();
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.Drop();
        }
        else if (call.Signature.Parameters.Length == 1)
        {
            var type = call.Signature.Parameters[0].Code;
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
