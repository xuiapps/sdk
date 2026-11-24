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
    private readonly StorageModel storage;
    public int[] Locals { get; }
    public int[] Arguments { get; }
    public Dictionary<int, int> Values { get; } = new();
    public int Size { get; private set; }
    public List<(int Offset, LocalVariable Type)> References { get; } = new();
    public LocalIndex ReferenceBase { get; set; }
    public LocalIndex ReferenceValue { get; set; }
    public LocalIndex CopySource { get; set; }
    public LocalIndex CopyDestination { get; set; }
    public LocalIndex Base { get; set; }
    public LocalIndex AddressScratch { get; set; }
    public LocalIndex I32Scratch { get; set; }
    public LocalIndex I64Scratch { get; set; }
    public LocalIndex F64Scratch { get; set; }

    public MemoryFrame(Model.Module module, LocalVariable[] locals, LocalVariable[] parameters, StorageModel storage)
    {
        this.storage = storage;
        Arguments = new int[parameters.Length];
        Array.Fill(Arguments, -1);
        for (int i = 0; i < parameters.Length; i++)
            if (parameters[i].IsLinearValue) Arguments[i] = Allocate(parameters[i].Identity!.Value);
        Locals = new int[locals.Length];
        Array.Fill(Locals, -1);
        for (int i = 0; i < locals.Length; i++)
            if (!locals[i].UsesGc && !locals[i].IsByReference && locals[i].Identity is { } identity) Locals[i] = Allocate(identity);
    }

    public StackStructLayout Layout(TypeIdentity identity) => storage.StackLayout(identity);

    public int ValueSize(TypeIdentity identity)
    {
        // NFloat's target-dependent scalar representation is already used by LayoutResolver.
        if (IsNativeFloat(identity)) return TargetLayout.Wasm32.NativeFloatSize;
        return Layout(identity).Size;
    }

    internal static bool IsNativeFloat(TypeIdentity identity) => XuiNet.DotNet.Intrinsics.NativeFloat.Is(identity);

    public void AllocateValues(InstructionGraph graph)
    {
        foreach (var node in graph.Instructions)
        {
            if (node.After?.Entry is not { Type: StackType.ValueType, DeclaredType: { Identity: { } identity } }) continue;
            if (storage.IsGc(identity)) continue;
            // A void call can expose a struct already below its arguments. It needs
            // no result slot; only instructions that actually push a value do.
            if (node.Lowest is null || node.After.Depth <= node.Lowest.Depth) continue;
            if (node.Code is OpCode.Ldloc or OpCode.Ldloc_s or OpCode.Ldloc_0 or OpCode.Ldloc_1 or
                OpCode.Ldloc_2 or OpCode.Ldloc_3 or OpCode.Ldfld or OpCode.Ldobj or OpCode.Dup or OpCode.Newobj or
                OpCode.Ldarg or OpCode.Ldarg_s or OpCode.Ldarg_0 or OpCode.Ldarg_1 or OpCode.Ldarg_2 or OpCode.Ldarg_3 or
                OpCode.Call or OpCode.Callvirt)
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
        if (storage.HasStackReferences(identity))
            foreach (var leaf in storage.Shape(identity).Leaves)
                if (StorageModel.IsReference(leaf.Storage)) References.Add((offset + leaf.MemoryOffset, leaf.Storage));
        return offset;
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
