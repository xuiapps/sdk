using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.Layout;
using Xui.Compiler.WASM.Wasm.Indices;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Aligned linear-memory slots for struct locals and immutable evaluation-stack copies.</summary>
/// <remarks>One frame per invocation. Struct values cannot cross loop boundaries yet, so a load's
/// temporary slot can be reused on later iterations without overwriting a live carried value.</remarks>
internal sealed class MemoryFrame
{
    public const int PageCount = 16;
    public const int Capacity = PageCount * 65536;
    public const int Start = 16;
    private readonly Model.Module module;
    private readonly LayoutResolver layouts;
    private readonly StorageModel storage;
    public int[] Locals { get; }
    public Dictionary<int, int> Values { get; } = new();
    public int Size { get; private set; }
    public LocalIndex Base { get; set; }
    public LocalIndex AddressScratch { get; set; }
    public LocalIndex I32Scratch { get; set; }
    public LocalIndex I64Scratch { get; set; }
    public LocalIndex F64Scratch { get; set; }

    public MemoryFrame(Model.Module module, LocalVariable[] locals, StorageModel storage)
    {
        this.module = module;
        this.storage = storage;
        layouts = new(module, TargetLayout.Wasm32);
        Locals = new int[locals.Length];
        Array.Fill(Locals, -1);
        for (int i = 0; i < locals.Length; i++)
            if (!locals[i].UsesGc && locals[i].Identity is { } identity) Locals[i] = Allocate(identity);
    }

    public StructLayout Layout(TypeIdentity identity)
    {
        if (identity.Module != module || identity.Handle.Kind != HandleKind.TypeDefinition)
            throw new NotSupportedException($"External struct storage is not implemented: {identity}.");
        var handle = (TypeDefinitionHandle)identity.Handle;
        if (layouts.Classify(handle) != StorageKind.LinearMemory)
            throw new NotSupportedException($"Linear-memory storage requires a struct without managed references: {identity}.");
        return layouts.Resolve(handle);
    }

    public int ValueSize(TypeIdentity identity)
    {
        // NFloat's target-dependent scalar representation is already used by LayoutResolver.
        if (IsNativeFloat(identity)) return TargetLayout.Wasm32.NativeFloatSize;
        return Layout(identity).Size;
    }

    internal static bool IsNativeFloat(TypeIdentity identity)
    {
        if (identity.Handle.Kind != HandleKind.TypeReference) return false;
        var type = identity.Module.GetTypeReference((TypeReferenceHandle)identity.Handle);
        if (type.Namespace != "System.Runtime.InteropServices" || type.Name != "NFloat" ||
            type.ResolutionScope.Kind != HandleKind.AssemblyReference) return false;
        return identity.Module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name
            is "System.Private.CoreLib" or "System.Runtime" or "mscorlib";
    }

    public void AllocateValues(InstructionGraph graph)
    {
        foreach (var node in graph.Instructions)
        {
            if (node.After?.Entry is not { Type: StackType.ValueType, DeclaredType: { Identity: { } identity } }) continue;
            if (storage.IsGc(identity)) continue;
            if (node.Code is OpCode.Ldloc or OpCode.Ldloc_s or OpCode.Ldloc_0 or OpCode.Ldloc_1 or
                OpCode.Ldloc_2 or OpCode.Ldloc_3 or OpCode.Ldfld or OpCode.Ldobj or OpCode.Dup)
                Values.Add(node.Offset, Allocate(identity));
        }
        Size = Align(Size, 16);
        if (Size > Capacity - Start) throw new NotSupportedException("Method frame exceeds the linear-memory stack capacity.");
    }

    private int Allocate(TypeIdentity identity)
    {
        int size = ValueSize(identity);
        int alignment = IsNativeFloat(identity) ? TargetLayout.Wasm32.NativeFloatSize : Layout(identity).Alignment;
        int offset = Align(Size, alignment);
        Size = checked(offset + size);
        return offset;
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
