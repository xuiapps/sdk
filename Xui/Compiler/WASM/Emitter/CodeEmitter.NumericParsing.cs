using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNumericParsing(Instruction.Token instruction, int offset, NumericParsing parsing)
    {
        if (parsing.HasStyle) Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        else writer.I32Const(parsing.DefaultStyle);
        writer.LocalSet(method.NumericParsingStyle);
        PopAssignable(method.Storage.Bind(parsing.Input), instruction.Code, offset);
        if (parsing.IsSpan) { EmitCharacterSpanString(); stack.Pop(); }
        writer.LocalGet(method.NumericParsingStyle).Call(parsing.Result.Code == SignatureTypeCode.Double
            ? method.Storage.Formatting.ParseDouble : method.Storage.Formatting.ParseUInt32).LocalSet(method.GcI32);
        // Leave the parsed scalar on the operand stack; a failure throws and unwinds it.
        foreach (var (status, kind) in new[] { (1, ExceptionKind.FormatException), (2, ExceptionKind.NotSupportedException),
            (3, ExceptionKind.OverflowException), (5, ExceptionKind.ArgumentException) })
        {
            writer.LocalGet(method.GcI32).I32Const(status).I32Eq().If(); ThrowException(kind); writer.End();
        }
        writer.LocalGet(method.GcI32).I32Const(4).I32Eq().If(); ThrowArgumentException(ExceptionKind.ArgumentNullException, "s"); writer.End();
        stack.Push(new(method.Storage.Bind(parsing.Result).StackType));
    }
}
