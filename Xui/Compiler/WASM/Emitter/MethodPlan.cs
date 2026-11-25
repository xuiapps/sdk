using System.Reflection;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Op;
using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>One reachable method's signature, source locals, temporary locals, and module indices.</summary>
internal sealed partial class MethodPlan
{
    public Dictionary<int, LocalIndex> ExceptionLocals { get; } = new();
    public Dictionary<int, LocalIndex> CatchExceptionRefs { get; } = new();
    public LocalIndex UnwindException { get; private set; }
    public IR.MethodIdentity Identity { get; }
    public MethodDefinitionHandle Handle => Identity.Handle;
    public MethodSignature Signature { get; }
    public IR.MethodBody Body { get; }
    /// <summary>Only a signature is needed: an abstract declaration or an unselected default body.</summary>
    public bool IsDeclaration { get; }
    public bool IsDelegateInvoke => IR.DelegateInfo.Is(Identity.DeclaringType) && Identity.DeclaringType.Module.GetMethod(Handle).Name == "Invoke";
    public MemoryFrame? Frame { get; }
    public StorageModel Storage { get; }
    public bool UsesGc { get; private set; }
    public bool UsesReferenceCompareExchange { get; set; }
    public LocalIndex CompareExchangeReplacement { get; private set; }
    public LocalIndex CompareExchangeOriginal { get; private set; }
    public LocalIndex AddressOwner { get; private set; }
    public LocalIndex AddressSelector { get; private set; }
    public LocalIndex SourceOwner { get; private set; }
    public LocalIndex SourceSelector { get; private set; }
    public LocalIndex GcSource { get; private set; }
    public LocalIndex GcOwner { get; private set; }
    public LocalIndex GcI32 { get; private set; }
    public LocalIndex GcI64 { get; private set; }
    public LocalIndex GcF64 { get; private set; }
    public LocalIndex ExceptionParamName { get; private set; }
    public LocalIndex GcExtern { get; private set; }
    public LocalVariable[] Locals { get; }
    public List<IR.MethodIdentity> Calls { get; } = new();
    public IReadOnlyDictionary<int, IR.Intrinsics.InterpolatedStringHandler> InterpolationCalls { get; } = new Dictionary<int, IR.Intrinsics.InterpolatedStringHandler>();
    public IReadOnlyDictionary<int, InvariantFormattingCall> InvariantFormatting { get; } = new Dictionary<int, InvariantFormattingCall>();
    public InstructionGraph Graph { get; }
    public SequenceBlock? Flow { get; }
    public Dictionary<FlowNode, TypeIndex> BlockTypes { get; } = new();
    public Dictionary<SwitchBlock, TypeIndex> SwitchArmTypes { get; } = new();
    public bool UsesSwitch { get; set; }
    public LocalIndex SwitchSelector { get; private set; }
    public bool UsesNativeFloatClamp { get; set; }
    public LocalIndex NativeFloatMin { get; private set; }
    public LocalIndex NativeFloatMax { get; private set; }
    public bool UsesNativeFloat { get; set; }
    public LocalIndex NativeFloatTemporary { get; private set; }
    public bool NeedsDup { get; set; }
    public ValueTypeCode[] LocalTypes { get; private set; } = [];
    public TypeIndex Type { get; set; }
    public FunctionIndex Function { get; set; }
    public Dictionary<IR.MethodIdentity, LocalIndex[]> CallTemporaries { get; } = new();
    public Dictionary<LocalVariable, (LocalIndex Left, LocalIndex Right)> EqualityTemporaries { get; } = new();
    public int StringConcatArgumentCount { get; set; }
    public LocalIndex[] StringConcatArguments { get; private set; } = [];
    public bool UsesArrays { get; set; }
    public bool UsesArrayCopy { get; set; }
    public bool UsesEqualitySearch { get; set; }
    public LocalIndex EqualitySearchEnd { get; private set; }
    public LocalIndex EqualitySearchResult { get; private set; }
    public LocalIndex CopyLength { get; private set; }
    public LocalIndex CopySourceIndex { get; private set; }
    public LocalIndex CopyDestinationIndex { get; private set; }
    public LocalIndex ArrayReference { get; private set; }
    public LocalIndex ArrayIndex { get; private set; }
    public Dictionary<int, LocalIndex> ReferenceLocals { get; } = new();
    public Dictionary<IR.MethodIdentity, LocalIndex[]> CallReferenceTemporaries { get; } = new();
    private uint dupBase;

