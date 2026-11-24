using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Resolves class slots and interface mappings before backend indices are assigned.</summary>
internal sealed partial class DispatchTable
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
            if (identity.Arguments?.ContainsGenericParameters == true ||
                definition.GenericParameterCount != (identity.Arguments?.Values.Length ?? 0))
                throw new NotSupportedException("Interface and open generic runtime types are not implemented.");
            if (definition.IsInterface || DelegateInfo.Is(identity) || FrameworkTypes.Is(identity, "System", "Object"))
            {
                Types.Add(identity, Array.Empty<VirtualSlot>());
                return Types[identity];
            }
            foreach (var mapping in definition.MethodImplementations)
            {
                var declaration = MemberResolver.Method(module, module.GetMethodImplementation(mapping).Declaration, identity);
                if (!declaration.DeclaringType.IsInterface)
                    throw new NotSupportedException("Explicit class method implementations and covariant overrides are not implemented.");
            }
            var slots = new List<VirtualSlot>();
            if (identity.BaseType is { Handle.Kind: HandleKind.TypeDefinition } parent && !FrameworkTypes.Is(parent, "System", "Object"))
            {
                slots.AddRange(Resolve(parent));
                if ((parent.Module.GetType((TypeDefinitionHandle)parent.Handle).Attributes & TypeAttributes.Sealed) != 0)
                    throw new BadImageFormatException("Cannot derive from a sealed class.");
            }
            else if (identity.BaseType is { } root && FrameworkTypes.Is(root, "System", "Object")) { }
            else if (definition.BaseType.Kind == HandleKind.TypeReference)
            {
                var reference = module.GetTypeReference((TypeReferenceHandle)definition.BaseType);
                if (reference.Namespace != "System" || reference.Name != "Object")
                    throw new NotSupportedException("External class inheritance is not implemented.");
            }
            else throw new NotSupportedException($"Unsupported class base type: {identity}.");
            foreach (var handle in definition.Methods)
            {
                var method = module.GetMethod(handle);
                if ((method.Attributes & MethodAttributes.Virtual) == 0) continue;
                // Unsupported generic methods must not root otherwise unused bodies.
                // A reachable generic call still rejects when its signature is read.
                if (method.GenericParameters.Count != 0) continue;
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
                    if ((slots[slot].Implementation.DeclaringType.Module.GetMethod(slots[slot].Implementation.Handle).Attributes & MethodAttributes.Final) != 0)
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
        var right = b.DeclaringType.Module.GetMethod(b.Handle);
        if (left.Name != right.Name) return false;
        // Match templates first: F(T) and F(int) must not collapse when T becomes int.
        var parent = a.DeclaringType.GenericDefinition.BaseType;
        while (parent is { } current && current.GenericDefinition != b.DeclaringType.GenericDefinition) parent = current.BaseType;
        if (parent is not { } owner) return false;
        var x = SignatureReader.Method(module, left.Signature);
        var y = SignatureReader.Method(b.DeclaringType.Module, right.Signature, owner.Arguments);
        return x.Result == y.Result && x.Parameters.SequenceEqual(y.Parameters);
    }
}
