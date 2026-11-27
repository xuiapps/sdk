using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

public sealed partial class Analyzer
{
    private void Instantiate(Dependency.Instantiation node)
    {
        for (TypeIdentity? current = node.Identity; current is { } type; current = type.BaseType)
            if (DotNet.Intrinsics.NativeStringComparer.IsWrapper(type))
                throw new NotSupportedException("CoreLib string-hash wrappers cannot be constructed by the native JS-string runtime.");
        Add(new Dependency.Layout(node.Identity), node, DependencyReason.Allocation);
        if ((node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle).Attributes & TypeAttributes.BeforeFieldInit) == 0)
            Add(new Dependency.Initialization(node.Identity), node, DependencyReason.Initialization);
        if (node.Identity.IsValueType) return;
        var definition = node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle);
        if ((definition.Attributes & TypeAttributes.Abstract) != 0)
            throw new InvalidProgramException($"Cannot allocate an abstract class: {node.Identity}.");
        _ = dispatch.Resolve(node.Identity);
        if (objectTextCall is { } text) ConnectObjectText(text, node);
        if (objectHashCall is { } hash) ConnectObjectHash(hash, node);
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
            if (hashCalls.TryGetValue(key, out var hash)) ConnectHashing(hash, node);
        }
    }

    private void InvokeEquality(Dependency.Equality node)
    {
        equalityCalls.Add(node.Identity, node);
        if (instantiated.TryGetValue(node.Identity, out var instances))
            foreach (var instance in instances) ConnectEquality(node, instance);
    }

    private void InvokeObjectText(Dependency.ObjectText node)
    {
        objectTextCall = node;
        foreach (var instance in instantiated.Values.SelectMany(v => v).Distinct()) ConnectObjectText(node, instance);
    }

    private void ConnectObjectText(Dependency.ObjectText text, Dependency.Instantiation instance)
    {
        if (DotNet.Exceptions.ExceptionTypes.Kind(instance.Identity) is not null) return;
        if (DotNet.Intrinsics.ObjectText.Implementation(dispatch.Resolve(instance.Identity)) is { } method)
        {
            if (DotNet.Exceptions.ExceptionTypes.Kind(method.DeclaringType) is not null)
                throw new NotSupportedException($"Exception text requires a supported exception representation: {instance.Identity}.");
            Add(new Dependency.Method(method), text, DependencyReason.VirtualImplementation, condition: instance);
        }
    }

    private void InvokeObjectHash(Dependency.ObjectHash node)
    {
        objectHashCall = node;
        foreach (var instance in instantiated.Values.SelectMany(v => v).Distinct()) ConnectObjectHash(node, instance);
    }

    private void ConnectObjectHash(Dependency.ObjectHash hash, Dependency.Instantiation instance)
    {
        if (DotNet.Intrinsics.ObjectHash.Implementation(dispatch.Resolve(instance.Identity)) is { } method)
            Add(new Dependency.Method(method), hash, DependencyReason.VirtualImplementation, condition: instance);
    }

    private void ConnectEquality(Dependency.Equality equality, Dependency.Instantiation instance)
    {
        if (DotNet.Intrinsics.ObjectEquality.Implementation(dispatch.Resolve(instance.Identity)) is { } method)
            Add(new Dependency.Method(method), equality, DependencyReason.VirtualImplementation, condition: instance);
    }

    private void InvokeHashing(Dependency.Hashing node)
    {
        hashCalls.Add(node.Identity, node);
        if (instantiated.TryGetValue(node.Identity, out var instances))
            foreach (var instance in instances) ConnectHashing(node, instance);
    }

    private void ConnectHashing(Dependency.Hashing hash, Dependency.Instantiation instance)
    {
        if (DotNet.Intrinsics.ObjectHash.Implementation(dispatch.Resolve(instance.Identity)) is { } method)
            Add(new Dependency.Method(method), hash, DependencyReason.VirtualImplementation, condition: instance);
    }

    private void InvokeVirtual(Dependency.VirtualCall node)
    {
        if (DotNet.Intrinsics.DefaultEquality.ForInterface(node.Identity) is { } comparer)
        {
            Add(new Dependency.Intrinsic("DefaultComparer.Interface." + comparer.Operation), node, DependencyReason.VirtualCall);
            Add(new Dependency.Layout(comparer.Owner), node, DependencyReason.Layout);
            Signature(node, comparer.Element, DependencyReason.Signature);
            if (comparer.UsesObjectHash) Add(new Dependency.Hashing(comparer.Element.Identity!.Value), node, DependencyReason.VirtualCall);
            if (comparer.UsesObjectEquality) Add(new Dependency.Equality(comparer.Element.Identity!.Value), node, DependencyReason.VirtualCall);
            if (comparer.Implementation is { } implementation)
                Add(implementation.DeclaringType.IsInterface ? new Dependency.VirtualCall(implementation) : new Dependency.Method(implementation), node, DependencyReason.Call);
        }
        foreach (var array in arraySignatures.ToArray()) ConnectArrayEnumerator(node, array.Key, array.Value);
        bool defaultBody = node.Identity.DeclaringType.IsInterface &&
            (node.Identity.DeclaringType.Module.GetMethod(node.Identity.Handle).Attributes & MethodAttributes.Abstract) == 0;
        Add(defaultBody ? new Dependency.Signature(node.Identity) : new Dependency.Method(node.Identity), node, DependencyReason.VirtualCall);
        Add(new Dependency.Layout(node.Identity.DeclaringType), node, DependencyReason.Layout);
        if (!node.Identity.DeclaringType.IsInterface && node.Identity.Arguments is null) _ = dispatch.Slot(node.Identity);
        else _ = dispatch.Resolve(node.Identity.DeclaringType);
        if (!virtualCalls.TryGetValue(DispatchKey(node.Identity.DeclaringType), out var calls))
            virtualCalls.Add(DispatchKey(node.Identity.DeclaringType), calls = new());
        calls.Add(node);
        if (instantiated.TryGetValue(DispatchKey(node.Identity.DeclaringType), out var instances))
            foreach (var instance in instances) Connect(node, instance);
    }

    private void TrackArray(DotNet.Signatures.ArraySignature array, Dependency source)
    {
        if (!array.IsVector || !arraySignatures.TryAdd(array, source)) return;
        foreach (var call in virtualCalls.Values.SelectMany(c => c).ToArray()) ConnectArrayEnumerator(call, array, source);
    }

    private void ConnectArrayEnumerator(Dependency.VirtualCall call, DotNet.Signatures.ArraySignature array, Dependency source)
    {
        if (!DotNet.Intrinsics.ArrayInterface.Matches(array, call.Identity.DeclaringType) ||
            DotNet.Intrinsics.ArrayInterface.EnumeratorConstructor(call.Identity) is not { } constructor) return;
        Add(new Dependency.Instantiation(constructor.DeclaringType), call, DependencyReason.Allocation, condition: source);
        Add(new Dependency.Method(constructor), call, DependencyReason.Call, condition: source);
    }

    // Constructed interfaces share an index by definition, limiting variance checks to that family.
    private static TypeIdentity DispatchKey(TypeIdentity type) => type.IsInterface ? type.GenericDefinition : type;

    private void Connect(Dependency.VirtualCall call, Dependency.Instantiation instance)
    {
        if (!instance.Identity.IsAssignableTo(call.Identity.DeclaringType)) return;
        if (DelegateInfo.Is(instance.Identity)) throw new NotSupportedException("Virtual calls through delegate base/interface APIs are not implemented.");
        var implementation = call.Identity.DeclaringType.IsInterface
            ? dispatch.InterfaceImplementation(instance.Identity, call.Identity)
            : call.Identity.Arguments is not null ? dispatch.GenericVirtualImplementation(instance.Identity, call.Identity)
            : dispatch.Resolve(instance.Identity)[(int)dispatch.Slot(call.Identity)].Implementation;
        if ((implementation.DeclaringType.Module.GetMethod(implementation.Handle).Attributes & MethodAttributes.Abstract) != 0)
            throw new InvalidProgramException($"Concrete class {instance.Identity} has no implementation of {call.Identity}.");
        Add(new Dependency.Method(implementation), call, DependencyReason.VirtualImplementation, condition: instance);
    }
}
