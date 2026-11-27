using System.Reflection;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

internal sealed partial class DispatchTable
{
    public Dictionary<(TypeIdentity Type, MethodIdentity Method), MethodIdentity> GenericVirtualImplementations { get; } = new();

    public MethodIdentity GenericVirtualImplementation(TypeIdentity receiver, MethodIdentity method)
    {
        if (GenericVirtualImplementations.TryGetValue((receiver, method), out var found)) return found;
        var definition = method.DeclaringType.Module.GetMethod(method.Handle);
        if (method.DeclaringType.IsInterface || method.Arguments is null || method.Arguments.ContainsGenericParameters ||
            definition.GenericParameters.Count != method.Arguments.Values.Length ||
            (definition.Attributes & MethodAttributes.Virtual) == 0 || !receiver.IsAssignableTo(method.DeclaringType))
            throw new InvalidProgramException($"Expected a closed generic class virtual call: {method} on {receiver}.");
        _ = Resolve(receiver);
        var implementation = GenericOverride(receiver, method);
        GenericVirtualImplementations.Add((receiver, method), implementation);
        return implementation;
    }
}
