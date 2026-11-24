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
        var registered = new HashSet<TypeIdentity>();
        foreach (var contract in node.Identity.Interfaces()) Register(contract);
        TypeIdentity? ancestor = node.Identity;
        while (ancestor is { Handle.Kind: HandleKind.TypeDefinition } type)
        {
            Register(type);
            ancestor = type.BaseType;
        }
        void Register(TypeIdentity type)
        {
            var key = DispatchKey(type);
            if (!registered.Add(key)) return;
            if (!instantiated.TryGetValue(key, out var instances)) instantiated.Add(key, instances = new());
            instances.Add(node);
            if (virtualCalls.TryGetValue(key, out var calls))
                foreach (var call in calls) Connect(call, node);
            if (equalityCalls.TryGetValue(key, out var equality)) ConnectEquality(equality, node);
        }
    }

    private void InvokeEquality(Dependency.Equality node)
    {
        equalityCalls.Add(node.Identity, node);
        if (instantiated.TryGetValue(node.Identity, out var instances))
            foreach (var instance in instances) ConnectEquality(node, instance);
    }

    private void ConnectEquality(Dependency.Equality equality, Dependency.Instantiation instance)
    {
        if (DotNet.Intrinsics.ObjectEquality.Implementation(dispatch.Resolve(instance.Identity)) is { } method)
            Add(new Dependency.Method(method), equality, DependencyReason.VirtualImplementation, condition: instance);
    }

    private void InvokeVirtual(Dependency.VirtualCall node)
    {
        if (FrameworkTypes.Is(node.Identity.DeclaringType, "System.Collections.Generic", "IEqualityComparer`1"))
            throw new NotSupportedException("Framework IEqualityComparer dispatch is not implemented; default comparers currently require their concrete API.");
        bool defaultBody = node.Identity.DeclaringType.IsInterface &&
            (node.Identity.DeclaringType.Module.GetMethod(node.Identity.Handle).Attributes & MethodAttributes.Abstract) == 0;
        Add(defaultBody ? new Dependency.Signature(node.Identity) : new Dependency.Method(node.Identity), node, DependencyReason.VirtualCall);
        Add(new Dependency.Layout(node.Identity.DeclaringType), node, DependencyReason.Layout);
        if (!node.Identity.DeclaringType.IsInterface) _ = dispatch.Slot(node.Identity);
        else _ = dispatch.Resolve(node.Identity.DeclaringType);
        if (!virtualCalls.TryGetValue(DispatchKey(node.Identity.DeclaringType), out var calls))
            virtualCalls.Add(DispatchKey(node.Identity.DeclaringType), calls = new());
        calls.Add(node);
        if (instantiated.TryGetValue(DispatchKey(node.Identity.DeclaringType), out var instances))
            foreach (var instance in instances) Connect(node, instance);
    }

    // Constructed interfaces share an index by definition, limiting variance checks to that family.
    private static TypeIdentity DispatchKey(TypeIdentity type) => type.IsInterface ? type.GenericDefinition : type;

    private void Connect(Dependency.VirtualCall call, Dependency.Instantiation instance)
    {
        if (!instance.Identity.IsAssignableTo(call.Identity.DeclaringType)) return;
        if (DelegateInfo.Is(instance.Identity)) throw new NotSupportedException("Virtual calls through delegate base/interface APIs are not implemented.");
        var implementation = call.Identity.DeclaringType.IsInterface
            ? dispatch.InterfaceImplementation(instance.Identity, call.Identity)
            : dispatch.Resolve(instance.Identity)[(int)dispatch.Slot(call.Identity)].Implementation;
        if ((implementation.DeclaringType.Module.GetMethod(implementation.Handle).Attributes & MethodAttributes.Abstract) != 0)
            throw new InvalidProgramException($"Concrete class {instance.Identity} has no implementation of {call.Identity}.");
        Add(new Dependency.Method(implementation), call, DependencyReason.VirtualImplementation, condition: instance);
    }
}
