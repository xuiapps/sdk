using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

internal sealed partial class DispatchTable
{
    // Each closed interface method already has its own dispatch slot. Resolve its class
    // override in template space, then carry the method arguments to the selected body.
    private static MethodIdentity GenericInterfaceOverride(TypeIdentity receiver, MethodIdentity implementation)
    {
        var derived = new Stack<TypeIdentity>();
        for (var type = receiver; type != implementation.DeclaringType;)
        {
            derived.Push(type);
            type = type.BaseType ?? throw new InvalidProgramException("Interface implementation is not on the receiver's base chain.");
        }
        var slots = new List<MethodIdentity> { implementation };
        while (derived.TryPop(out var type))
        {
            foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Methods)
            {
                var method = type.Module.GetMethod(handle);
                if ((method.Attributes & MethodAttributes.Virtual) == 0 || method.GenericParameters.Count == 0) continue;
                int slot = -1;
                for (int i = slots.Count - 1; i >= 0; i--)
                    if (Matches(new(type, handle), slots[i])) { slot = i; break; }
                if (slot < 0) continue;
                var closed = new MethodIdentity(type, handle, implementation.Arguments);
                if ((method.Attributes & MethodAttributes.NewSlot) != 0) { slots.Add(closed); continue; }
                var previous = slots[slot].DeclaringType.Module.GetMethod(slots[slot].Handle);
                if ((previous.Attributes & MethodAttributes.Final) != 0) throw new BadImageFormatException("Cannot override a final generic method.");
                slots[slot] = closed;
            }
        }
        return slots[0];
    }
}
