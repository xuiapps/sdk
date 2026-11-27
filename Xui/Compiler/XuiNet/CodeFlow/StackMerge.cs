using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Interns a common class-reference stack at joins; value types and managed addresses remain exact.</summary>
internal static class StackMerge
{
    public static StackState Merge(StackState existing, StackState incoming)
    {
        if (ReferenceEquals(existing, incoming)) return existing;
        if (existing.Depth != incoming.Depth) throw new InvalidProgramException("Stack merge depths differ.");
        if (existing.Parent is null || incoming.Parent is null) throw new InvalidProgramException("Stack roots differ.");
        var parent = Merge(existing.Parent, incoming.Parent);
        return parent.Push(Merge(existing.Entry!.Value, incoming.Entry!.Value));
    }

    private static bool ArrayToReference(StackEntry a, StackEntry b) => a.Type == StackType.ArrayReference && b.Type == StackType.ObjectReference &&
        (b.DeclaredType?.Code == SignatureTypeCode.Object || a.DeclaredType?.Array is { } array && b.DeclaredType?.Identity is { } contract &&
         DotNet.Intrinsics.ArrayInterface.Matches(array, contract));

    private static StackEntry Merge(StackEntry a, StackEntry b)
    {
        if (a == b) return a;
        if (ArrayToReference(a, b)) return b;
        if (ArrayToReference(b, a)) return a;
        if (a.Type == StackType.Null && b.Type is StackType.ObjectReference or StackType.ArrayReference or StackType.String) return b;
        if (b.Type == StackType.Null && a.Type is StackType.ObjectReference or StackType.ArrayReference or StackType.String) return a;
        if (a.Type == StackType.ObjectReference && b.Type == StackType.ObjectReference)
        {
            if (a.DeclaredType?.Code == SignatureTypeCode.Object) return a;
            if (b.DeclaredType?.Code == SignatureTypeCode.Object) return b;
        }
        if (a.Type == StackType.ObjectReference && b.Type == StackType.ObjectReference &&
            a.DeclaredType?.Identity is { } left && b.DeclaredType?.Identity is { } right)
        {
            if (left.IsAssignableTo(right)) return b;
            var visited = new HashSet<TypeIdentity>();
            while (left.Handle.Kind == HandleKind.TypeDefinition && visited.Add(left))
            {
                if (right.IsAssignableTo(left)) return StackEntry.From(a.DeclaredType.Value with { Identity = left });
                if (left.BaseType is not { } parent) break;
                left = parent;
            }
        }
        throw new InvalidProgramException($"Stack merge has incompatible types {a} and {b}.");
    }
}
