using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class MethodPlan
{
    public Dictionary<(int Depth, ValueTypeCode Type), LocalIndex> MergeLocals { get; } = new();
    public Dictionary<InstructionNode, TypeIndex> MergeBranchTypes { get; } = new();

    public static bool NeedsStringMerge(StackState? source, StackState? target)
    {
        while (source?.Entry is { } from && target?.Entry is { } to)
        {
            if (from.Type == StackType.Null && to.Type == StackType.String) return true;
            source = source.Parent; target = target.Parent;
        }
        return false;
    }

    private void AllocateMergeLocals(List<ValueTypeCode> types, uint parameterCount)
    {
        if (Graph is null || !Graph.Instructions.Any(n => n.Successors.Any(s => NeedsStringMerge(n.After,s.Before)))) return;
        // Only methods crossing the eqref/externref boundary need spills. A byref
        // uses the eqref and i32 locals at its depth, preserving both components.
        int depth = Graph.Instructions.Max(n => n.After?.Depth ?? 0);
        for (int i=0;i<depth;i++)
            foreach (var type in new[] { ValueTypeCode.I32, ValueTypeCode.I64, ValueTypeCode.F64, ValueTypeCode.EqRef, ValueTypeCode.ExternRef })
            {
                MergeLocals.Add((i,type), new(parameterCount+(uint)types.Count));
                types.Add(type);
            }
    }
}
