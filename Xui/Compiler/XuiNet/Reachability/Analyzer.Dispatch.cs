using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

public sealed partial class Analyzer
{
    private void Instantiate(Dependency.Instantiation node)
    {
        Add(new Dependency.Layout(node.Identity), node, DependencyReason.Allocation);
        if ((node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle).Attributes & TypeAttributes.BeforeFieldInit) == 0)
            Add(new Dependency.Initialization(node.Identity), node, DependencyReason.Initialization);
        if (node.Identity.IsValueType) return;
        var definition = node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle);
        if ((definition.Attributes & TypeAttributes.Abstract) != 0)
            throw new InvalidProgramException($"Cannot allocate an abstract class: {node.Identity}.");
        _ = dispatch.Resolve(node.Identity);
        // Index by each ancestor so a new slot only visits compatible instantiated classes.
        foreach (var contract in node.Identity.Interfaces()) Register(contract);
        TypeIdentity? ancestor = node.Identity;
        while (ancestor is { Handle.Kind: HandleKind.TypeDefinition } type)
        {
            Register(type);
            ancestor = type.BaseType;
        }
        void Register(TypeIdentity type)
        {
            if (!instantiated.TryGetValue(type, out var instances)) instantiated.Add(type, instances = new());
            instances.Add(node);
            if (virtualCalls.TryGetValue(type, out var calls))
                foreach (var call in calls) Connect(call, node);
        }
    }

    private void InvokeVirtual(Dependency.VirtualCall node)
    {
        Add(new Dependency.Method(node.Identity), node, DependencyReason.VirtualCall);
        Add(new Dependency.Layout(node.Identity.DeclaringType), node, DependencyReason.Layout);
        if (!node.Identity.DeclaringType.IsInterface) _ = dispatch.Slot(node.Identity);
        else _ = dispatch.Resolve(node.Identity.DeclaringType);
        if (!virtualCalls.TryGetValue(node.Identity.DeclaringType, out var calls))
            virtualCalls.Add(node.Identity.DeclaringType, calls = new());
        calls.Add(node);
        if (instantiated.TryGetValue(node.Identity.DeclaringType, out var instances))
            foreach (var instance in instances) Connect(node, instance);
    }

    private void Connect(Dependency.VirtualCall call, Dependency.Instantiation instance)
    {
        var implementation = call.Identity.DeclaringType.IsInterface
            ? dispatch.InterfaceImplementation(instance.Identity, call.Identity)
            : dispatch.Resolve(instance.Identity)[(int)dispatch.Slot(call.Identity)].Implementation;
        if ((implementation.DeclaringType.Module.GetMethod(implementation.Handle).Attributes & MethodAttributes.Abstract) != 0)
            throw new InvalidProgramException($"Concrete class {instance.Identity} has no implementation of {call.Identity}.");
        Add(new Dependency.Method(implementation), call, DependencyReason.VirtualImplementation, condition: instance);
    }
}
