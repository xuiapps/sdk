using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

public sealed partial class Analyzer
{
    private void Expand(Dependency.Root node)
    {
        switch (node.Value)
        {
            case Root.Method method:
                RootMethod(node, method.Identity);
                break;
            case Root.Type type:
                if (type.Identity.Handle.Kind != HandleKind.TypeDefinition)
                    throw new NotSupportedException("Type roots require resolved definitions.");
                Add(new Dependency.Type(type.Identity), node, DependencyReason.Root);
                if (IsOpen(type.Identity))
                {
                    if (!typeRoots.TryGetValue(type.Identity.GenericDefinition, out var roots))
                        typeRoots.Add(type.Identity.GenericDefinition, roots = new());
                    roots.Add(type);
                    // A template root may arrive after its closed instances have already been visited.
                    foreach (var known in nodes.OfType<Dependency.Type>().ToArray())
                        if (known.Identity.GenericDefinition == type.Identity.GenericDefinition && !IsOpen(known.Identity))
                            Preserve(node, known.Identity, type.Policy);
                }
                else Preserve(node, type.Identity, type.Policy);
                break;
            case Root.Assembly assembly:
                var module = assembly.Value.ManifestModule;
                UseModule(module, node);
                foreach (var handle in module.Types)
                {
                    if (module.GetType(handle).Name == "<Module>") continue;
                    if (assembly.Policy == MemberPolicy.PublicApi && !Visible(module, handle)) continue;
                    Add(new Dependency.Root(new Root.Type(new(module, handle), assembly.Policy)), node, DependencyReason.Root);
                }
                break;
        }
    }

    private void Preserve(Dependency source, TypeIdentity identity, MemberPolicy policy)
    {
        var type = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        if (!type.IsInterface) Add(new Dependency.Layout(identity), source, DependencyReason.Root);
        foreach (var handle in type.Methods)
        {
            var method = identity.Module.GetMethod(handle);
            if (policy == MemberPolicy.PublicApi && !Visible(method.Attributes)) continue;
            RootMethod(source, new(identity, handle));
        }
        foreach (var handle in type.Fields)
        {
            var field = identity.Module.GetField(handle);
            if (policy == MemberPolicy.PublicApi && (field.Attributes & FieldAttributes.FieldAccessMask) is not
                (FieldAttributes.Public or FieldAttributes.Family or FieldAttributes.FamORAssem)) continue;
            Add(new Dependency.Field(new(identity, handle)), source, DependencyReason.Root);
            if ((field.Attributes & FieldAttributes.Static) != 0)
                Add(new Dependency.Initialization(identity), source, DependencyReason.Initialization);
        }
    }

    private void RootMethod(Dependency source, MethodIdentity identity)
    {
        var module = identity.DeclaringType.Module;
        var method = module.GetMethod(identity.Handle);
        Add(new Dependency.Method(identity), source, DependencyReason.Root);
        // Instance roots can be invoked with a receiver supplied by the host, even without newobj in IL.
        if ((method.Attributes & MethodAttributes.Static) == 0 && !identity.DeclaringType.IsValueType &&
            (module.GetType((TypeDefinitionHandle)identity.DeclaringType.Handle).Attributes & TypeAttributes.Abstract) == 0)
            Add(new Dependency.Instantiation(identity.DeclaringType), source, DependencyReason.Root);
    }

    private static bool Visible(MethodAttributes attributes) => (attributes & MethodAttributes.MemberAccessMask) is
        MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;

    private static bool Visible(Model.Module module, TypeDefinitionHandle handle)
    {
        var type = module.Metadata.GetTypeDefinition(handle);
        return (type.Attributes & TypeAttributes.VisibilityMask) switch
        {
            TypeAttributes.Public => true,
            TypeAttributes.NestedPublic or TypeAttributes.NestedFamily or TypeAttributes.NestedFamORAssem => Visible(module, type.GetDeclaringType()),
            _ => false
        };
    }
}
