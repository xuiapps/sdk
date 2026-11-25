using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryTimeSpanFactory(Instruction.Token instruction, int offset)
    {
        if (TimeSpanFactory.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } factory) return false;
        var input = factory.Signature.Parameters[0].Code;
        Pop(input, instruction.Code, offset);
        if (input == SignatureTypeCode.Double) EmitFloatingDuration(factory);
        else
        {
            if (input == SignatureTypeCode.Int32) writer.I64ExtendI32S();
            writer.LocalSet(method.GcI64);
            writer.LocalGet(method.GcI64).I64Const(long.MaxValue / factory.TicksPerUnit).I64GtS()
                .LocalGet(method.GcI64).I64Const(long.MinValue / factory.TicksPerUnit).I64LtS().I32Or().If();
            ThrowTimeSpanException(ExceptionKind.ArgumentOutOfRangeException, TimeSpanFactory.OverflowMessage);
            writer.End().LocalGet(method.GcI64).I64Const(factory.TicksPerUnit).I64Mul().LocalSet(method.GcI64);
        }
        FrameAddress(Frame.Values[offset]);
        writer.LocalGet(method.GcI64).I64Store();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, factory.Owner));
        return true;
    }

    private void EmitFloatingDuration(TimeSpanFactory factory)
    {
        writer.LocalTee(method.GcF64).LocalGet(method.GcF64).F64Eq().I32Eqz().If();
        ThrowTimeSpanException(ExceptionKind.ArgumentException, TimeSpanFactory.NaNMessage);
        writer.End();
        writer.LocalGet(method.GcF64).F64Const(factory.TicksPerUnit).F64Mul().LocalSet(method.GcF64);
        writer.LocalGet(method.GcF64).F64Const((double)long.MaxValue).F64Gt()
            .LocalGet(method.GcF64).F64Const((double)long.MinValue).F64Lt().I32Or().If();
        ThrowTimeSpanException(ExceptionKind.OverflowException, TimeSpanFactory.OverflowMessage);
        writer.End();

        // double(long.MaxValue) is 2^63. .NET accepts that rounded boundary as MaxValue.
        writer.LocalGet(method.GcF64).F64Const((double)long.MaxValue).F64Eq().If();
        writer.I64Const(long.MaxValue).LocalSet(method.GcI64).Else();
        writer.LocalGet(method.GcF64).I64TruncF64S().LocalSet(method.GcI64).End();
    }

    private void ThrowTimeSpanException(ExceptionKind kind, string message)
    {
        EmitString(message);
        NewException(kind, suppliedMessage: true);
        writer.Throw(method.Storage.ExceptionTag);
    }
}
