using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringSplit(Instruction.Token instruction, int offset)
    {
        var call = StringSplit.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (call is null) return false;
        var runtime = method.Storage.StringSplit;
        if (call.HasOptions) Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        else writer.I32Const(0);
        writer.LocalSet(method.SplitOptions);
        if (call.HasCount) Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        else writer.I32Const(int.MaxValue);
        writer.LocalSet(method.SplitCount);
        var separator = call.Signature.Parameters[0];
        PopAssignable(method.Storage.Bind(separator), instruction.Code, offset);
        if (separator.Array is { } separators)
        {
            var shape = method.Storage.ArrayShape(separators);
            writer.LocalSet(method.ArrayReference).RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.ExternRef).LocalSet(method.SplitSeparator);
            writer.LocalGet(method.ArrayReference).RefIsNull().I32Eqz().If();
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().Call(runtime.Create).LocalSet(method.SplitSeparator);
            writer.I32Const(0).LocalSet(method.ArrayIndex).Block(method.Storage.EmptyBlock).Loop();
            writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32LtU().I32Eqz().BrIf(new(1));
            writer.LocalGet(method.SplitSeparator).LocalGet(method.ArrayIndex);
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex).ArrayGet(shape.Type);
            if (separators.Element.Code == SignatureTypeCode.Char) writer.Call(runtime.Character);
            writer.Call(runtime.Set);
            writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0)).End().End().End();
        }
        else
        {
            if (separator.Code == SignatureTypeCode.Char) writer.Call(runtime.Character);
            writer.LocalSet(method.SplitSeparator);
        }
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.LocalSet(method.SplitText).LocalGet(method.SplitText).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End().LocalGet(method.SplitCount).I32Const(0).I32LtS().If();
        ThrowStringArgument(ExceptionKind.ArgumentOutOfRangeException, "count");
        writer.End().LocalGet(method.SplitOptions).I32Const(3).I32GtU().If();
        ThrowStringArgument(ExceptionKind.ArgumentException, "options");
        writer.End();
        writer.LocalGet(method.SplitText).LocalGet(method.SplitSeparator).I32Const(separator.Array is null ? 0 : 1)
            .LocalGet(method.SplitCount).LocalGet(method.SplitOptions).Call(runtime.Split).LocalSet(method.GcExtern);
        var result = method.Storage.ArrayShape(call.Signature.Result.Array!);
        writer.LocalGet(method.GcExtern).Call(runtime.Length).ArrayNewDefault(result.Type);
        TagArray(result);
        writer.LocalSet(method.ArrayReference);
        writer.I32Const(0).LocalSet(method.ArrayIndex).Block(method.Storage.EmptyBlock).Loop();
        writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(result.Type).ArrayLen().I32LtU().I32Eqz().BrIf(new(1));
        writer.LocalGet(method.ArrayReference).RefCast(result.Type).LocalGet(method.ArrayIndex);
        writer.LocalGet(method.GcExtern).LocalGet(method.ArrayIndex).Call(runtime.Get).ArraySet(result.Type);
        writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0)).End().End();
        writer.LocalGet(method.ArrayReference);
        stack.Push(new(SignatureTypeCode.SZArray, array: result.Signature));
        return true;
    }

}
