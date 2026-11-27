using System.Reflection.Metadata;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Op;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Reads MSIL instructions and emits the supported operations into a WASM function body.</summary>
/// <remarks>The caller owns the body writer and keeps the source assembly undisposed during emission.</remarks>
public readonly ref partial struct CodeEmitter
{
    private readonly IR.Module source;
    private readonly BodyWriter writer;
    private readonly IReadOnlyDictionary<string, (DataIndex Data, GlobalIndex Global)> literals;
    private readonly CodeRewriter rewriter;
    private readonly SignatureTypeCode returnType;
    private readonly MethodPlan method;
    private readonly IReadOnlyDictionary<IR.MethodIdentity, MethodPlan> methods;
    private readonly ReadOnlySpan<LocalVariable> parameters;
    private readonly ReadOnlySpan<LocalVariable> locals;
    private readonly bool[] initializedLocals;
    private readonly bool initLocals;
    private readonly Stack<MsilStackEntry> stack = new();

    private readonly Stack<(ExceptionBlock Block, int Level)> exceptionExits = new();
    private readonly Stack<int> cleanupExits = new();
    private readonly Stack<LocalIndex> caughtExceptions = new();

    private readonly Stack<(LoopBlock? Loop, LoopTransferKind Kind)> labels = new();
    private readonly Dictionary<LoopBlock, bool[]> loopExitInitialization = new();

    /// <summary>The source module whose metadata is used by instruction rewrites.</summary>
    public IR.Module Source => source;

    /// <summary>The borrowed function body writer; its owner, not a rewrite, must dispose it.</summary>
    public BodyWriter Writer => writer;

    /// <summary>The required types and functions for low-level emission in this module.</summary>
    public EmissionRuntime Runtime { get; }

    internal CodeEmitter(IR.Module source, BodyWriter writer,
        IReadOnlyDictionary<string, (DataIndex Data, GlobalIndex Global)> literals,
        EmissionRuntime runtime, CodeRewriter rewriter, MethodPlan method,
        IReadOnlyDictionary<IR.MethodIdentity, MethodPlan> methods)
    {
        this.source = source;
        this.writer = writer;
        this.literals = literals;
        Runtime = runtime;
        this.rewriter = rewriter;
        this.method = method;
        this.methods = methods;
        returnType = method.Signature.ReturnType;
        parameters = method.Signature.Parameters;
        locals = method.Locals;
        initLocals = method.Body.InitLocals;
        initializedLocals = new bool[locals.Length];
    }

    /// <summary>Consumes the supplied IL cursor, dispatching each opcode to its dedicated emitter.</summary>
    public void Emit(ILReader reader)
    {
        stack.Clear();
        Array.Fill(initializedLocals, initLocals);
        CheckExecutionDepth();
        InitializeMethodType();
        EnterFrame();
        InitializeGcLocals();
        if (initLocals)
            foreach (var selector in method.ReferenceLocals.Values) writer.I32Const(-1).LocalSet(selector);
        EnterExecutionDepth();
        if ((method.Frame is not null || method.Storage.ExecutionStack.Enabled) && method.Storage.ExceptionsEnabled)
        {
            OpenExceptionLabel(method.Storage.ExceptionReferenceBlock);
            writer.TryCatchAllRef(new(0)); labels.Push((null, default));
            EmitBody(reader);
            writer.Unreachable();
            CloseExceptionLabel();
            writer.Unreachable(); // The void try_table has no normal result for the exnref handler block.
            CloseExceptionLabel();
            writer.LocalSet(method.UnwindException);
            LeaveFrame();
            LeaveExecutionDepth();
            writer.LocalGet(method.UnwindException).ThrowRef();
        }
        else EmitBody(reader);
    }

    private void EmitBody(ILReader reader)
    {
        if (method.Flow is { } flow)
        {
            EmitFlow(flow);
            return;
        }
        while (reader.HasRemaining) EmitInstruction(ref reader);
    }

    private void EmitInstruction(ref ILReader reader)
    {
        int offset = reader.Offset;
        if (method.Graph.Specializations.TryGetValue(offset, out var specialized))
        {
            reader.Seek(specialized.End);
            if (specialized.Kind == XuiNet.CodeFlow.SpecializationKind.Int32)
            { writer.I32Const(specialized.Value); stack.Push(new(SignatureTypeCode.Int32)); }
            else if (specialized.Kind == XuiNet.CodeFlow.SpecializationKind.Branch)
            { stack.Pop(); writer.Drop(); }
            else if (specialized.Kind == XuiNet.CodeFlow.SpecializationKind.NullTypeTest)
            { stack.Pop(); writer.Drop(); EmitLdnull(); }
            else if (specialized.Kind == XuiNet.CodeFlow.SpecializationKind.NullCheckAndInt32)
            {
                stack.Pop(); writer.RefIsNull().If(); ThrowException(IR.Exceptions.ExceptionKind.NullReferenceException);
                writer.End().I32Const(specialized.Value); stack.Push(new(SignatureTypeCode.Int32));
            }
            else if (specialized.Kind == XuiNet.CodeFlow.SpecializationKind.DiscardAndInt32)
            { EmitPop(offset); writer.I32Const(specialized.Value); stack.Push(new(SignatureTypeCode.Int32)); }
            return;
        }
        var instruction = reader.Read();
        var candidate = reader;
        if (rewriter.TryRewrite(instruction, ref candidate, this, offset))
        {
            reader = candidate;
            return;
        }
        switch (instruction.Code)
        {
            case OpCode.Localloc: EmitLocalloc(offset); break;
            case OpCode.Mul_ovf_un: EmitMul_ovf_un(offset); break;
            case OpCode.Add_ovf: EmitAdd_ovf(offset); break;
            case OpCode.Add_ovf_un: EmitAdd_ovf_un(offset); break;
            case OpCode.Sub_ovf: EmitSub_ovf(offset); break;
            case OpCode.Sub_ovf_un: EmitSub_ovf_un(offset); break;
            case OpCode.Constrained:
                _ = instruction.ReadToken(ref reader);
                if (!method.SpanCalls.ContainsKey(reader.Offset) && !method.InterpolationCalls.ContainsKey(reader.Offset) && !method.PrimitiveCalls.ContainsKey(reader.Offset) && !method.ConstrainedCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch requiring boxing or a native primitive receiver is not implemented.");
                break;
            case OpCode.Ldind_i4: EmitLdind_i4(offset); break;
            case OpCode.Stind_i4: EmitStind_i4(offset); break;
            case OpCode.Throw: EmitThrow(offset); break;
            case OpCode.Rethrow: EmitRethrow(); break;
            case OpCode.Endfinally: EmitEndfinally(); break;
            case OpCode.Leave: EmitLeave(instruction.ReadBranch(ref reader).Target); break;
            case OpCode.Leave_s: EmitLeave(instruction.ReadShortBranch(ref reader).Target); break;
            case OpCode.Ldind_i1: EmitLdind_i1(offset); break;
            case OpCode.Ldind_u1: EmitLdind_u1(offset); break;
            case OpCode.Ldind_i2: EmitLdind_i2(offset); break;
            case OpCode.Ldind_u2: EmitLdind_u2(offset); break;
            case OpCode.Ldind_u4: EmitLdind_u4(offset); break;
            case OpCode.Ldind_i8: EmitLdind_i8(offset); break;
            case OpCode.Ldind_i: EmitLdind_i(offset); break;
            case OpCode.Ldind_r4: EmitLdind_r4(offset); break;
            case OpCode.Ldind_r8: EmitLdind_r8(offset); break;
            case OpCode.Ldind_ref: EmitLdind_ref(offset); break;
            case OpCode.Stind_i1: EmitStind_i1(offset); break;
            case OpCode.Stind_i2: EmitStind_i2(offset); break;
            case OpCode.Stind_i8: EmitStind_i8(offset); break;
            case OpCode.Stind_i: EmitStind_i(offset); break;
            case OpCode.Stind_r4: EmitStind_r4(offset); break;
            case OpCode.Stind_r8: EmitStind_r8(offset); break;
            case OpCode.Stind_ref: EmitStind_ref(offset); break;
            case OpCode.Ldsflda: EmitLdsflda(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Stsfld: EmitStsfld(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldtoken: EmitLdtoken(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldloca:
                EmitLdloca(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Ldloca_s:
                EmitLdloca_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Ldsfld: EmitLdsfld(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldfld:
                EmitLdfld(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldflda:
                EmitLdflda(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Stfld:
                EmitStfld(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Unbox:
                EmitUnbox(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Unbox_any:
                EmitUnbox_any(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Box:
                EmitBox(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Sizeof:
                EmitSizeof(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Initobj:
                EmitInitobj(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldobj:
                EmitLdobj(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Stobj:
                EmitStobj(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Cpobj:
                EmitCpobj(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldarga:
                EmitLdarga(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Ldarga_s:
                EmitLdarga_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Isinst:
                EmitIsinst(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Castclass:
                EmitCastclass(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldnull:
                EmitLdnull(); break;
            case OpCode.Callvirt:
                EmitCallvirt(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Newobj:
                EmitNewobj(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Newarr: EmitNewarr(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldlen: EmitLdlen(offset); break;
            case OpCode.Ldelema: EmitLdelema(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldelem: EmitLdelem(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Stelem: EmitStelem(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldelem_i1: EmitLdelem_i1(offset); break;
            case OpCode.Ldelem_u1: EmitLdelem_u1(offset); break;
            case OpCode.Ldelem_i2: EmitLdelem_i2(offset); break;
            case OpCode.Ldelem_u2: EmitLdelem_u2(offset); break;
            case OpCode.Ldelem_i4: EmitLdelem_i4(offset); break;
            case OpCode.Ldelem_u4: EmitLdelem_u4(offset); break;
            case OpCode.Ldelem_i8: EmitLdelem_i8(offset); break;
            case OpCode.Ldelem_i: EmitLdelem_i(offset); break;
            case OpCode.Ldelem_r4: EmitLdelem_r4(offset); break;
            case OpCode.Ldelem_r8: EmitLdelem_r8(offset); break;
            case OpCode.Ldelem_ref: EmitLdelem_ref(offset); break;
            case OpCode.Stelem_i1: EmitStelem_i1(offset); break;
            case OpCode.Stelem_i2: EmitStelem_i2(offset); break;
            case OpCode.Stelem_i4: EmitStelem_i4(offset); break;
            case OpCode.Stelem_i8: EmitStelem_i8(offset); break;
            case OpCode.Stelem_i: EmitStelem_i(offset); break;
            case OpCode.Stelem_r4: EmitStelem_r4(offset); break;
            case OpCode.Stelem_r8: EmitStelem_r8(offset); break;
            case OpCode.Stelem_ref: EmitStelem_ref(offset); break;
            case OpCode.Nop:
                EmitNop(); break;
            case OpCode.Volatile:
                EmitVolatile(); break;
            case OpCode.Ldc_i4_m1:
                EmitLdc_i4_m1(); break;
            case OpCode.Ldc_i4_0:
                EmitLdc_i4_0(); break;
            case OpCode.Ldc_i4_1:
                EmitLdc_i4_1(); break;
            case OpCode.Ldc_i4_2:
                EmitLdc_i4_2(); break;
            case OpCode.Ldc_i4_3:
                EmitLdc_i4_3(); break;
            case OpCode.Ldc_i4_4:
                EmitLdc_i4_4(); break;
            case OpCode.Ldc_i4_5:
                EmitLdc_i4_5(); break;
            case OpCode.Ldc_i4_6:
                EmitLdc_i4_6(); break;
            case OpCode.Ldc_i4_7:
                EmitLdc_i4_7(); break;
            case OpCode.Ldc_i4_8:
                EmitLdc_i4_8(); break;
            case OpCode.Ldc_i4:
                EmitLdc_i4(instruction.ReadInt(ref reader).Value); break;
            case OpCode.Ldc_i4_s:
                EmitLdc_i4_s(instruction.ReadShortInt(ref reader).Value); break;
            case OpCode.Ldc_r4:
                EmitLdc_r4(instruction.ReadFloat(ref reader).Value); break;
            case OpCode.Ldc_r8:
                EmitLdc_r8(instruction.ReadDouble(ref reader).Value); break;
            case OpCode.Add:
                EmitAdd(offset); break;
            case OpCode.Pop:
                EmitPop(offset); break;
            case OpCode.Sub:
                EmitSub(offset); break;
            case OpCode.Mul:
                EmitMul(offset); break;
            case OpCode.Div:
                EmitDiv(offset); break;
            case OpCode.Div_un:
                EmitDiv_un(offset); break;
            case OpCode.Rem:
                EmitRem(offset); break;
            case OpCode.Rem_un:
                EmitRem_un(offset); break;
            case OpCode.And:
                EmitAnd(offset); break;
            case OpCode.Or:
                EmitOr(offset); break;
            case OpCode.Xor:
                EmitXor(offset); break;
            case OpCode.Not:
                EmitNot(offset); break;
            case OpCode.Neg:
                EmitNeg(offset); break;
            case OpCode.Shl:
                EmitShl(offset); break;
            case OpCode.Shr:
                EmitShr(offset); break;
            case OpCode.Shr_un:
                EmitShr_un(offset); break;
            case OpCode.Ceq:
                EmitCeq(offset); break;
            case OpCode.Cgt:
                EmitCgt(offset); break;
            case OpCode.Cgt_un:
                EmitCgt_un(offset); break;
            case OpCode.Clt:
                EmitClt(offset); break;
            case OpCode.Clt_un:
                EmitClt_un(offset); break;
            case OpCode.Conv_i1:
                EmitConv_i1(offset); break;
            case OpCode.Conv_u1:
                EmitConv_u1(offset); break;
            case OpCode.Conv_i2:
                EmitConv_i2(offset); break;
            case OpCode.Conv_u2:
                EmitConv_u2(offset); break;
            case OpCode.Conv_i4:
                EmitConv_i4(offset); break;
            case OpCode.Conv_u4:
                EmitConv_u4(offset); break;
            case OpCode.Conv_i8:
                EmitConv_i8(offset); break;
            case OpCode.Conv_u8:
                EmitConv_u8(offset); break;
            case OpCode.Conv_r4:
                EmitConv_r4(offset); break;
            case OpCode.Conv_r8:
                EmitConv_r8(offset); break;
            case OpCode.Conv_r_un:
                EmitConv_r_un(offset); break;
            case OpCode.Ldc_i8:
                EmitLdc_i8(instruction.ReadLong(ref reader).Value); break;
            case OpCode.Ldloc:
                EmitLdloc(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Ldloc_s:
                EmitLdloc_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Ldloc_0:
                EmitLdloc_0(offset); break;
            case OpCode.Ldloc_1:
                EmitLdloc_1(offset); break;
            case OpCode.Ldloc_2:
                EmitLdloc_2(offset); break;
            case OpCode.Ldloc_3:
                EmitLdloc_3(offset); break;
            case OpCode.Stloc:
                EmitStloc(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Stloc_s:
                EmitStloc_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Stloc_0:
                EmitStloc_0(offset); break;
            case OpCode.Stloc_1:
                EmitStloc_1(offset); break;
            case OpCode.Stloc_2:
                EmitStloc_2(offset); break;
            case OpCode.Stloc_3:
                EmitStloc_3(offset); break;
            case OpCode.Conv_i:
                EmitConv_i(offset); break;
            case OpCode.Conv_u:
                EmitConv_u(offset); break;
            case OpCode.Dup:
                EmitDup(offset); break;
            case OpCode.Call:
                EmitCall(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldarg:
                EmitLdarg(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Ldarg_s:
                EmitLdarg_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Starg:
                EmitStarg(instruction.ReadVariable(ref reader), offset); break;
            case OpCode.Starg_s:
                EmitStarg_s(instruction.ReadShortVariable(ref reader), offset); break;
            case OpCode.Ldarg_0:
                EmitLdarg_0(offset); break;
            case OpCode.Ldarg_1:
                EmitLdarg_1(offset); break;
            case OpCode.Ldarg_2:
                EmitLdarg_2(offset); break;
            case OpCode.Ldarg_3:
                EmitLdarg_3(offset); break;
            case OpCode.Ldstr:
                EmitLdstr(instruction.ReadToken(ref reader)); break;
            case OpCode.Ldftn:
                EmitLdftn(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ldvirtftn:
                EmitLdvirtftn(instruction.ReadToken(ref reader), offset); break;
            case OpCode.Ret:
                EmitRet(offset); break;
            default:
                throw new NotSupportedException($"Unsupported XuiNet instruction {instruction.Code} at IL_{offset:x4}.");
        }
    }

    /// <summary>Collects literals, call targets, and temporary requirements, giving rewrites first chance.</summary>
    internal static List<string> CollectRequirements(IR.Module source, MethodPlan method, XuiNet.Reachability.DependencyGraph dependencies)
    {
        var reader = method.Body.Instructions;
        var literals = new List<string>();
        var seenLiterals = new HashSet<string>(StringComparer.Ordinal);
        if (method.Graph.Instructions.Any(n => n.IsReachable && method.Graph.Specializations.TryGetValue(n.Offset, out var replacement) &&
            replacement.Kind == XuiNet.CodeFlow.SpecializationKind.NullCheckAndInt32))
        {
            method.Storage.ExceptionsEnabled = true;
            method.RequireGcTemporaries();
        }
        foreach (var address in method.Graph.Instructions.Select(n => n.After?.Entry).OfType<XuiNet.CodeFlow.StackEntry>())
            if (address.DeclaredType is { IsByReference: true } declared)
            { method.Storage.RequireAddress(method.Storage.Bind(declared)); method.RequireGcTemporaries(); }
        foreach (var node in method.Graph.Instructions)
        {
            if (method.Flow is not null && !node.IsReachable || method.Graph.Specializations.ContainsKey(node.Offset)) continue;
            if (node.StoresStringAsObject) { method.Storage.StringObjects.Enable(method.Storage, source); method.RequireGcTemporaries(); }
            reader.Seek(node.Offset);
            int offset = reader.Offset;
            var instruction = reader.Read();
            if (dependencies.IsRewritten(method.Identity, offset)) continue;
            if (node.Successors.Length == 2 || instruction.Code is OpCode.Br or OpCode.Br_s) continue;
            switch (instruction.Code)
            {
                case >= OpCode.Newarr and <= OpCode.Stelem:
                    method.UsesArrays = true;
                    method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.IndexOutOfRangeException, IR.Exceptions.ExceptionKind.OverflowException]);
                    method.Storage.ExceptionsEnabled = true;
                    method.RequireGcTemporaries();
                    if (instruction.Code is OpCode.Newarr or OpCode.Ldelema)
                    {
                        var element = IR.Signatures.SignatureReader.StorageType(source,
                            System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
                        _ = method.Storage.ArrayShape(new(element));
                        if (instruction.Code == OpCode.Ldelema) method.Storage.RequireArrayAddress(new(element));
                        if (element.Identity is { } elementIdentity) method.RequireGc(elementIdentity);
                    }
                    break;
                case OpCode.Constrained:
                    _ = instruction.ReadToken(ref reader);
                    if (!method.SpanCalls.ContainsKey(reader.Offset) && !method.InterpolationCalls.ContainsKey(reader.Offset) && !method.PrimitiveCalls.ContainsKey(reader.Offset) && !method.ConstrainedCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch requiring boxing or a native primitive receiver is not implemented.");
                    break;
                case OpCode.Ldsflda:
                case OpCode.Ldtoken:
                    if (method.StaticSpans.TryGetValue(offset, out var dataToken))
                    {
                        _ = instruction.ReadToken(ref reader);
                        method.Storage.StaticData.Add(dataToken);
                        break;
                    }
                    if (instruction.Code == OpCode.Ldsflda) goto case OpCode.Stsfld;
                    var tokenType = IR.Intrinsics.ManagedType.Token(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
                    method.Storage.ManagedTypesEnabled = true;
                    if (tokenType.IsValueType) _ = method.Storage.Classes.Resolve(tokenType);
                    else method.RequireGc(tokenType);
                    break;
                case OpCode.Isinst:
                case OpCode.Castclass:
                    RequireTypeTest(method, source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value));
                    break;
                case OpCode.Mul_ovf_un:
                case OpCode.Add_ovf:
                case OpCode.Add_ovf_un:
                case OpCode.Sub_ovf:
                case OpCode.Sub_ovf_un:
                    var productType = node.After?.Entry?.Type switch {
                        XuiNet.CodeFlow.StackType.Int64 => SignatureTypeCode.Int64,
                        XuiNet.CodeFlow.StackType.NativeInt => SignatureTypeCode.IntPtr,
                        _ => SignatureTypeCode.Int32 };
                    method.EqualityTemporaries.TryAdd(new(productType), default);
                    method.RequireGcTemporaries(); method.Storage.ExceptionsEnabled = true;
                    method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.OverflowException);
                    break;
                case OpCode.Div:
                case OpCode.Div_un:
                case OpCode.Rem:
                case OpCode.Rem_un:
                    if (instruction.Code == OpCode.Rem && node.Before?.Entry?.Type == XuiNet.CodeFlow.StackType.Float)
                        method.Storage.MathImports.UsesRemainder = true;
                    if (node.Before?.Entry?.Type != XuiNet.CodeFlow.StackType.Float)
                    {
                        var category = node.Before?.Entry?.Type switch {
                            XuiNet.CodeFlow.StackType.Int64 => SignatureTypeCode.Int64,
                            XuiNet.CodeFlow.StackType.NativeInt => SignatureTypeCode.IntPtr,
                            _ => SignatureTypeCode.Int32 };
                        method.EqualityTemporaries.TryAdd(new(category), default);
                        method.RequireGcTemporaries(); method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.DivideByZeroException, IR.Exceptions.ExceptionKind.OverflowException]);
                    }
                    break;
                case OpCode.Ldnull:
                    method.RequireGcTemporaries(); break;
                case OpCode.Throw:
                case OpCode.Rethrow:
                    method.Storage.ExceptionsEnabled = true;
                    method.RequireGcTemporaries();
                    break;
                case OpCode.Leave:
                case OpCode.Leave_s:
                case OpCode.Endfinally:
                    break;
                case OpCode.Switch:
                    method.UsesSwitch = true;
                    break;
                case OpCode.Ldind_i1:
                case OpCode.Ldind_u1:
                case OpCode.Ldind_i2:
                case OpCode.Ldind_u2:
                case OpCode.Ldind_u4:
                case OpCode.Ldind_i8:
                case OpCode.Ldind_i:
                case OpCode.Ldind_r4:
                case OpCode.Ldind_r8:
                case OpCode.Ldind_ref:
                case OpCode.Stind_i1:
                case OpCode.Stind_i2:
                case OpCode.Stind_i8:
                case OpCode.Stind_i:
                case OpCode.Stind_r4:
                case OpCode.Stind_r8:
                case OpCode.Stind_ref:
                    method.RequireGcTemporaries(); break;
                case OpCode.Ldind_i4:
                case OpCode.Stind_i4:
                case OpCode.Nop:
                case OpCode.Localloc:
                case OpCode.Volatile:
                case OpCode.Ldarg_0:
                case OpCode.Ldarg_1:
                case OpCode.Ldarg_2:
                case OpCode.Ldarg_3:
                case OpCode.Conv_i:
                case OpCode.Conv_u:
                case OpCode.Ldloc_0:
                case OpCode.Ldloc_1:
                case OpCode.Ldloc_2:
                case OpCode.Ldloc_3:
                case OpCode.Stloc_0:
                case OpCode.Stloc_1:
                case OpCode.Stloc_2:
                case OpCode.Stloc_3:
                case OpCode.Ret:
                case OpCode.Add:
                case OpCode.Pop:
                case OpCode.Sub:
                case OpCode.Mul:
                case OpCode.And:
                case OpCode.Or:
                case OpCode.Xor:
                case OpCode.Not:
                case OpCode.Neg:
                case OpCode.Shl:
                case OpCode.Shr:
                case OpCode.Shr_un:
                case OpCode.Ceq:
                case OpCode.Cgt:
                case OpCode.Cgt_un:
                case OpCode.Clt:
                case OpCode.Clt_un:
                case OpCode.Conv_i1:
                case OpCode.Conv_u1:
                case OpCode.Conv_i2:
                case OpCode.Conv_u2:
                case OpCode.Conv_i4:
                case OpCode.Conv_u4:
                case OpCode.Conv_i8:
                case OpCode.Conv_u8:
                case OpCode.Conv_r4:
                case OpCode.Conv_r8:
                case OpCode.Conv_r_un:
                case OpCode.Ldc_i4_m1:
                case OpCode.Ldc_i4_0:
                case OpCode.Ldc_i4_1:
                case OpCode.Ldc_i4_2:
                case OpCode.Ldc_i4_3:
                case OpCode.Ldc_i4_4:
                case OpCode.Ldc_i4_5:
                case OpCode.Ldc_i4_6:
                case OpCode.Ldc_i4_7:
                case OpCode.Ldc_i4_8:
                    break;
                case OpCode.Ldarga:
                case OpCode.Ldloca:
                case OpCode.Ldarg:
                case OpCode.Starg:
                case OpCode.Ldloc:
                case OpCode.Stloc:
                    _ = instruction.ReadVariable(ref reader); break;
                case OpCode.Ldarga_s:
                case OpCode.Ldloca_s:
                case OpCode.Ldarg_s:
                case OpCode.Starg_s:
                case OpCode.Ldloc_s:
                case OpCode.Stloc_s:
                    _ = instruction.ReadShortVariable(ref reader); break;
                case OpCode.Ldsfld:
                    var staticField = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    if (IR.Intrinsics.StringEmpty.Is(source, staticField))
                    {
                        if (seenLiterals.Add("")) literals.Add("");
                        break;
                    }
                    if (IR.Intrinsics.StopwatchConstant.Read(source, staticField) is not null) break;
                    if (IR.Intrinsics.TimeSpanZero.Read(source, staticField, method.Identity.DeclaringType) is not null) break;
                    reader.Seek(offset); _ = reader.Read();
                    goto case OpCode.Stsfld;
                case OpCode.Stsfld:
                    var staticToken = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    var staticIdentity = IR.MemberResolver.Field(source, staticToken, method.Identity.DeclaringType, method.Identity.Arguments);
                    var staticDefinition = staticIdentity.DeclaringType.Module.GetField(staticIdentity.Handle);
                    if ((staticDefinition.Attributes & (System.Reflection.FieldAttributes.HasFieldRVA | System.Reflection.FieldAttributes.Literal)) != 0)
                        throw new NotSupportedException($"Direct access to literal/RVA static storage is not implemented: {staticDefinition.Name}.");
                    var staticStorage = method.Storage.Statics[staticIdentity.DeclaringType].Fields.Fields[staticIdentity].Storage;
                    method.RequireGcTemporaries();
                    if (staticStorage.Identity is { } staticValue) method.RequireGc(staticValue);
                    if (instruction.Code == OpCode.Ldsflda) method.Storage.RequireAddress(staticStorage);
                    break;
                case OpCode.Ldfld:
                case OpCode.Ldflda:
                case OpCode.Stfld:
                    var fieldHandle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    var resolvedField = IR.MemberResolver.Field(source, fieldHandle, method.Identity.DeclaringType, method.Identity.Arguments);
                    if (instruction.Code == OpCode.Ldflda || resolvedField.DeclaringType.IsValueType)
                        method.Storage.RequireAddress(method.Storage.Bind(IR.Signatures.SignatureReader.Field(resolvedField.DeclaringType.Module, resolvedField.DeclaringType.Module.GetField(resolvedField.Handle).Signature, resolvedField.DeclaringType.Arguments)));
                    if (instruction.Code == OpCode.Ldflda && StorageModel.IsReference(method.Storage.Bind(IR.Signatures.SignatureReader.Field(
                        resolvedField.DeclaringType.Module, resolvedField.DeclaringType.Module.GetField(resolvedField.Handle).Signature, resolvedField.DeclaringType.Arguments))))
                    { method.Storage.ReferenceAddressesEnabled = true; method.RequireGcTemporaries(); }
                    if (method.Storage.IsGc(resolvedField.DeclaringType))
                    {
                        method.RequireGc(resolvedField.DeclaringType);
                        if (method.Storage.IsClass(resolvedField.DeclaringType)) method.Storage.ExceptionsEnabled = true;
                    }
                    break;
                case OpCode.Box:
                case OpCode.Unbox:
                case OpCode.Unbox_any:
                    var boxToken = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    if (IR.BoxingType.Read(source, boxToken, method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } box)
                    {
                        method.RequireGcTemporaries();
                        if (instruction.Code == OpCode.Box) method.Storage.RequireBoxAllocation(box);
                        else if (instruction.Code == OpCode.Unbox) method.Storage.RequireBoxAddress(box);
                        else _ = method.Storage.Box(box);
                    }
                    else if (instruction.Code != OpCode.Box)
                    {
                        if (instruction.Code == OpCode.Unbox) throw new NotSupportedException("unbox requires a value type.");
                        RequireTypeTest(method, source, boxToken);
                    }
                    break;
                case OpCode.Sizeof:
                    _ = SizeOfType(method, source, instruction.ReadToken(ref reader).Value);
                    break;
                case OpCode.Initobj:
                case OpCode.Ldobj:
                case OpCode.Stobj:
                case OpCode.Cpobj:
                    var objectStorage = method.Storage.Bind(IR.Signatures.SignatureReader.StorageType(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments));
                    if (objectStorage.IsScalar) method.RequireGcTemporaries();
                    break;
                case OpCode.Ldc_i8:
                    _ = instruction.ReadLong(ref reader); break;
                case OpCode.Ldc_i4:
                    _ = instruction.ReadInt(ref reader); break;
                case OpCode.Ldc_i4_s:
                    _ = instruction.ReadShortInt(ref reader); break;
                case OpCode.Ldc_r4:
                    _ = instruction.ReadFloat(ref reader); break;
                case OpCode.Ldc_r8:
                    _ = instruction.ReadDouble(ref reader); break;

                case OpCode.Dup:
                    method.NeedsDup = true;
                    break;
                case OpCode.Ldftn:
                case OpCode.Ldvirtftn:
                    var pointed = ResolveCall(source, method, instruction.ReadToken(ref reader).Value, offset);
                    if (instruction.Code == OpCode.Ldftn) method.Storage.DelegateMethods.Add(pointed);
                    else
                    {
                        _ = method.Storage.Classes.Slot(pointed);
                        method.Storage.ExceptionsEnabled = true;
                        method.RequireGcTemporaries();
                    }
                    break;
                case OpCode.Newobj:
                case OpCode.Callvirt:
                case OpCode.Call:
                    var call = instruction.ReadToken(ref reader);
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ExecutionStack.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is not null)
                    {
                        method.Storage.ExecutionStack.Enabled = method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.InsufficientExecutionStackException);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.MemoryBarrier.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value))) break;
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.CurrentThread.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value))) break;
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.GcArrayAllocation.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } allocation)
                    {
                        method.UsesArrays = method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.OverflowException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                        _ = method.Storage.ArrayShape(allocation.Signature.Result.Array!);
                        if (allocation.Element.Identity is { } elementType) method.RequireGc(elementType);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (!method.PrimitiveCalls.ContainsKey(offset) &&
                        (!method.ConstrainedCalls.TryGetValue(offset, out var hashCall) || hashCall.ReferenceReceiver is not null) &&
                        IR.Intrinsics.ObjectHash.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        if (hashCall?.ReferenceReceiver is { } hashReceiver)
                            method.Storage.RequireAddress(method.Storage.Bind(hashReceiver));
                        method.Storage.Primitives.UsesObjectHash = method.Storage.Primitives.UsesStringHash = true;
                        method.Storage.ExceptionsEnabled = true;
                        method.RequireGcTemporaries();
                        if (instruction.Code == OpCode.Callvirt)
                            foreach (var (receiver, implementation) in method.Storage.ObjectHashTargets)
                            {
                                method.RequireGc(receiver);
                                if (!method.Calls.Contains(implementation)) method.Calls.Add(implementation);
                            }
                        break;
                    }
                    if (!method.PrimitiveCalls.ContainsKey(offset) && !method.ConstrainedCalls.ContainsKey(offset) &&
                        !method.SpanCalls.ContainsKey(offset) && !method.InterpolationCalls.ContainsKey(offset) &&
                        IR.Intrinsics.ObjectText.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.Storage.ObjectTextEnabled = method.Storage.ArrayIdentity.Enabled = true;
                        method.UsesExceptionText = method.UsesExceptionMessage = method.Storage.Formatting.ExceptionText = true;
                        method.UsesCompositeFormatting = method.Storage.Formatting.Composite = true;
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.NotSupportedException);
                        method.RequireGcTemporaries();
                        foreach (var textTarget in method.Storage.ObjectTextTargets)
                        {
                            method.RequireGc(textTarget.Type);
                            if (instruction.Code == OpCode.Callvirt && textTarget.Override is { } implementation && !method.Calls.Contains(implementation))
                                method.Calls.Add(implementation);
                        }
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.RuntimeImport.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } hostImport)
                    {
                        method.Storage.HostImports.Require(hostImport);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.HostScheduler.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } scheduler)
                    {
                        method.Storage.Tasks.Callback = scheduler.Callback;
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.MonitorMethod.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } monitor)
                    {
                        method.Storage.Monitors.Enabled = true;
                        method.RequireGcTemporaries();
                        if (monitor.HasLockTaken) method.Storage.RequireAddress(new(SignatureTypeCode.Boolean));
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.SynchronizationLockException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.NotSupportedException);
                        foreach (string parameter in new[] { "obj", "lockTaken", MonitorRuntime.InvalidLockTakenMessage })
                            if (seenLiterals.Add(parameter)) literals.Add(parameter);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.BitConversion.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is not null) break;
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.SuppressFinalization.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        if (seenLiterals.Add("obj")) literals.Add("obj");
                        break;
                    }
                    if (IR.Intrinsics.RectangularArray.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } rectangle)
                    {
                        method.UsesArrays = true;
                        method.RequireGcTemporaries();
                        method.RectangularIndexCount = Math.Max(method.RectangularIndexCount, rectangle.ArrayType?.Array?.Rank ?? 1);
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.IndexOutOfRangeException, IR.Exceptions.ExceptionKind.OverflowException]);
                        if (rectangle.ArrayType?.Array is { } array)
                        {
                            _ = method.Storage.ArrayShape(array);
                            if (rectangle.Operation == "Address") method.Storage.RequireArrayAddress(new(array.Element));
                        }
                        break;
                    }
                    if (method.StaticSpans.TryGetValue(offset, out var staticSpan))
                    {
                        method.Storage.StaticData.Add(staticSpan);
                        if (staticSpan.ArrayInitializer)
                        {
                            method.UsesArrays = true; method.RequireGcTemporaries(); method.Storage.ExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentException, IR.Exceptions.ExceptionKind.ArgumentNullException]);
                            method.Storage.ArgumentExceptionsEnabled = true;
                            if (seenLiterals.Add("array")) literals.Add("array");
                        }
                        break;
                    }
                    if (method.PrimitiveCalls.TryGetValue(offset, out var primitive))
                    {
                        if (primitive.AddressReceiver) method.Storage.RequireAddress(method.Storage.Bind(primitive.Element.ByReference()));
                        RequirePrimitiveOperation(method, primitive.Element, primitive.Operation);
                        if (primitive.Operation == "ToString")
                        {
                            string descriptor = method.Storage.Formatting.RequireEnum(primitive.Element.Identity!.Value);
                            if (seenLiterals.Add(descriptor)) literals.Add(descriptor);
                        }
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.ManagedType.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } managedType)
                    {
                        method.Storage.ManagedTypesEnabled = true;
                        if (managedType.Factory is { } assemblyFactory)
                        {
                            method.Storage.ManagedAssemblyEnabled = true;
                            if (!method.Calls.Contains(assemblyFactory)) method.Calls.Add(assemblyFactory);
                        }
                        method.Storage.ExceptionsEnabled |= managedType.Signature.HasThis;
                        method.RequireGcTemporaries();
                        if (managedType.Signature.Result.Identity is { } typeIdentity) method.RequireGc(typeIdentity);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.NativeStringComparer.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is not null) break;
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.CharFormatting.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } character)
                    {
                        method.Storage.StringAccess.UsesCharFormatting = true;
                        if (character.HasThis) method.Storage.RequireAddress(new(SignatureTypeCode.Char, IsByReference: true));
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.StringAccess.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } stringAccess)
                    {
                        method.UsesStringAccess = method.Storage.StringAccess.Enabled = true;
                        method.Storage.StringAccess.UsesLowerInvariant |= stringAccess.Operation == "ToLowerInvariant";
                        if (stringAccess.Operation == "Replace")
                        {
                            method.Storage.StringAccess.UsesReplace = true;
                            method.Storage.StringAccess.UsesCharFormatting |= stringAccess.Signature.Parameters[0].Code == SignatureTypeCode.Char;
                            method.EqualityTemporaries.TryAdd(new(SignatureTypeCode.String), default);
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.ArgumentException]);
                            if (seenLiterals.Add("oldValue")) literals.Add("oldValue");
                        }
                        if (stringAccess.Search is not null)
                        {
                            method.Storage.StringAccess.UsesSearch = true;
                            method.Storage.StringAccess.UsesCharFormatting |= stringAccess.Signature.Parameters[0].Code == SignatureTypeCode.Char;
                            method.EqualityTemporaries.TryAdd(new(SignatureTypeCode.String), default);
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                            foreach (string text in new[] { "value", "comparisonType" }) if (seenLiterals.Add(text)) literals.Add(text);
                        }
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException, IR.Exceptions.ExceptionKind.IndexOutOfRangeException]);
                        method.RequireGcTemporaries();
                        foreach (string text in new[] { "startIndex", "length", "count" }) if (seenLiterals.Add(text)) literals.Add(text);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.StringSplit.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } split)
                    {
                        method.UsesStringSplit = method.UsesArrays = method.Storage.StringSplit.Enabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.RequireGcTemporaries();
                        _ = method.Storage.ArrayShape(split.Signature.Result.Array!);
                        if (split.Signature.Parameters[0].Array is { } separators) _ = method.Storage.ArrayShape(separators);
                        foreach (string text in new[] { "count", "options" }) if (seenLiterals.Add(text)) literals.Add(text);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.TextElement.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } textElement)
                    {
                        method.Storage.StringAccess.UsesTextElements = true;
                        method.UsesSpanOperations = true;
                        method.RequireGc(textElement.Signature.Parameters[0].Identity!.Value);
                        method.Storage.RequireAddress(new(SignatureTypeCode.Char));
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.StringConcatenation.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } concat)
                    {
                        if (concat.Spans)
                        {
                            method.UsesSpanOperations = true;
                            method.RequireGc(concat.Signature.Parameters[0].Identity!.Value);
                            method.Storage.RequireAddress(new(SignatureTypeCode.Char));
                        }
                        method.StringConcatArgumentCount = Math.Max(method.StringConcatArgumentCount, concat.Signature.Parameters.Length);
                        method.Storage.StringConcatEnabled = true;
                        literals.Add("");
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ResourceString.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } resourceText)
                    {
                        if (seenLiterals.Add(resourceText)) literals.Add(resourceText);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.CompositeFormatting.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } formatting)
                    {
                        method.UsesCompositeFormatting = method.Storage.Formatting.Composite = true;
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException, IR.Exceptions.ExceptionKind.ArgumentNullException]);
                        method.RequireGcTemporaries();
                        if (formatting.HasArray)
                        {
                            method.UsesArrays = true;
                            _ = method.Storage.ArrayShape(formatting.Signature.Parameters[^1].Array!);
                        }
                        foreach (string text in new[] { "format", "args" }) if (seenLiterals.Add(text)) literals.Add(text);
                        break;
                    }
                    if (method.InvariantFormatting.TryGetValue(offset, out var parsed) && parsed.Parsing is { } parsing)
                    {
                        method.UsesNumericParsing = true;
                        method.Storage.Formatting.UsesDoubleParsing |= parsing.Result.Code == SignatureTypeCode.Double;
                        method.Storage.Formatting.UsesUInt32Parsing |= parsing.Result.Code == SignatureTypeCode.UInt32;
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException,
                            IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.OverflowException]);
                        method.RequireGcTemporaries();
                        if (seenLiterals.Add("s")) literals.Add("s");
                        if (parsing.IsSpan)
                        {
                            method.UsesSpanOperations = true;
                            method.RequireGc(parsing.Input.Identity!.Value);
                            method.Storage.RequireAddress(new(SignatureTypeCode.Char));
                        }
                        break;
                    }
                    if (method.InvariantFormatting.TryGetValue(offset, out var decimalFormatting) && decimalFormatting.Value.Identity is { } decimalType &&
                        IR.FrameworkTypes.Is(decimalType, "System", "Decimal"))
                    {
                        method.Storage.Formatting.UsesDecimal = true;
                        method.Storage.RequireAddress(method.Storage.Bind(decimalFormatting.Value));
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (method.InvariantFormatting.TryGetValue(offset, out var scalarFormatting) && scalarFormatting.ValueType is SignatureTypeCode.Double or SignatureTypeCode.Single)
                    {
                        method.Storage.Formatting.UsesDouble |= scalarFormatting.ValueType == SignatureTypeCode.Double;
                        method.Storage.Formatting.UsesSingle |= scalarFormatting.ValueType == SignatureTypeCode.Single;
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (!method.InvariantFormatting.ContainsKey(offset) && instruction.Code == OpCode.Call && IR.Intrinsics.Int32Formatting.InvariantProvider(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } provider)
                    {
                        method.RequireGc(provider.Identity!.Value);
                        break;
                    }
                    if (method.InvariantFormatting.ContainsKey(offset) || instruction.Code == OpCode.Call && IR.Intrinsics.Int32Formatting.Is(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.Storage.Formatting.RequireInteger(method.InvariantFormatting.TryGetValue(offset, out var integerFormat) ? integerFormat.ValueType : SignatureTypeCode.Int32);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ConstructorFactory.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } factoryConstructor)
                    {
                        method.RequireGc(factoryConstructor.Type);
                        method.Storage.ExceptionsEnabled = true;
                        if (factoryConstructor.Constructor is { } constructor)
                        {
                            if (!method.Calls.Contains(constructor)) method.Calls.Add(constructor);
                            method.Storage.InnerExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.TargetInvocationException);
                        }
                        else method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.MissingMethodException);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ArrayClear.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is not null)
                    {
                        method.UsesArrayCopy = true;
                        method.RequireGcTemporaries();
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.IndexOutOfRangeException);
                        if (seenLiterals.Add("array")) literals.Add("array");
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ArrayCopy.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } copyOperation)
                    {
                        method.UsesArrayCopy = true;
                        method.RequireGcTemporaries();
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        foreach (var name in copyOperation.ByteOffsets ? new[] { "src", "dst", "srcOffset", "dstOffset", "count" } :
                            new[] { "sourceArray", "destinationArray", "sourceIndex", "destinationIndex", "length" })
                            if (seenLiterals.Add(name)) literals.Add(name);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.MonotonicClock.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } clock)
                    {
                        if (clock.Operation == "get_TickCount64") method.Storage.Clock.UsesTickCount64 = true;
                        else method.Storage.Clock.UsesTimestamp = true;
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.MathMethod.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } math)
                    {
                        if (math.HasRoundingMode)
                        {
                            method.RequireGcTemporaries();
                            method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentException);
                            if (seenLiterals.Add("mode")) literals.Add("mode");
                        }
                        method.Storage.MathImports.UsesCbrt |= math.Operation == "Cbrt";
                        method.Storage.MathImports.UsesExp |= math.Operation == "Exp";
                        method.Storage.MathImports.UsesLog |= math.Operation == "Log";
                        method.Storage.MathImports.UsesSin |= math.Operation == "Sin";
                        method.Storage.MathImports.UsesCos |= math.Operation == "Cos";
                        method.Storage.MathImports.UsesTan |= math.Operation == "Tan";
                        method.Storage.MathImports.UsesAtan2 |= math.Operation == "Atan2";
                        if (math.Operation is "Pow" or "Atan2" or "CopySign" or "Min" or "Max")
                        {
                            method.Storage.MathImports.UsesPow |= math.Operation == "Pow";
                            method.UsesNativeFloat |= math.IsSingle;
                        }
                        break;
                    }
                    if ((method.SpanCalls.GetValueOrDefault(offset) ?? (instruction.Code is OpCode.Call or OpCode.Newobj ? IR.Intrinsics.SpanAccess.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) : null)) is { } spanAccess)
                    {
                        method.UsesSpanOperations |= spanAccess.Operation is "Fill" or "Clear" or "Slice" or "CopyTo" or "TryCopyTo" or "FromArray" or "FromReference" or "FromPointer" or "ToString" or "ToArray";
                        if (spanAccess.Operation == "ToArray")
                        {
                            method.UsesArrays = true;
                            method.Storage.RequireArrayAddress(spanAccess.Signature.Result.Array!);
                        }
                        if (spanAccess.Operation == "Fill")
                        {
                            var element = method.Storage.Bind(spanAccess.Owner.Arguments!.Values[0]);
                            if (element.Identity is { IsValueType: true })
                                throw new NotSupportedException("Span.Fill with struct values is not implemented.");
                            method.EqualityTemporaries.TryAdd(element, default);
                        }
                        if (spanAccess.Operation == "FromPointer")
                        {
                            var element = method.Storage.Bind(spanAccess.Owner.Arguments!.Values[0]);
                            if (element.UsesGc || StorageModel.IsReference(element) || element.Identity is { } identity && method.Storage.HasStackReferences(identity))
                                throw new NotSupportedException("Pointer-backed spans require reference-free linear elements.");
                        }
                        if (spanAccess.Operation == "FromReference") method.RequireGc(spanAccess.Owner);
                        if (spanAccess.Operation == "FromArray")
                        {
                            method.UsesArrays = true;
                            method.RequireGc(spanAccess.Owner);
                            method.Storage.RequireArrayAddress(spanAccess.Signature.Parameters[0].Array!);
                        }
                        method.Storage.ExceptionsEnabled = true;
                        if (spanAccess.Operation == "get_Item") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.IndexOutOfRangeException);
                        if (spanAccess.Operation is "Fill" or "Clear" or "Slice" or "FromArray" or "FromPointer") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        if (spanAccess.Operation == "CopyTo") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentException);
                        method.RequireGcTemporaries();
                        if (spanAccess.Operation is "Fill" or "Clear" or "CopyTo" or "TryCopyTo" or "ToString")
                            method.Storage.RequireAddress(method.Storage.Bind(spanAccess.Owner.Arguments!.Values[0]));
                        break;
                    }
                    if (instruction.Code == OpCode.Newobj && IR.Intrinsics.StringCharacters.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } characters)
                    {
                        method.UsesStringAccess = true;
                        method.RequireGcTemporaries();
                        _ = method.Storage.ArrayShape(characters.Parameters[0].Array!);
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException]);
                        foreach (string text in new[] { "value", "startIndex", "length" }) if (seenLiterals.Add(text)) literals.Add(text);
                        break;
                    }
                    if (instruction.Code == OpCode.Newobj && IR.Intrinsics.StringRepeat.Is(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.RequireGcTemporaries();
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        if (seenLiterals.Add("count")) literals.Add("count");
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Newobj && IR.Intrinsics.StringSpan.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } stringSpan)
                    {
                        method.UsesSpanOperations=true;
                        method.RequireGc((stringSpan.Constructor?stringSpan.Signature.Parameters[0]:stringSpan.Signature.Result).Identity!.Value);
                        if (!stringSpan.Constructor)
                        {
                            method.Storage.StringObjects.Enable(method.Storage,source);
                            method.Storage.StringAccess.Enabled=method.Storage.StringAccess.UsesSpans=true;
                        }
                        method.Storage.RequireAddress(new(SignatureTypeCode.Char));
                        method.Storage.ExceptionsEnabled=true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.InlineArrayAccess.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is not null)
                    {
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.IndexOutOfRangeException);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if ((method.InterpolationCalls.GetValueOrDefault(offset) ?? (instruction.Code is OpCode.Call or OpCode.Newobj ? IR.Intrinsics.InterpolatedStringHandler.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) : null)) is { } interpolation)
                    {
                        method.Storage.Interpolation.Enabled = true;
                        if (interpolation.HasProvider)
                        {
                            method.RequireGc(IR.Intrinsics.InvariantCulture.Resolve(source));
                            method.Storage.ExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.NotSupportedException);
                        }
                        if (interpolation.IsEnum)
                        {
                            string descriptor = method.Storage.Formatting.RequireEnum(interpolation.Signature.Parameters[0].Identity!.Value);
                            if (seenLiterals.Add(descriptor)) literals.Add(descriptor);
                        }
                        if (interpolation.IsFormattedInteger)
                        {
                            method.Storage.Formatting.UsesFormattedInteger = method.Storage.ExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                        }
                        if (interpolation.IsFloatingPoint)
                        {
                            method.Storage.Formatting.UsesSingle |= interpolation.IsSingle;
                            method.Storage.Formatting.UsesDouble |= !interpolation.IsSingle;
                            method.Storage.ExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                        }
                        method.RequireGcTemporaries();
                        foreach (string text in new[] { "True", "False" }) if (seenLiterals.Add(text)) literals.Add(text);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.NullGuard.Is(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.StringEquality.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } textEquality)
                    {
                        method.Storage.StringEqualityEnabled = true;
                        if (textEquality.HasComparison)
                        {
                            method.UsesStringAccess = method.Storage.StringAccess.UsesIgnoreCaseEquality = true;
                            method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                            if (seenLiterals.Add("comparisonType")) literals.Add("comparisonType");
                        }
                        method.Storage.ExceptionsEnabled |= textEquality.Signature.HasThis;
                        method.EqualityTemporaries.TryAdd(new(SignatureTypeCode.String), default);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.SharedArrayPool.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } pool)
                    {
                        method.RequireGc(pool.Owner);
                        method.UsesArrays = method.UsesArrayCopy = true;
                        _ = method.Storage.ArrayShape(new(pool.Element));
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException]);
                        foreach (string name in new[] { "array", "minimumLength" }) if (seenLiterals.Add(name)) literals.Add(name);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.DefaultEquality.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } equality)
                    {
                        if (IR.Intrinsics.DefaultEquality.Is(equality.Owner)) method.RequireGc(equality.Owner);
                        else method.RequireGcTemporaries();
                        if (equality.Operation == "GetHashCode") RequireDefaultHash(method, equality);
                        if (equality.Operation is "Equals" or "IndexOf")
                        {
                            method.Storage.ExceptionsEnabled = true;
                            var storage = method.Storage.Bind(equality.Element);
                            method.EqualityTemporaries.TryAdd(storage, default);
                            if (storage.TypeCode == SignatureTypeCode.String) method.Storage.StringEqualityEnabled = true;
                            if (equality.Implementation is { } equals && !method.Calls.Contains(equals)) method.Calls.Add(equals);
                            if (equality.UsesObjectEquality)
                                foreach (var equalityTarget in method.Storage.EqualityTargets(equality.Element.Identity!.Value))
                                    if (!method.Calls.Contains(equalityTarget.Method)) method.Calls.Add(equalityTarget.Method);
                            if (equality.Operation == "IndexOf")
                            {
                                method.UsesEqualitySearch = method.UsesArrays = true;
                                _ = method.Storage.ArrayShape(new(equality.Element));
                                method.Storage.ArgumentExceptionsEnabled = true;
                                method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                                method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                                foreach (var name in new[] { "array", "startIndex", "count" })
                                    if (seenLiterals.Add(name)) literals.Add(name);
                            }
                        }
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.TimeSpanFactory.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } duration)
                    {
                        var shape = method.Storage.Shape(duration.Owner);
                        if (method.Storage.IsGc(duration.Owner) || shape.Leaves.Count != 1 || shape.Leaves[0].Storage.TypeCode != SignatureTypeCode.Int64 ||
                            shape.Leaves[0].MemoryOffset != 0 || method.Storage.StackLayout(duration.Owner).Size != 8)
                            throw new NotSupportedException("TimeSpan requires a single signed 64-bit tick field at offset zero.");
                        method.Storage.ExceptionsEnabled = true;
                        bool floating = duration.Signature.Parameters[0].Code == SignatureTypeCode.Double;
                        method.Storage.ExtraExceptionKinds.Add(floating ? IR.Exceptions.ExceptionKind.OverflowException : IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        method.RequireGcTemporaries();
                        if (floating && seenLiterals.Add(IR.Intrinsics.TimeSpanFactory.NaNMessage)) literals.Add(IR.Intrinsics.TimeSpanFactory.NaNMessage);
                        if (seenLiterals.Add(IR.Intrinsics.TimeSpanFactory.OverflowMessage)) literals.Add(IR.Intrinsics.TimeSpanFactory.OverflowMessage);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ManagedBuffer.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } buffer)
                    {
                        method.UsesSpanOperations = true;
                        method.RequireGcTemporaries();
                        method.Storage.ExceptionsEnabled = method.Storage.ReferenceAddressesEnabled = true;
                        method.Storage.RequireAddress(method.Storage.Bind(buffer.Element));
                        if (buffer.Operation == "GetArrayDataReference") method.Storage.RequireArrayAddress(new(buffer.Element));
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ManagedReference.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } address)
                    {
                        method.Storage.RequireAddress(method.Storage.Bind(address.Element.ByReference()));
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ReferenceInterlocked.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } exchange)
                    {
                        var exchangeValue = method.Storage.Bind(exchange.Result);
                        method.Storage.RequireAddress(exchangeValue);
                        if (exchange.Result.Code == SignatureTypeCode.String) method.EqualityTemporaries.TryAdd(exchangeValue, default);
                        else method.UsesReferenceInterlocked = true;
                        method.Storage.ReferenceAddressesEnabled = true;
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.NativeFloat.TryMethod(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), out var nativeOperation, out _))
                    {
                        method.UsesNativeFloat = true;
                        if (nativeOperation == "op_Modulus") method.Storage.MathImports.UsesRemainder = true;
                        if (nativeOperation == "Equals") method.RequireGcTemporaries();
                        if (nativeOperation == "Clamp")
                        {
                            method.UsesNativeFloatClamp = true;
                            method.Storage.ExceptionsEnabled = true;
                            method.RequireGcTemporaries();
                        }
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.WellKnownMethods.IsObjectConstructor(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value))) break;
                    if (IR.Exceptions.ExceptionTypes.TryMember(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), out var exceptionKind, out var exceptionMember))
                    {
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(exceptionKind);
                        if (exceptionKind == IR.Exceptions.ExceptionKind.XmlException)
                        {
                            method.Storage.XmlExceptionsEnabled = true;
                            if (exceptionMember == IR.Exceptions.ExceptionMember.XmlConstructor)
                            {
                                method.UsesXmlConstructor = method.UsesArrays = method.UsesCompositeFormatting = true;
                                method.Storage.InnerExceptionsEnabled = method.Storage.StringConcatEnabled = true;
                                method.Storage.Formatting.Composite = method.Storage.Formatting.UsesInt32 = true;
                                _ = method.Storage.ArrayShape(new(new(SignatureTypeCode.String)));
                                method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                                foreach (string text in new[] { "{0}", " Line ", ", position ", "." })
                                    if (seenLiterals.Add(text)) literals.Add(text);
                            }
                        }
                        if (exceptionKind == IR.Exceptions.ExceptionKind.ObjectDisposedException)
                            foreach (string text in new[] { "", "\nObject name: '", "'." })
                                if (seenLiterals.Add(text)) literals.Add(text);
                        method.Storage.ArgumentExceptionsEnabled |= IR.Exceptions.ExceptionTypes.UsesParameterName(exceptionMember) || exceptionMember == IR.Exceptions.ExceptionMember.TypeName;
                        method.Storage.InnerExceptionsEnabled |= exceptionMember is IR.Exceptions.ExceptionMember.InnerException or IR.Exceptions.ExceptionMember.MessageInnerConstructor;
                        if (exceptionMember == IR.Exceptions.ExceptionMember.MessageInnerConstructor && exceptionKind == IR.Exceptions.ExceptionKind.AggregateException)
                        {
                            method.Storage.ArgumentExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                            if (seenLiterals.Add("innerException")) literals.Add("innerException");
                        }
                        if (exceptionKind == IR.Exceptions.ExceptionKind.AggregateException)
                        {
                            method.Storage.StringConcatEnabled = method.Storage.InnerExceptionsEnabled = true;
                            foreach (string text in new[] { "", " (", ")" })
                                if (seenLiterals.Add(text)) literals.Add(text);
                        }
                        if (exceptionMember == IR.Exceptions.ExceptionMember.Text)
                            method.UsesExceptionText = method.Storage.Formatting.ExceptionText = true;
                        method.UsesExceptionMessage |= exceptionMember is IR.Exceptions.ExceptionMember.Message or IR.Exceptions.ExceptionMember.Text;
                        if (exceptionMember is IR.Exceptions.ExceptionMember.ActualValue or IR.Exceptions.ExceptionMember.ParameterValueMessageConstructor)
                        {
                            method.Storage.ActualValueExceptionsEnabled = method.Storage.Formatting.Composite = true;
                            method.Storage.ArgumentExceptionsEnabled = true;
                            method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.FormatException, IR.Exceptions.ExceptionKind.NotSupportedException]);
                            if (seenLiterals.Add("\nActual value was {0}.")) literals.Add("\nActual value was {0}.");
                        }
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (IR.DelegateInfo.Operation(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity) is { } delegateOperation)
                    {
                        method.Storage.DelegateOperations.Add(delegateOperation);
                        method.Storage.DelegatesEnabled = true;
                        method.Storage.ExceptionsEnabled = true;
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (IR.DelegateInfo.Member(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity) is { } delegateMember)
                    {
                        method.RequireGc(delegateMember.DeclaringType);
                        method.Storage.ExceptionsEnabled = true;
                        var invoke = IR.DelegateInfo.Invoke(delegateMember.DeclaringType);
                        if (!method.Calls.Contains(invoke)) method.Calls.Add(invoke);
                        break;
                    }
                    if (instruction.Code == OpCode.Callvirt) { method.Storage.ExceptionsEnabled = true; method.RequireGcTemporaries(); }
                    var target = method.ConstrainedCalls.TryGetValue(offset, out var concrete) ? concrete.Target : ResolveCall(source, method, call.Value, offset);
                    if (method.ConstrainedCalls.TryGetValue(offset, out var receiverCall) && receiverCall.ReferenceReceiver is { } referenceReceiver)
                        method.Storage.RequireAddress(method.Storage.Bind(referenceReceiver));
                    method.Storage.RequireArrayInterface(target);
                    if (method.Storage.ArrayEnumerator(target) is { } enumerator)
                    {
                        method.RequireGc(enumerator.DeclaringType);
                        if (!method.Calls.Contains(enumerator)) method.Calls.Add(enumerator);
                    }
                    if (IR.Intrinsics.ArrayInterface.Operation(target) == "CopyTo")
                    {
                        method.UsesArrayCopy = true;
                        method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.UnionWith([IR.Exceptions.ExceptionKind.ArgumentNullException, IR.Exceptions.ExceptionKind.ArgumentException]);
                        foreach (var name in new[] { "destinationArray", "destinationIndex" })
                            if (seenLiterals.Add(name)) literals.Add(name);
                    }
                    if (IR.Intrinsics.ArrayInterface.Operation(target) == "FixedSize")
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.NotSupportedException);
                    if (IR.Intrinsics.ArrayInterface.Operation(target) == "Item")
                    {
                        method.Storage.ArgumentExceptionsEnabled = true;
                        if (seenLiterals.Add("index")) literals.Add("index");
                    }
                    if (IR.Intrinsics.DefaultEquality.ForInterface(target) is { } interfaceComparer)
                    {
                        method.Storage.ComparerInterfacesEnabled = true;
                        method.RequireGc(interfaceComparer.Owner);
                        var elementStorage = method.Storage.Bind(interfaceComparer.Element);
                        if (interfaceComparer.Operation == "GetHashCode") RequireDefaultHash(method, interfaceComparer);
                        if (interfaceComparer.Operation == "Equals")
                        {
                            method.EqualityTemporaries.TryAdd(elementStorage, default);
                            method.Storage.StringEqualityEnabled |= elementStorage.TypeCode == SignatureTypeCode.String;
                            if (interfaceComparer.Implementation is { } implementation && !method.Calls.Contains(implementation)) method.Calls.Add(implementation);
                            if (interfaceComparer.UsesObjectEquality)
                                foreach (var equalityTarget in method.Storage.EqualityTargets(interfaceComparer.Element.Identity!.Value))
                                    if (!method.Calls.Contains(equalityTarget.Method)) method.Calls.Add(equalityTarget.Method);
                        }
                    }
                    var attributes = target.DeclaringType.Module.GetMethod(target.Handle).Attributes;
                    if (instruction.Code == OpCode.Callvirt && (!method.ConstrainedCalls.TryGetValue(offset, out var constrainedCall) || constrainedCall.ReferenceReceiver is not null) && (attributes & System.Reflection.MethodAttributes.Virtual) != 0)
                    {
                        _ = method.Storage.Classes.Slot(target);
                    }
                    else if (instruction.Code == OpCode.Call && (attributes & System.Reflection.MethodAttributes.Abstract) != 0)
                        throw new InvalidProgramException("A direct call cannot target an abstract method.");
                    if (instruction.Code == OpCode.Newobj)
                    {
                        var constructor = target.DeclaringType.Module.GetMethod(target.Handle);
                        var owner = target.DeclaringType;
                        if (constructor.Name != ".ctor")
                            throw new InvalidProgramException("newobj must target a constructor.");
                        if (method.Storage.IsGc(owner)) method.RequireGc(owner);
                        else _ = (method.Frame ?? throw new NotSupportedException($"Constructor result storage is not implemented for {owner} in {method.Identity}.")).Layout(owner);
                        if (method.Storage.IsClass(owner))
                        {
                            var type = method.Storage.Classes.Resolve(owner);
                            if (type.IsAbstract) throw new InvalidProgramException("Cannot allocate an abstract class.");
                        }
                    }
                    if (!method.Calls.Contains(target)) method.Calls.Add(target);
                    break;
                case OpCode.Ldstr:
                    var loadString = instruction.ReadToken(ref reader);
                    string literal = GetString(source, loadString.Value);
                    if (seenLiterals.Add(literal)) literals.Add(literal);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported XuiNet instruction {instruction.Code} at IL_{offset:x4}.");
            }
        }
        return literals;
    }
}
