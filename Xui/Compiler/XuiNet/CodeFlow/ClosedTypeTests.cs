using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Proves interface/class disjointness within the fixed compilation's class hierarchy.</summary>
/// <remarks>Open generic descendants and unresolved bases prevent proof. This assumes no dynamic assembly loading.</remarks>
internal sealed class ClosedTypeTests
{
    private static readonly ConditionalWeakTable<Compilation, ClosedTypeTests> Cache = new();
    private readonly Dictionary<TypeIdentity, List<TypeIdentity>> children = new();
    private readonly Dictionary<(TypeIdentity, TypeIdentity), bool> results = new();
    private bool complete = true;

    private ClosedTypeTests(Compilation compilation)
    {
        foreach (var assembly in compilation.Assemblies)
        {
            var module = assembly.ManifestModule;
            foreach (var handle in module.Types)
            {
                var definition = module.GetType(handle);
                if (definition.IsInterface || definition.BaseType.IsNil) continue;
                var parent = definition.BaseType;
                // Only the generic definition matters for ancestry. Avoid decoding unrelated field/signature types.
                if (parent.Kind == HandleKind.TypeSpecification)
                {
                    var blob = module.Metadata.GetBlobReader(module.Metadata.GetTypeSpecification((TypeSpecificationHandle)parent).Signature);
                    if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance || blob.ReadSignatureTypeCode() != SignatureTypeCode.TypeHandle)
                    { complete = false; continue; }
                    parent = blob.ReadTypeHandle();
                }
                var identity = module.ResolveType(parent);
                if (identity.Handle.Kind != HandleKind.TypeDefinition) { complete = false; continue; }
                if (!children.TryGetValue(identity, out var descendants)) children.Add(identity, descendants = new());
                descendants.Add(new(module, handle));
            }
        }
    }

    public static bool Disjoint(TypeIdentity contract, TypeIdentity target)
    {
        if (contract.Module.Compilation is not { } compilation || target.Module.Compilation != compilation ||
            target.Handle.Kind != HandleKind.TypeDefinition || target.IsInterface || target.IsValueType ||
            target.Module.GetType((TypeDefinitionHandle)target.Handle).GenericParameterCount != 0) return false;
        var cache = Cache.GetValue(compilation, static c => new(c));
        if (!cache.complete) return false;
        if (cache.results.TryGetValue((contract, target), out bool result)) return result;
        var pending = new Stack<TypeIdentity>(); pending.Push(target);
        var visited = new HashSet<TypeIdentity>();
        result = true;
        while (pending.TryPop(out var candidate))
        {
            if (!visited.Add(candidate)) continue;
            if (candidate.Module.GetType((TypeDefinitionHandle)candidate.Handle).GenericParameterCount != 0 || candidate.IsAssignableTo(contract))
            { result = false; break; }
            if (cache.children.TryGetValue(candidate, out var descendants))
                foreach (var child in descendants) pending.Push(child);
        }
        cache.results.Add((contract, target), result);
        return result;
    }
}
