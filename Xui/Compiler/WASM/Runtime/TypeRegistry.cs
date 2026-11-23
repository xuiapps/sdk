using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Resolves same-module class identities and specialized class virtual slots.</summary>
internal sealed class TypeRegistry(Model.Module module)
{
    private readonly Dictionary<TypeIdentity, Type> types = new();
    private readonly HashSet<TypeIdentity> resolving = new();
    public IReadOnlyCollection<Type> Types => types.Values;
    public HashSet<TypeIdentity> Instantiated { get; } = new();
    public HashSet<MethodIdentity> VirtualCalls { get; } = new();

    public Type Resolve(TypeIdentity identity)
    {
        if (types.TryGetValue(identity, out var found)) return found;
        if (identity.Module != module || identity.Handle.Kind != HandleKind.TypeDefinition || identity.IsValueType)
            throw new NotSupportedException($"Runtime class metadata requires a same-module class: {identity}.");
        if (!resolving.Add(identity)) throw new BadImageFormatException("Cyclic class inheritance.");
        try
        {
            var definition = module.GetType((TypeDefinitionHandle)identity.Handle);
            if (definition.IsInterface || identity.Arguments?.ContainsGenericParameters == true ||
                definition.GenericParameterCount != (identity.Arguments?.Values.Length ?? 0))
                throw new NotSupportedException("Interface and open generic runtime types are not implemented.");
            if (definition.MethodImplementations.Count != 0)
                throw new NotSupportedException("Explicit method implementations and covariant overrides are not implemented.");
            Type? parent = null;
            var parentIdentity = identity.BaseType;
            if (parentIdentity is { Handle.Kind: HandleKind.TypeDefinition } baseIdentity)
            {
                parent = Resolve(baseIdentity);
                if ((module.GetType((TypeDefinitionHandle)parent.Identity.Handle).Attributes & TypeAttributes.Sealed) != 0)
                    throw new BadImageFormatException("Cannot derive from a sealed class.");
            }
            else if (definition.BaseType.Kind == HandleKind.TypeReference)
            {
                var reference = module.GetTypeReference((TypeReferenceHandle)definition.BaseType);
                if (reference.Namespace != "System" || reference.Name != "Object")
                    throw new NotSupportedException("External class inheritance is not implemented.");
            }
            else throw new NotSupportedException("Unsupported class base type.");
            var slots = parent?.VirtualSlots.ToList() ?? new List<VirtualSlot>();
            foreach (var handle in definition.Methods)
            {
                var method = module.GetMethod(handle);
                if ((method.Attributes & MethodAttributes.Virtual) == 0) continue;
                var specialized = new MethodIdentity(identity, handle);
                var signature = SignatureReader.Method(module, method.Signature, identity.Arguments);
                if (!signature.HasThis) throw new BadImageFormatException("Virtual methods require an instance receiver.");
                int slot = -1;
                if ((method.Attributes & MethodAttributes.NewSlot) == 0)
                    for (int i = slots.Count - 1; i >= 0; i--)
                        if (Matches(specialized, slots[i].Implementation)) { slot = i; break; }
                if (slot < 0) slots.Add(new((uint)slots.Count, specialized, specialized));
                else
                {
                    var overridden = module.GetMethod(slots[slot].Implementation.Handle);
                    if ((overridden.Attributes & MethodAttributes.Final) != 0)
                        throw new BadImageFormatException("Cannot override a final virtual method.");
                    slots[slot] = slots[slot] with { Implementation = specialized };
                }
            }
            var type = new Type((uint)types.Count + 1, identity, parent,
                (definition.Attributes & TypeAttributes.Abstract) != 0, slots.AsReadOnly());
            types.Add(identity, type);
            return type;
        }
        finally { resolving.Remove(identity); }
    }

    public uint Slot(MethodIdentity handle)
    {
        var method = module.GetMethod(handle.Handle);
        var type = Resolve(handle.DeclaringType);
        foreach (var slot in type.VirtualSlots)
            if (slot.Implementation == handle || slot.Declaration == handle) return slot.Index;
        throw new NotSupportedException($"No virtual slot for {method.Name}.");
    }

    public IEnumerable<MethodIdentity> RequiredImplementations()
    {
        foreach (var identity in Instantiated)
        {
            var type = Resolve(identity);
            foreach (var target in VirtualCalls)
            {
                var owner = target.DeclaringType;
                if (identity.IsAssignableTo(owner)) yield return type.VirtualSlots[(int)Slot(target)].Implementation;
            }
        }
    }

    private bool Matches(MethodIdentity a, MethodIdentity b)
    {
        var left = module.GetMethod(a.Handle); var right = module.GetMethod(b.Handle);
        if (left.Name != right.Name) return false;
        // Match in the overriding declaration's open context. Specializing first would
        // collapse distinct slots such as F(T) and F(int) when T happens to be int.
        var parent = a.DeclaringType.GenericDefinition.BaseType;
        while (parent is { } current && current.Handle != b.DeclaringType.Handle)
            parent = current.BaseType;
        if (parent is not { } owner) return false;
        var x = SignatureReader.Method(module, left.Signature);
        var y = SignatureReader.Method(module, right.Signature, owner.Arguments);
        return x.Result == y.Result && x.Parameters.SequenceEqual(y.Parameters);
    }
}