    public MethodPlan(IR.Module source, IR.MethodIdentity identity, bool entryPoint, StorageModel storage, bool declarationOnly = false)
    {
        Identity = identity;
        var handle = identity.Handle;
        Storage = storage;
        var method = source.GetMethod(handle);
        Signature = MethodSignature.Read(source, method, identity.DeclaringType, entryPoint, storage, identity.Arguments);
        if (!declarationOnly && (method.ImplementationAttributes & MethodImplAttributes.Synchronized) != 0)
            throw new NotSupportedException($"Synchronized methods are not implemented: {method.Name}.");
        var declaringType = source.GetType(method.DeclaringType);
        foreach (var sibling in declaringType.Methods)
            if (!declarationOnly && source.GetMethod(sibling).Name == ".cctor" &&
                (method.Name == ".cctor" || (declaringType.Attributes & TypeAttributes.BeforeFieldInit) == 0))
                throw new NotSupportedException($"Type initialization is not implemented: {declaringType.Name}.");
        IsDeclaration = declarationOnly || (method.Attributes & MethodAttributes.Abstract) != 0;
        if (IsDeclaration)
        {
            Body = null!;
            Locals = [];
            Graph = null!;
            foreach (var parameter in Signature.Parameters.Append(Signature.Result))
                if (parameter.UsesGc) { if (parameter.Identity is { } id) RequireGc(id); else RequireGcTemporaries(); }
            return;
        }
        Body = method.Body ?? throw new NotSupportedException($"The method has no managed body: {method.Name}.");
        if (Body.ExceptionRegions.Length != 0) { storage.ExceptionsEnabled = true; RequireGcTemporaries(); }
        Locals = LocalVariable.Read(source, Body.LocalSignature, storage, identity.DeclaringType.Arguments, identity.Arguments);
        foreach (var local in Locals.Concat(Signature.Parameters).Append(Signature.Result))
            if (local.UsesGc) { if (local.Identity is { } id) RequireGc(id); else RequireGcTemporaries(); }
        if (Locals.Concat(Signature.Parameters).Append(Signature.Result).Any(p => p.IsByReference)) { storage.ReferenceAddressesEnabled = true; RequireGcTemporaries(); }
        Graph = InstructionGraph.Read(Body.Instructions);
        InvariantFormatting = InvariantFormattingCalls.Read(source, Body, Graph);
        InterpolationCalls = XuiNet.CodeFlow.InterpolationCalls.Read(source, Body, Graph, identity.DeclaringType, identity.Arguments);
        if (CallsTimeSpanFactory() || Locals.Any(local => local.Identity is not null) || Signature.Parameters.Any(p => p.Identity is not null) || Signature.Result.Identity is not null ||
            Graph.Instructions.Any(n => n.Code is OpCode.Ldtoken or OpCode.Newobj or OpCode.Ldsfld or OpCode.Ldftn or OpCode.Ldvirtftn || n.Code is >= OpCode.Newarr and <= OpCode.Stelem)) Graph = Analyzer.Analyze(source, identity);
        if (Body.ExceptionRegions.Length != 0)
        {
            Graph = Analyzer.Analyze(source, identity);
            try { Flow = ExceptionBlocks.Build(source, Body, Graph); }
            catch (NotSupportedException error) { AddFlowDiagnostic(error); throw; }
        }
        else if (Graph.Instructions.Any(node => node.Code is OpCode.Switch or OpCode.Throw or OpCode.Rethrow || node.Successors.Length > 1 ||
            node.Code is OpCode.Br or OpCode.Br_s))
        {
            Graph = Analyzer.Analyze(source, identity);
            try { Flow = StructuredBlocks.Build(Graph); }
            catch (NotSupportedException error)
            {
                AddFlowDiagnostic(error);
                throw;
            }
        }
        CollectReferenceVariables();
        if (addressedReferenceLocals.Count + addressedReferenceArguments.Count + addressedScalarLocals.Count + addressedScalarArguments.Count != 0 || Locals.Concat(Signature.Parameters).Any(local => local.Identity is not null && !local.UsesGc) ||
            Graph.Instructions.Any(n => n.After?.Entry is { DeclaredType.Identity: { IsValueType: true } id } entry && !storage.IsGc(id) &&
                (entry.Type == StackType.ValueType || entry.Type == StackType.ManagedPointer && n.Code is OpCode.Call or OpCode.Callvirt)))
        {
            Frame = new(source, Locals, Signature.Parameters, storage);
            foreach (int index in addressedReferenceLocals) Frame.ReferenceLocals.Add(index, Frame.AllocateReference(Locals[index]));
            foreach (int index in addressedReferenceArguments) Frame.ReferenceArguments.Add(index, Frame.AllocateReference(Signature.Parameters[index]));
            foreach (int index in addressedScalarLocals) Frame.ScalarLocals.Add(index, Frame.AllocateScalar(Locals[index]));
            foreach (int index in addressedScalarArguments) Frame.ScalarArguments.Add(index, Frame.AllocateScalar(Signature.Parameters[index]));
            Frame.AllocateValues(Graph);
            if (Frame.References.Count != 0) { storage.ReferenceSlotsEnabled = true; storage.ExceptionsEnabled = true; }
            if (storage.ReferenceSlotsEnabled) RequireGcTemporaries();
        }
    }

