using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private int OpenExceptionLabel(TypeIndex type)
    {
        int level = labels.Count;
        writer.Block(type);
        labels.Push((null, default));
        return level;
    }
    private void CloseExceptionLabel() { writer.End(); labels.Pop(); }
    private void BranchExceptionLabel(int level) => writer.Br(new((uint)(labels.Count - level - 1)));

    private void EmitExceptionBlock(ExceptionBlock block)
    {
        var storage = method.Storage;
        int done = OpenExceptionLabel(storage.EmptyBlock);
        exceptionExits.Push((block.Continuation, done));
        bool cleanup = block.Handlers[0].Kind is ExceptionRegionKind.Finally or ExceptionRegionKind.Fault;
        OpenExceptionLabel(cleanup ? storage.ExceptionReferenceBlock : storage.ExceptionCatchBlock);
        if (cleanup) writer.TryCatchAllRef(new(0)); else writer.TryCatchRef(storage.ExceptionTag, new(0));
        labels.Push((null, default));
        stack.Clear();
        EmitFlow(block.Body);
        CloseExceptionLabel(); // try_table
        BranchExceptionLabel(done);
        CloseExceptionLabel(); // caught payload
        var pending = method.ExceptionLocals[block.Handlers[0].Offset];
        var pendingReference = cleanup ? pending : method.CatchExceptionRefs[block.Handlers[0].Offset];
        if (!cleanup) writer.LocalSet(pendingReference);
        writer.LocalSet(pending);
        stack.Clear();
        if (cleanup)
        {
            EmitCleanup(block.Handlers[0]);
            writer.LocalGet(pending).ThrowRef();
        }
        else
        {
            foreach (var handler in block.Handlers)
            {
                EmitExceptionMatch(pending, ExceptionTypes.Kind(handler.CatchType!.Value) ?? ExceptionKind.Exception);
                writer.If(); labels.Push((null, default));
                var caught = method.ExceptionLocals[handler.Offset];
                writer.LocalGet(pending).LocalTee(caught);
                stack.Push(new(SignatureTypeCode.TypeHandle, handler.CatchType));
                var caughtReference = method.CatchExceptionRefs[handler.Offset];
                writer.LocalGet(pendingReference).LocalSet(caughtReference);
                caughtExceptions.Push(caughtReference);
                EmitFlow(handler.Body);
                caughtExceptions.Pop();
                stack.Clear();
                BranchExceptionLabel(done);
                CloseExceptionLabel();
            }
            writer.LocalGet(pendingReference).ThrowRef();
        }
        exceptionExits.Pop();
        CloseExceptionLabel(); // normal continuation
        stack.Clear();
        if (cleanup && block.Handlers[0].Kind == ExceptionRegionKind.Finally) EmitCleanup(block.Handlers[0]);
        if (!block.CanContinue) writer.Unreachable();
    }

    private void EmitCleanup(ExceptionHandler handler)
    {
        int end = OpenExceptionLabel(method.Storage.EmptyBlock);
        cleanupExits.Push(end);
        stack.Clear();
        EmitFlow(handler.Body);
        cleanupExits.Pop();
        CloseExceptionLabel();
        stack.Clear();
    }

    private void EmitExceptionMatch(LocalIndex value, ExceptionKind target)
    {
        if (target == ExceptionKind.Exception) { writer.I32Const(1); return; }
        writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0));
        if (target == ExceptionKind.SystemException) writer.I32Const((int)ExceptionKind.Exception).I32Eq().I32Eqz();
        else
        {
            writer.I32Const((int)target).I32Eq();
            if (target == ExceptionKind.ArgumentException)
                foreach (var derived in new[] { ExceptionKind.ArgumentNullException, ExceptionKind.ArgumentOutOfRangeException })
                    if (method.Storage.ExtraExceptionKinds.Contains(derived))
                        writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                            .I32Const((int)derived).I32Eq().I32Or();
        }
    }

    private void NewException(ExceptionKind kind, bool suppliedMessage = false)
    {
        if (suppliedMessage)
        {
            writer.LocalSet(method.GcExtern).LocalGet(method.GcExtern).RefIsNull().If();
            EmitString(ExceptionTypes.Message(kind));
            writer.LocalSet(method.GcExtern).End().I32Const((int)kind).LocalGet(method.GcExtern);
        }
        else
        {
            writer.I32Const((int)kind);
            EmitString(ExceptionTypes.Message(kind));
        }
        if (method.Storage.ArgumentExceptionsEnabled) writer.RefNull(ValueTypeCode.ExternRef);
        writer.StructNew(method.Storage.ExceptionType);
    }

    private void ThrowException(ExceptionKind kind)
    {
        NewException(kind);
        writer.Throw(method.Storage.ExceptionTag);
    }

    // The reference remains on the WASM stack. This does not alter the tracked MSIL stack.
    private void CheckExceptionNull()
    {
        writer.LocalTee(method.GcOwner).RefIsNull().If();
        ThrowException(ExceptionKind.NullReferenceException);
        writer.End().LocalGet(method.GcOwner);
    }

    private bool TryExceptionMember(Instruction.Token instruction, int offset)
    {
        var token = MetadataTokens.EntityHandle(instruction.Value);
        if (!ExceptionTypes.TryMember(source, token, out var kind, out var member)) return false;
        if (ExceptionTypes.IsConstructor(member))
        {
            if (instruction.Code != OpCode.Newobj) throw new NotSupportedException("Exception base-constructor calls are not implemented.");
            if (ExceptionTypes.UsesParameterName(member)) EmitArgumentException(kind, member, instruction, offset);
            else
            {
                bool message = member == ExceptionMember.MessageConstructor;
                if (message) Pop(SignatureTypeCode.String, instruction.Code, offset);
                NewException(kind, message);
            }
            var reference = source.GetMemberReference((MemberReferenceHandle)token);
            stack.Push(new(SignatureTypeCode.TypeHandle, new(source, reference.Parent)));
        }
        else
        {
            _ = Pop(instruction.Code, offset);
            CheckExceptionNull();
            writer.RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(member == ExceptionMember.ParamName ? 2u : 1u));
            stack.Push(new(SignatureTypeCode.String));
        }
        return true;
    }
}
