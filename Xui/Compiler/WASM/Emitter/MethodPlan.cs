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
    public IR.MethodIdentity Identity { get; }
    public MethodDefinitionHandle Handle => Identity.Handle;
    public MethodSignature Signature { get; }
    public IR.MethodBody Body { get; }
    public bool IsAbstract { get; }
    public MemoryFrame? Frame { get; }
    public StorageModel Storage { get; }
    public bool UsesGc { get; private set; }
    public LocalIndex GcSource { get; private set; }
    public LocalIndex GcOwner { get; private set; }
    public LocalIndex GcI32 { get; private set; }
    public LocalIndex GcI64 { get; private set; }
    public LocalIndex GcF64 { get; private set; }
    public LocalIndex GcExtern { get; private set; }
    public LocalVariable[] Locals { get; }
    public List<IR.MethodIdentity> Calls { get; } = new();
    public InstructionGraph Graph { get; }
    public SequenceBlock? Flow { get; }
    public Dictionary<FlowNode, TypeIndex> BlockTypes { get; } = new();
    public bool NeedsDup { get; set; }
    public ValueTypeCode[] LocalTypes { get; private set; } = [];
    public TypeIndex Type { get; set; }
    public FunctionIndex Function { get; set; }
    public Dictionary<IR.MethodIdentity, LocalIndex[]> CallTemporaries { get; } = new();
    private uint dupBase;

    public MethodPlan(IR.Module source, IR.MethodIdentity identity, bool entryPoint, StorageModel storage)
    {
        Identity = identity;
        var handle = identity.Handle;
        Storage = storage;
        var method = source.GetMethod(handle);
        Signature = MethodSignature.Read(source, method, identity.DeclaringType, entryPoint, storage);
        if ((method.ImplementationAttributes & MethodImplAttributes.Synchronized) != 0)
            throw new NotSupportedException($"Synchronized methods are not implemented: {method.Name}.");
        var declaringType = source.GetType(method.DeclaringType);
        foreach (var sibling in declaringType.Methods)
            if (source.GetMethod(sibling).Name == ".cctor")
                throw new NotSupportedException($"Type initialization is not implemented: {declaringType.Name}.");
        IsAbstract = (method.Attributes & MethodAttributes.Abstract) != 0;
        if (IsAbstract)
        {
            Body = null!;
            Locals = [];
            Graph = null!;
            foreach (var parameter in Signature.Parameters.Append(Signature.Result))
                if (parameter.UsesGc) { if (parameter.Identity is { } id) RequireGc(id); else RequireGcTemporaries(); }
            return;
        }
        Body = method.Body ?? throw new NotSupportedException($"The method has no managed body: {method.Name}.");
        if (Body.ExceptionRegions.Length != 0)
            throw new NotSupportedException("WASM exception handling is not implemented.");
        Locals = LocalVariable.Read(source, Body.LocalSignature, storage, identity.DeclaringType.Arguments);
        foreach (var local in Locals.Concat(Signature.Parameters).Append(Signature.Result))
            if (local.UsesGc) { if (local.Identity is { } id) RequireGc(id); else RequireGcTemporaries(); }
        Graph = InstructionGraph.Read(Body.Instructions);
        if (Locals.Any(local => local.Identity is not null) || Signature.Parameters.Any(p => p.Identity is not null) ||
            Graph.Instructions.Any(n => n.Code == OpCode.Newobj)) Graph = Analyzer.Analyze(source, identity);
        if (Graph.Instructions.Any(node => node.Successors.Length > 1 ||
            node.Code is OpCode.Br or OpCode.Br_s))
        {
            Graph = Analyzer.Analyze(source, identity);
            Flow = StructuredBlocks.Build(Graph);
        }
        if (Locals.Any(local => local.Identity is not null && !local.UsesGc) ||
            Graph.Instructions.Any(n => n.After?.Entry is { Type: StackType.ValueType, DeclaredType.Identity: { } id } && !storage.IsGc(id)))
        {
            Frame = new(source, Locals, storage);
            Frame.AllocateValues(Graph);
        }
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
        uint parameterCount = (uint)Signature.Parameters.Length;
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
            if (!methods[target].Signature.HasThis && !parameters.Any(parameter => parameter.NeedsNarrowing)) continue;
            var slots = new LocalIndex[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                slots[i] = new(parameterCount + (uint)types.Count);
                types.Add(parameters[i].WasmType);
            }
            CallTemporaries.Add(target, slots);
        }
        if (Frame is { } frame)
        {
            frame.Base = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.AddressScratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.I32Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            frame.I64Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I64);
            frame.F64Scratch = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F64);
        }
        if (UsesGc)
        {
            GcSource = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            GcOwner = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.EqRef);
            GcI32 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I32);
            GcI64 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.I64);
            GcF64 = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.F64);
            GcExtern = new(parameterCount + (uint)types.Count); types.Add(ValueTypeCode.ExternRef);
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
