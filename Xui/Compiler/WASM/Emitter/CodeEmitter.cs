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

    private readonly Stack<(int? Target, int Level)> exceptionExits = new();
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
        EnterFrame();
        InitializeGcLocals();
        if (initLocals)
            foreach (var selector in method.ReferenceLocals.Values) writer.I32Const(-1).LocalSet(selector);
        if (method.Frame is not null && method.Storage.ExceptionsEnabled)
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
            { Pop(SignatureTypeCode.Int32, OpCode.Pop, offset); writer.Drop(); }
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
            case OpCode.Constrained:
                _ = instruction.ReadToken(ref reader);
                if (!method.InterpolationCalls.ContainsKey(reader.Offset) && !method.PrimitiveCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch is only implemented for interpolation ToString and primitive hashing/equality.");
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
            case OpCode.Ldsflda:
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
        foreach (var address in method.Graph.Instructions.Select(n => n.After?.Entry).OfType<XuiNet.CodeFlow.StackEntry>())
            if (address.DeclaredType is { IsByReference: true } declared)
            { method.Storage.RequireAddress(method.Storage.Bind(declared)); method.RequireGcTemporaries(); }
        foreach (var node in method.Graph.Instructions)
        {
            if (method.Flow is not null && !node.IsReachable || method.Graph.Specializations.ContainsKey(node.Offset)) continue;
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
                    if (!method.InterpolationCalls.ContainsKey(reader.Offset) && !method.PrimitiveCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch is only implemented for interpolation ToString and primitive hashing/equality.");
                    break;
                case OpCode.Ldsflda:
                case OpCode.Ldtoken:
                    if (method.StaticSpans.TryGetValue(offset, out var dataToken))
                    {
                        _ = instruction.ReadToken(ref reader);
                        method.Storage.StaticData.Add(dataToken);
                        break;
                    }
                    var tokenType = IR.Intrinsics.ManagedType.Token(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
                    method.Storage.ManagedTypesEnabled = true;
                    if (tokenType.IsValueType) _ = method.Storage.Classes.Resolve(tokenType);
                    else method.RequireGc(tokenType);
                    break;
                case OpCode.Isinst:
                case OpCode.Castclass:
                    RequireTypeTest(method, source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value));
                    break;
                case OpCode.Div:
                case OpCode.Div_un:
                case OpCode.Rem:
                case OpCode.Rem_un:
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
                    if (IR.Intrinsics.StopwatchConstant.Read(source, staticField) is not null) break;
                    if (IR.Intrinsics.TimeSpanZero.Read(source, staticField, method.Identity.DeclaringType) is null)
                        throw new NotSupportedException("Static field storage is not implemented except TimeSpan.Zero and Stopwatch constants.");
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
                    var pointed = ResolveCall(source, method.Identity, instruction.ReadToken(ref reader).Value, offset);
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
                    if (method.StaticSpans.TryGetValue(offset, out var staticSpan))
                    {
                        method.Storage.StaticData.Add(staticSpan);
                        break;
                    }
                    if (method.PrimitiveCalls.TryGetValue(offset, out var primitive))
                    {
                        if (primitive.AddressReceiver) method.Storage.RequireAddress(method.Storage.Bind(primitive.Element.ByReference()));
                        RequirePrimitiveOperation(method, primitive.Element, primitive.Operation);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.ManagedType.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } managedType)
                    {
                        method.Storage.ManagedTypesEnabled = true;
                        method.Storage.ExceptionsEnabled |= managedType.Signature.HasThis;
                        method.RequireGcTemporaries();
                        if (managedType.Signature.Result.Identity is { } typeIdentity) method.RequireGc(typeIdentity);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.StringConcatenation.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is { } concat)
                    {
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
                    if (method.InvariantFormatting.ContainsKey(offset) || instruction.Code == OpCode.Call && IR.Intrinsics.Int32Formatting.Is(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)))
                    {
                        method.Storage.Formatting.UsesInt32 = true;
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ConstructorFactory.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } factoryConstructor)
                    {
                        method.RequireGc(factoryConstructor.DeclaringType);
                        if (!method.Calls.Contains(factoryConstructor)) method.Calls.Add(factoryConstructor);
                        method.Storage.ExceptionsEnabled = method.Storage.InnerExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.TargetInvocationException);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ArrayCopy.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value)) is not null)
                    {
                        method.UsesArrayCopy = true;
                        method.RequireGcTemporaries();
                        method.Storage.ExceptionsEnabled = method.Storage.ArgumentExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentNullException);
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        foreach (var name in new[] { "sourceArray", "destinationArray", "sourceIndex", "destinationIndex", "length" })
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
                        if (math.Operation == "Pow")
                        {
                            method.Storage.MathImports.UsesPow = true;
                            method.UsesNativeFloat |= math.IsSingle;
                        }
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.SpanAccess.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } spanAccess)
                    {
                        method.UsesSpanOperations |= spanAccess.Operation is "Slice" or "CopyTo" or "TryCopyTo";
                        method.Storage.ExceptionsEnabled = true;
                        if (spanAccess.Operation == "get_Item") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.IndexOutOfRangeException);
                        if (spanAccess.Operation == "Slice") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
                        if (spanAccess.Operation == "CopyTo") method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.ArgumentException);
                        method.RequireGcTemporaries();
                        if (spanAccess.Operation is "CopyTo" or "TryCopyTo" && spanAccess.Owner.Arguments!.Values[0].Identity is { IsValueType: true } element)
                            _ = method.Storage.Shape(element);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.InlineArrayAccess.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is not null)
                    {
                        method.Storage.ExceptionsEnabled = true;
                        method.Storage.ExtraExceptionKinds.Add(IR.Exceptions.ExceptionKind.IndexOutOfRangeException);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (method.InterpolationCalls.ContainsKey(offset) || instruction.Code is OpCode.Call or OpCode.Newobj && IR.Intrinsics.InterpolatedStringHandler.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is not null)
                    {
                        method.Storage.Interpolation.Enabled = true;
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
                        method.Storage.ExceptionsEnabled |= textEquality.Signature.HasThis;
                        method.EqualityTemporaries.TryAdd(new(SignatureTypeCode.String), default);
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && IR.Intrinsics.DefaultEquality.Read(source,
                        System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } equality)
                    {
                        if (IR.Intrinsics.DefaultEquality.Is(equality.Owner)) method.RequireGc(equality.Owner);
                        else method.RequireGcTemporaries();
                        if (equality.Operation == "GetHashCode") RequirePrimitiveOperation(method, equality.Element, equality.Operation);
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
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ManagedReference.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } address)
                    {
                        method.Storage.RequireAddress(method.Storage.Bind(address.Element.ByReference()));
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.ReferenceCompareExchange.Read(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } exchange)
                    {
                        _ = method.Storage.Bind(exchange.Result);
                        method.UsesReferenceCompareExchange = true;
                        method.Storage.ReferenceAddressesEnabled = true;
                        method.RequireGcTemporaries();
                        break;
                    }
                    if (instruction.Code == OpCode.Call && IR.Intrinsics.NativeFloat.TryMethod(source, System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle(call.Value), out var nativeOperation, out _))
                    {
                        method.UsesNativeFloat = true;
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
                        method.Storage.ArgumentExceptionsEnabled |= IR.Exceptions.ExceptionTypes.UsesParameterName(exceptionMember);
                        method.Storage.InnerExceptionsEnabled |= exceptionMember == IR.Exceptions.ExceptionMember.InnerException;
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
                    var target = ResolveCall(source, method.Identity, call.Value, offset);
                    if (IR.Intrinsics.DefaultEquality.ForInterface(target) is { } interfaceComparer)
                    {
                        method.Storage.ComparerInterfacesEnabled = true;
                        method.RequireGc(interfaceComparer.Owner);
                        var elementStorage = method.Storage.Bind(interfaceComparer.Element);
                        if (interfaceComparer.Operation == "GetHashCode") RequirePrimitiveOperation(method, interfaceComparer.Element, interfaceComparer.Operation);
                        if (interfaceComparer.Operation == "Equals")
                        {
                            method.EqualityTemporaries.TryAdd(elementStorage, default);
                            method.Storage.StringEqualityEnabled |= elementStorage.TypeCode == SignatureTypeCode.String;
                        }
                    }
                    var attributes = target.DeclaringType.Module.GetMethod(target.Handle).Attributes;
                    if (instruction.Code == OpCode.Callvirt && (attributes & System.Reflection.MethodAttributes.Virtual) != 0)
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