    private void AddFlowDiagnostic(NotSupportedException error)
    {
        error.Data["Xui.Method"] = Identity.ToString();
        error.Data["Xui.InstructionGraph"] = string.Join("\n", Graph.Instructions.Select(n => $"{n.Offset:x4} {n.Code} -> {string.Join(", ", n.Successors.Select(s => s.Offset.ToString("x4")))}"));
    }

    public void RequireGcTemporaries() => UsesGc = true;

    public void RequireGc(IR.TypeIdentity identity)
    {
        _ = Storage.Shape(identity);
        UsesGc = true;
    }

    public void AllocateTemporaries(IReadOnlyDictionary<IR.MethodIdentity, MethodPlan> methods)
    {
        var types = Locals.Select(local => local.WasmType).ToList();
        uint parameterCount = (uint)Signature.WasmParameterCount;
        if (Locals.Concat(Signature.Parameters).Append(Signature.Result).Any(v => v.IsByReference) ||
            Calls.Any(call => methods[call].Signature.Parameters.Any(p => p.IsByReference)) ||
            Graph is not null && Graph.Instructions.Any(n => n.Code is OpCode.Ldloca or OpCode.Ldloca_s or OpCode.Ldarga or OpCode.Ldarga_s or OpCode.Ldflda or OpCode.Ldelema or OpCode.Ldobj or OpCode.Stobj or OpCode.Cpobj or OpCode.Initobj ||
                n.After?.Entry?.DeclaredType is { IsByReference: true }))
        {
            UsesGc = true;
            AddressOwner = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            AddressSelector = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            SourceOwner = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            SourceSelector = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
        }
        if (UsesSwitch) { SwitchSelector = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32); }
        if (UsesEqualitySearch)
        {
            EqualitySearchEnd = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            EqualitySearchResult = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
        }
        if (UsesArrayCopy)
        {
            CopyLength = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            CopySourceIndex = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            CopyDestinationIndex = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
        }
        for (int i = 0; i < Locals.Length; i++)
            if (Locals[i].IsByReference)
            {
                ReferenceLocals.Add(i, new(parameterCount + (uint)types.Count));
                types.Add(ValueTypeCode.I32);
            }
        if (UsesNativeFloat) { NativeFloatTemporary = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F32); }
        if (UsesNativeFloatClamp)
        {
            NativeFloatMin = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F32);
            NativeFloatMax = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F32);
        }
        if (NeedsDup)
        {
            dupBase = parameterCount + (uint)types.Count;
            // One reusable scratch slot per evaluation-stack representation, not per dup instruction.
            types.AddRange([ValueTypeCode.I32, ValueTypeCode.I64, ValueTypeCode.F64, ValueTypeCode.ExternRef]);
        }
        foreach (var target in Calls)
        {
            var parameters = methods[target].Signature.Parameters;
            if (methods[target].Signature.Result.UsesGc || parameters.Any(parameter => parameter.UsesGc)) UsesGc = true;
            if (!methods[target].Signature.HasThis && !parameters.Any(parameter => parameter.NeedsNarrowing || parameter.IsByReference || parameter.TypeCode == SignatureTypeCode.String)) continue;
            var slots = new LocalIndex[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                slots[i] = new(parameterCount + (uint)types.Count);
                types.Add(parameters[i].WasmType);
            }
            CallTemporaries.Add(target, slots);
            if (parameters.Any(p => p.IsByReference))
            {
                var selectors = new LocalIndex[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    if (parameters[i].IsByReference) { selectors[i] = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32); }
                CallReferenceTemporaries.Add(target, selectors);
            }
        }
        if (!IsDeclaration) foreach (var region in Body.ExceptionRegions)
        {
            ExceptionLocals[region.HandlerOffset] = new(parameterCount + (uint)types.Count);
            types.Add(region.Kind == ExceptionRegionKind.Catch ? ValueTypeCode.EqRef : ValueTypeCode.ExnRef);
            if (region.Kind == ExceptionRegionKind.Catch)
            {
                CatchExceptionRefs[region.HandlerOffset] = new(parameterCount + (uint)types.Count);
                types.Add(ValueTypeCode.ExnRef);
            }
        }
        if (Frame is not null && Storage.ExceptionsEnabled)
        {
            UnwindException = new(parameterCount + (uint)types.Count);
            types.Add(ValueTypeCode.ExnRef);
        }
        if (Frame is { } frame)
        {
            if (Storage.ReferenceSlotsEnabled)
            {
                frame.ReferenceValue = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
                frame.ReferenceBase = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
                frame.CopySource = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
                frame.CopyDestination = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            }
            frame.Base = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.AddressScratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.I32Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.I64Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I64);
            frame.F64Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F64);
        }
        if (UsesArrays)
        {
            ArrayReference = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            ArrayIndex = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
        }
        if (UsesGc)
        {
            GcSource = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            GcOwner = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            GcI32 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            GcI64 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I64);
            GcF64 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F64);
            GcExtern = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.ExternRef);
            if (Storage.ArgumentExceptionsEnabled) { ExceptionParamName = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.ExternRef); }
        }
        foreach (var category in EqualityTemporaries.Keys.ToArray())
        {
            EqualityTemporaries[category] = (new(parameterCount + (uint)types.Count), new(parameterCount + (uint)types.Count + 1));
            var wasm = category.UsesGc ? ValueTypeCode.EqRef : category.Identity is not null ? ValueTypeCode.I32 : category.StackType switch {
                SignatureTypeCode.String => ValueTypeCode.ExternRef,
                SignatureTypeCode.Double => ValueTypeCode.F64,
                SignatureTypeCode.Int64 => ValueTypeCode.I64,
                _ => ValueTypeCode.I32
            };
            types.Add(wasm); types.Add(wasm);
        }
        if (UsesReferenceCompareExchange)
        {
            CompareExchangeReplacement = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            CompareExchangeOriginal = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
        }
        StringConcatArguments = new LocalIndex[StringConcatArgumentCount];
        for (int i = 0; i < StringConcatArgumentCount; i++)
        {
            StringConcatArguments[i] = new(parameterCount + (uint)types.Count);
            types.Add(ValueTypeCode.ExternRef);
        }
        LocalTypes = types.ToArray();
    }

    public LocalIndex DupTemporary(SignatureTypeCode type) => new(dupBase + (type switch
    {
        SignatureTypeCode.Int32 or SignatureTypeCode.IntPtr => 0u,
        SignatureTypeCode.Int64 => 1u,
        SignatureTypeCode.Double => 2u,
        SignatureTypeCode.String => 3u,
        _ => throw new NotSupportedException($"Unsupported dup type: {type}.")
    }));
}
