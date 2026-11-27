using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TrySharedArrayPool(Instruction.Token instruction, int offset)
    {
        var access = SharedArrayPool.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        var pool = method.Storage.SharedPools[access.Owner];
        if (access.Operation == "get_Shared")
        {
            writer.GlobalGet(pool.Instance);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
            return true;
        }
        var shape = method.Storage.ArrayShape(new(access.Element));
        if (access.Operation == "Rent")
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyLength);
        }
        else
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopySourceIndex);
            PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), instruction.Code, offset);
            writer.LocalSet(method.ArrayReference);
        }
        var receiver = Pop(instruction.Code, offset);
        if (!receiver.IsNull && (receiver.IsByReference || receiver.Identity != access.Owner))
            throw new NotSupportedException("Shared array pool calls require their declared pool receiver.");
        writer.RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
        if (access.Operation == "Rent")
        {
            writer.LocalGet(method.CopyLength).I32Const(0).I32LtS().If();
            ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "minimumLength"); writer.End();
            // Take the cached array before exposing it; nested rents cannot alias a live lease.
            writer.GlobalGet(pool.Cached).LocalSet(method.ArrayReference);
            writer.LocalGet(method.ArrayReference).RefIsNull().I32Eqz().If();
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().LocalGet(method.CopyLength).I32LtU().If();
            writer.RefNull(ValueTypeCode.EqRef).LocalSet(method.ArrayReference).Else();
            writer.RefNull(ValueTypeCode.EqRef).GlobalSet(pool.Cached).End().End();
            writer.LocalGet(method.ArrayReference).RefIsNull().If();
            writer.LocalGet(method.CopyLength); NewArray(shape); writer.LocalSet(method.ArrayReference);
            writer.End().LocalGet(method.ArrayReference);
            stack.Push(new(SignatureTypeCode.SZArray, array: shape.Signature));
        }
        else
        {
            CheckCopyNull(method.ArrayReference, "array");
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().LocalSet(method.CopyLength);
            writer.LocalGet(method.CopySourceIndex).If();
            writer.LocalGet(method.ArrayReference).LocalSet(method.GcOwner).I32Const(0).LocalSet(method.CopyDestinationIndex);
            ClearArray(shape); writer.End();
            // Bound retained storage. Larger arrays are released to WASM GC.
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32Const(65536).I32GtU().I32Eqz().If();
            writer.LocalGet(method.ArrayReference).GlobalSet(pool.Cached).End();
        }
        return true;
    }
}
