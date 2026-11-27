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
        exceptionExits.Push((block, done));
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
        else if (block.Continuation is { } continuation && exceptionExits.Any(exit => exit.Block.Continuations.Contains(continuation)))
        {
            // A nested leave may exit several protected scopes. Finish this scope's
            // cleanup, then resume the enclosing exit so its finally runs as well.
            EmitLeave(block.Continuation!.Value);
        }
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
        if (target == ExceptionKind.SystemException)
        {
            writer.I32Const((int)ExceptionKind.Exception).I32Eq().I32Eqz();
            foreach (var excluded in new[] { ExceptionKind.ApplicationException, ExceptionKind.TargetInvocationException, ExceptionKind.AggregateException, ExceptionKind.JsonException })
                if (method.Storage.ExtraExceptionKinds.Contains(excluded))
                    writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                        .I32Const((int)excluded).I32Eq().I32Eqz().I32And();
        }
        else
        {
            writer.I32Const((int)target).I32Eq();
            if (target == ExceptionKind.ApplicationException && method.Storage.ExtraExceptionKinds.Contains(ExceptionKind.TargetInvocationException))
                writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                    .I32Const((int)ExceptionKind.TargetInvocationException).I32Eq().I32Or();
            if (target == ExceptionKind.ArithmeticException)
                foreach (var derived in new[] { ExceptionKind.DivideByZeroException, ExceptionKind.OverflowException })
                    if (method.Storage.ExtraExceptionKinds.Contains(derived))
                        writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                            .I32Const((int)derived).I32Eq().I32Or();
            if (target is ExceptionKind.IOException or ExceptionKind.InvalidOperationException or ExceptionKind.NotSupportedException or ExceptionKind.FormatException)
            {
                var derived = target == ExceptionKind.FormatException ? ExceptionKind.UriFormatException : target == ExceptionKind.IOException ? ExceptionKind.EndOfStreamException : target == ExceptionKind.NotSupportedException ? ExceptionKind.PlatformNotSupportedException : ExceptionKind.ObjectDisposedException;
                if (method.Storage.ExtraExceptionKinds.Contains(derived))
                    writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                        .I32Const((int)derived).I32Eq().I32Or();
            }
            if (target == ExceptionKind.ArgumentException)
                foreach (var derived in new[] { ExceptionKind.ArgumentNullException, ExceptionKind.ArgumentOutOfRangeException, ExceptionKind.EncoderFallbackException, ExceptionKind.DecoderFallbackException })
                    if (method.Storage.ExtraExceptionKinds.Contains(derived))
                        writer.LocalGet(value).RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(0))
                            .I32Const((int)derived).I32Eq().I32Or();
            if (target is ExceptionKind.MemberAccessException or ExceptionKind.MissingMemberException)
                foreach (var derived in new[] { ExceptionKind.MissingMemberException, ExceptionKind.MissingMethodException, ExceptionKind.MissingFieldException })
                    if (derived != target && ExceptionTypes.IsAssignable(derived, target) && method.Storage.ExtraExceptionKinds.Contains(derived))
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
        if (method.Storage.InnerExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        if (method.Storage.ActualValueExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        method.Storage.WriteXmlExceptionDefaults(writer);
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
            if (member == ExceptionMember.XmlConstructor) EmitXmlException(instruction, offset);
            else if (member == ExceptionMember.MessageInnerConstructor) EmitExceptionWithInner(kind, instruction, offset);
            else if (ExceptionTypes.UsesParameterName(member)) EmitArgumentException(kind, member, instruction, offset);
            else
            {
                bool message = member == ExceptionMember.MessageConstructor;
                if (message) Pop(SignatureTypeCode.String, instruction.Code, offset);
                NewException(kind, message);
            }
            var parent = token.Kind == HandleKind.MemberReference ? source.GetMemberReference((MemberReferenceHandle)token).Parent
                : source.GetMethod((MethodDefinitionHandle)token).DeclaringType;
            stack.Push(new(SignatureTypeCode.TypeHandle, new(source, parent)));
        }
        else
        {
            _ = Pop(instruction.Code, offset);
            CheckExceptionNull();
            uint field = member == ExceptionMember.ActualValue ? method.Storage.ActualValueField : member == ExceptionMember.InnerException ? method.Storage.InnerExceptionField : member is ExceptionMember.ParamName or ExceptionMember.TypeName ? 2u : 1u;
            if (member is ExceptionMember.XmlLineNumber or ExceptionMember.XmlLinePosition or ExceptionMember.XmlSourceUri or ExceptionMember.XmlResource)
            {
                uint xmlField = method.Storage.XmlExceptionField + (member switch {
                    ExceptionMember.XmlLineNumber => 0u, ExceptionMember.XmlLinePosition => 1u,
                    ExceptionMember.XmlSourceUri => 2u, _ => 3u });
                writer.RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(xmlField));
            }
            else if (member == ExceptionMember.Text) EmitExceptionText();
            else if (member == ExceptionMember.Message && method.Storage.ExtraExceptionKinds.Contains(ExceptionKind.AggregateException)) EmitAggregateMessage();
            else if (member == ExceptionMember.Message && method.Storage.ActualValueExceptionsEnabled) EmitExceptionMessage();
            else writer.RefCast(method.Storage.ExceptionType).StructGet(method.Storage.ExceptionType, new(field));
            if (kind == ExceptionKind.ObjectDisposedException && member == ExceptionMember.ParamName)
            {
                writer.LocalTee(method.GcExtern).RefIsNull().If(); EmitString("");
                writer.LocalSet(method.GcExtern).End().LocalGet(method.GcExtern);
            }
            var signature = XuiNet.DotNet.Signatures.SignatureReader.Method(source, token.Kind == HandleKind.MemberReference ? source.GetMemberReference((MemberReferenceHandle)token).Signature
                : source.GetMethod((MethodDefinitionHandle)token).Signature);
            stack.Push(new(signature.Result.Code, signature.Result.Identity));
        }
        return true;
    }
}
