using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    private readonly Dictionary<TypeIdentity, (TypeIdentity Receiver, MethodIdentity Method)[]> equalityTargets = new();

    public (TypeIdentity Receiver, MethodIdentity Method)[] EqualityTargets(TypeIdentity type)
    {
        if (equalityTargets.TryGetValue(type, out var targets)) return targets;
        var result = new List<(TypeIdentity, MethodIdentity)>();
        foreach (var node in dependencies.Nodes.OfType<Dependency.Instantiation>())
            if (!node.Identity.IsValueType && node.Identity.IsAssignableTo(type) &&
                ObjectEquality.Implementation(dependencies.VirtualSlots(node.Identity)) is { } method)
                result.Add((node.Identity, method));
        return equalityTargets[type] = result.ToArray();
    }
}
