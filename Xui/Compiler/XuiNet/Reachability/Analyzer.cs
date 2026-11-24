using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Discovers source dependencies with an interned work queue before backend planning.</summary>
public sealed partial class Analyzer
{
    private readonly Dictionary<Dependency, Dependency> interned = new();
    private readonly List<Dependency> nodes = new();
    private readonly List<DependencyEdge> edges = new();
    private readonly HashSet<DependencyEdge> seenEdges = new();
    private readonly DispatchTable dispatch = new();
    private readonly Dictionary<TypeIdentity, List<Root.Type>> typeRoots = new();
    private readonly Dictionary<TypeIdentity, List<Dependency.VirtualCall>> virtualCalls = new();
    private readonly Dictionary<TypeIdentity, List<Dependency.Instantiation>> instantiated = new();
    private readonly HashSet<Model.Module> modules = new();
    private readonly InstructionRewriter? rewrite;

    private Analyzer(InstructionRewriter? rewrite)
    {
        this.rewrite = rewrite;
    }

    /// <summary>Expands mixed roots and follows their dependencies to a fixed point.</summary>
    /// <remarks>Resolves members in the supplied compilation and closed generic classes. Unknown external calls,
    /// generic methods, and exception regions fail explicitly. Generic argument identities retain
    /// their existing nesting bound to reject expanding generic recursion.</remarks>
    public static DependencyGraph Analyze(IEnumerable<Root> roots, InstructionRewriter? rewrite = null)
    {
        var analyzer = new Analyzer(rewrite);
        foreach (var root in roots) analyzer.Add(new Dependency.Root(root));
        for (int i = 0; i < analyzer.nodes.Count; i++) analyzer.Visit(analyzer.nodes[i]);
        return new(analyzer.nodes, analyzer.edges, analyzer.dispatch);
    }

    private Dependency Add(Dependency target, Dependency? source = null, DependencyReason reason = default,
        int? offset = null, Dependency? condition = null)
    {
        if (!interned.TryGetValue(target, out var canonical))
        {
            canonical = target;
            interned.Add(target, target);
            nodes.Add(target);
        }
        if (source is not null)
        {
            var edge = new DependencyEdge(source, canonical, reason, offset, condition);
            if (seenEdges.Add(edge)) edges.Add(edge);
        }
        return canonical;
    }

    private void Visit(Dependency node)
    {
        switch (node)
        {
            case Dependency.Root root: Expand(root); break;
            case Dependency.Method method: ReadMethod(method); break;
            case Dependency.Signature signature: ReadDeclaration(signature, signature.Identity, false); break;
            case Dependency.Type type: ReadType(type); break;
            case Dependency.Layout layout: ReadLayout(layout); break;
            case Dependency.Field field: ReadField(field); break;
            case Dependency.Initialization initialization: ReadInitializer(initialization); break;
            case Dependency.Instantiation allocation: Instantiate(allocation); break;
            case Dependency.VirtualCall call: InvokeVirtual(call); break;
        }
    }

    private void Signature(Dependency source, SignatureType signature, DependencyReason reason, int? offset = null)
    {
        if (signature.Identity is not { } identity) return;
        Add(new Dependency.Type(identity), source, reason, offset);
        // Preserve the existing representation, including complete layouts of named reference fields.
        if (identity.Handle.Kind == HandleKind.TypeDefinition && !FrameworkTypes.IsRuntimeBase(identity) && !DotNet.Intrinsics.NativeFloat.Is(identity))
            Add(new Dependency.Layout(identity), source, DependencyReason.Layout, offset);
    }

    private void ReadType(Dependency.Type node)
    {
        var identity = node.Identity;
        if (FrameworkTypes.IsRuntimeBase(identity) || DotNet.Intrinsics.NativeFloat.Is(identity)) return;
        if (identity.Arguments is { } arguments)
        {
            foreach (var argument in arguments.Values) Signature(node, argument, DependencyReason.GenericArgument);
        }
        if (identity.Handle.Kind != HandleKind.TypeDefinition) return; // Unresolved identity, not an executable dependency.
        if (IsOpen(identity)) return;
        var type = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        if (!type.BaseType.IsNil)
            Signature(node, SignatureReader.Resolve(identity.Module, type.BaseType, identity.Arguments), DependencyReason.BaseType);
        // Value-type interface membership matters when boxing/constrained dispatch is implemented;
        // it contributes no fields or executable code to an unboxed tuple.
        if (!identity.IsValueType) foreach (var handle in type.Interfaces)
            Add(new Dependency.Type(SignatureReader.Resolve(identity.Module, identity.Module.GetInterfaceType(handle), identity.Arguments).Identity!.Value),
                node, DependencyReason.Interface);
        if (type.IsInterface) _ = dispatch.Resolve(identity);
        if (typeRoots.TryGetValue(identity.GenericDefinition, out var roots))
            foreach (var root in roots) Preserve(new Dependency.Root(root), identity, root.Policy);
    }

    private void ReadLayout(Dependency.Layout node)
    {
        var identity = node.Identity;
        Add(new Dependency.Type(identity), node, DependencyReason.Layout);
        if (identity.Handle.Kind != HandleKind.TypeDefinition || IsOpen(identity))
            throw new NotSupportedException($"Layout requires a closed resolved type: {identity}.");
        var type = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        if (!identity.IsValueType && type.Name != "<Module>") _ = dispatch.Resolve(identity);
        if (identity.BaseType is { Handle.Kind: HandleKind.TypeDefinition } parent && !FrameworkTypes.IsRuntimeBase(parent))
            Add(new Dependency.Layout(parent), node, DependencyReason.BaseType);
        foreach (var handle in type.Fields)
            if ((identity.Module.GetField(handle).Attributes & FieldAttributes.Static) == 0)
                Add(new Dependency.Field(new(identity, handle)), node, DependencyReason.InstanceField);
    }

    private void ReadField(Dependency.Field node)
    {
        var identity = node.Identity;
        var field = identity.DeclaringType.Module.GetField(identity.Handle);
        Add(new Dependency.Type(identity.DeclaringType), node, DependencyReason.DeclaringType);
        Signature(node, SignatureReader.Field(identity.DeclaringType.Module, field.Signature, identity.DeclaringType.Arguments), DependencyReason.FieldType);
        if ((field.Attributes & FieldAttributes.Static) == 0)
            Add(new Dependency.Layout(identity.DeclaringType), node, DependencyReason.Layout);
    }

    private void ReadInitializer(Dependency.Initialization node)
    {
        Add(new Dependency.Type(node.Identity), node, DependencyReason.Initialization);
        foreach (var handle in node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle).Methods)
            if (node.Identity.Module.GetMethod(handle).Name == ".cctor")
                Add(new Dependency.Method(new(node.Identity, handle)), node, DependencyReason.Initializer);
    }

    private static bool IsOpen(TypeIdentity identity) => identity.Arguments?.ContainsGenericParameters == true ||
        identity.Module.GetType((TypeDefinitionHandle)identity.Handle).GenericParameterCount != (identity.Arguments?.Values.Length ?? 0);

    private void UseModule(Model.Module module, Dependency source)
    {
        if (!modules.Add(module)) return;
        foreach (var handle in module.Types)
            if (module.GetType(handle).Name == "<Module>")
            {
                foreach (var method in module.GetType(handle).Methods)
                    if (module.GetMethod(method).Name == ".cctor")
                        Add(new Dependency.Initialization(new(module, handle)), source, DependencyReason.Initialization);
                break;
            }
    }
}
