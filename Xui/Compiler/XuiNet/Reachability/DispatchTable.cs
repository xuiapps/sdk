using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Resolves inherited class slots before any backend indices are assigned.</summary>
internal sealed class DispatchTable
{
    public Dictionary<TypeIdentity, IReadOnlyList<VirtualSlot>> Types { get; } = new();
    private readonly HashSet<TypeIdentity> resolving = new();

    public IReadOnlyList<VirtualSlot> Resolve(TypeIdentity identity)
    {
        if (Types.TryGetValue(identity, out var found)) return found;
        var module = identity.Module;
        if (identity.Handle.Kind != HandleKind.TypeDefinition || identity.IsValueType)
            throw new NotSupportedException($"Class dispatch requires a resolved class: {identity}.");
        if (!resolving.Add(identity)) throw new BadImageFormatException("Cyclic class inheritance.");
        try
        {
            var definition = module.GetType((TypeDefinitionHandle)identity.Handle);
            if (definition.IsInterface || identity.Arguments?.ContainsGenericParameters == true ||
                definition.GenericParameterCount != (identity.Arguments?.Values.Length ?? 0))
                throw new NotSupportedException("Interface and open generic runtime types are not implemented.");
            if (definition.MethodImplementations.Count != 0)
                throw new NotSupportedException("Explicit method implementations and covariant overrides are not implemented.");
            var slots = new List<VirtualSlot>();
            if (identity.BaseType is { Handle.Kind: HandleKind.TypeDefinition } parent)
            {
                slots.AddRange(Resolve(parent));
                if ((module.GetType((TypeDefinitionHandle)parent.Handle).Attributes & TypeAttributes.Sealed) != 0)
                    throw new BadImageFormatException("Cannot derive from a sealed class.");
            }
            else if (definition.BaseType.Kind == HandleKind.TypeReference)
            {
                var reference = module.GetTypeReference((TypeReferenceHandle)definition.BaseType);
                if (reference.Namespace != "System" || reference.Name != "Object")
                    throw new NotSupportedException("External class inheritance is not implemented.");
            }
            else throw new NotSupportedException("Unsupported class base type.");
            foreach (var handle in definition.Methods)
            {
                var method = module.GetMethod(handle);
                if ((method.Attributes & MethodAttributes.Virtual) == 0) continue;
                var specialized = new MethodIdentity(identity, handle);
                if (!SignatureReader.Method(module, method.Signature, identity.Arguments).HasThis)
                    throw new BadImageFormatException("Virtual methods require an instance receiver.");
                int slot = -1;
                if ((method.Attributes & MethodAttributes.NewSlot) == 0)
                    for (int i = slots.Count - 1; i >= 0; i--)
                        if (Matches(specialized, slots[i].Implementation)) { slot = i; break; }
                if (slot < 0) slots.Add(new((uint)slots.Count, specialized, specialized));
                else
                {
                    if ((module.GetMethod(slots[slot].Implementation.Handle).Attributes & MethodAttributes.Final) != 0)
                        throw new BadImageFormatException("Cannot override a final virtual method.");
                    slots[slot] = slots[slot] with { Implementation = specialized };
                }
            }
            var result = slots.AsReadOnly();
            Types.Add(identity, result);
            return result;
        }
        finally { resolving.Remove(identity); }
    }

    public uint Slot(MethodIdentity method)
    {
        foreach (var slot in Resolve(method.DeclaringType))
            if (slot.Implementation == method || slot.Declaration == method) return slot.Index;
        throw new NotSupportedException($"No virtual slot for {method}.");
    }

    private static bool Matches(MethodIdentity a, MethodIdentity b)
    {
        var module = a.DeclaringType.Module;
        var left = module.GetMethod(a.Handle);
        var right = module.GetMethod(b.Handle);
        if (left.Name != right.Name) return false;
        // Match templates first: F(T) and F(int) must not collapse when T becomes int.
        var parent = a.DeclaringType.GenericDefinition.BaseType;
        while (parent is { } current && current.Handle != b.DeclaringType.Handle) parent = current.BaseType;
        if (parent is not { } owner) return false;
        var x = SignatureReader.Method(module, left.Signature);
        var y = SignatureReader.Method(module, right.Signature, owner.Arguments);
        return x.Result == y.Result && x.Parameters.SequenceEqual(y.Parameters);
    }
}
