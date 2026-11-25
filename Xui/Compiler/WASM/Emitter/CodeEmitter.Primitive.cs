using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static void RequirePrimitiveOperation(MethodPlan method, SignatureType element, string operation)
    {
        var storage = method.Storage.Bind(element);
        method.EqualityTemporaries.TryAdd(storage, default);
        method.RequireGcTemporaries();
        method.Storage.ExceptionsEnabled = true;
        if (storage.TypeCode == SignatureTypeCode.String)
        {
            if (operation == "Equals") method.Storage.StringEqualityEnabled = true;
            else method.Storage.Primitives.UsesStringHash = true;
        }
    }

    private void EmitPrimitiveCall(PrimitiveCall call, OpCode code, int offset)
    {
        var storage = method.Storage.Bind(call.Element);
        var (left, right) = method.EqualityTemporaries[storage];
        if (call.Operation == "Equals")
        {
            PopAssignable(storage, code, offset);
            StorePrimitive(storage, right);
        }
        if (call.AddressReceiver) ReadScalarAddress(Pop(code, offset), storage);
        else PopAssignable(storage, code, offset);
        writer.LocalSet(left);
        if (storage.TypeCode == SignatureTypeCode.String)
        {
            writer.LocalGet(left).RefIsNull().If();
            ThrowException(ExceptionKind.NullReferenceException);
            writer.End();
        }
        if (call.Operation == "GetHashCode") EmitPrimitiveHash(storage, left);
        else ComparePrimitive(storage, left, right);
        stack.Push(new(SignatureTypeCode.Int32));
    }

    private void StorePrimitive(LocalVariable storage, LocalIndex local)
    {
        Narrow(storage);
        if (storage.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
        writer.LocalSet(local);
    }

    // Input locals use the MSIL evaluation representation (f64 for Single).
    // Hash bits match CoreLib scalar rules on the WASM32 target.
    private void EmitPrimitiveHash(LocalVariable storage, LocalIndex value)
    {
        switch (storage.TypeCode)
        {
            case SignatureTypeCode.String:
                writer.LocalGet(value).Call(method.Storage.Primitives.StringHash);
                break;
            case SignatureTypeCode.Boolean:
                writer.LocalGet(value).I32Eqz().I32Eqz();
                break;
            case SignatureTypeCode.Char:
                writer.LocalGet(value).LocalGet(value).I32Const(16).I32Shl().I32Or();
                break;
            case SignatureTypeCode.Single:
                writer.LocalGet(value).F32DemoteF64().I32ReinterpretF32().LocalSet(method.GcI32);
                FloatingSpecial(value);
                writer.If().LocalGet(method.GcI32).I32Const(0x7f800000).I32And().LocalSet(method.GcI32).End();
                writer.LocalGet(method.GcI32);
                break;
            case SignatureTypeCode.Double:
                writer.LocalGet(value).I64ReinterpretF64().LocalSet(method.GcI64);
                FloatingSpecial(value);
                writer.If().LocalGet(method.GcI64).I64Const(0x7ff0000000000000).I64And().LocalSet(method.GcI64).End();
                Fold64(method.GcI64);
                break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64:
                Fold64(value);
                break;
            default:
                writer.LocalGet(value);
                break;
        }
    }

    private void FloatingSpecial(LocalIndex value) => writer.LocalGet(value).F64Const(0).F64Eq()
        .LocalGet(value).LocalGet(value).F64Eq().I32Eqz().I32Or();

    private void Fold64(LocalIndex value) => writer.LocalGet(value).I32WrapI64()
        .LocalGet(value).I64Const(32).I64ShrU().I32WrapI64().I32Xor();
}
