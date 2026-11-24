using System.Reflection;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

internal sealed partial class DispatchTable
{
    private static MethodIdentity DefaultInterfaceImplementation(TypeIdentity type, MethodIdentity declaration)
    {
        var candidates = new HashSet<MethodIdentity> { declaration };
        foreach (var contract in type.Interfaces())
        {
            if (contract != declaration.DeclaringType && !contract.Interfaces().Contains(declaration.DeclaringType)) continue;
            var module = contract.Module;
            var definition = module.GetType((System.Reflection.Metadata.TypeDefinitionHandle)contract.Handle);
            foreach (var handle in definition.MethodImplementations)
            {
                var mapping = module.GetMethodImplementation(handle);
                if (MemberResolver.Method(module, mapping.Declaration, contract) == declaration)
                    candidates.Add(MemberResolver.Method(module, mapping.Body, contract));
            }
        }
        // Abstract overrides participate too: reabstraction must suppress a less-specific body.
        var mostSpecific = candidates.Where(candidate => !candidates.Any(other =>
            other.DeclaringType != candidate.DeclaringType && other.DeclaringType.Interfaces().Contains(candidate.DeclaringType))).ToArray();
        if (mostSpecific.Length != 1)
            throw new NotSupportedException($"Ambiguous default interface implementation of {declaration} on {type}: {string.Join(", ", mostSpecific.Select(m => m.ToString()))}.");
        var selected = mostSpecific[0];
        var method = selected.DeclaringType.Module.GetMethod(selected.Handle);
        if ((method.Attributes & MethodAttributes.Abstract) != 0)
            throw new NotSupportedException($"No concrete interface implementation of {declaration} on {type}; most-specific member {selected} is abstract.");
        return selected;
    }
}
