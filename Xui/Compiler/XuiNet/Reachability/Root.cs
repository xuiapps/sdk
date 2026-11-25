using Xui.Compiler.XuiNet.DotNet;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>An explicit preservation boundary. References discovered beneath a root do not become new roots.</summary>
public abstract record Root
{
    /// <summary>Preserves the selected members of an assembly; referenced assemblies are not rooted.</summary>
    public sealed record Assembly(
        Model.Assembly Value,
        MemberPolicy Policy = MemberPolicy.All) : Root;

    /// <summary>Preserves a type's layout and selected declared members. Open types apply to discovered specializations.</summary>
    public sealed record Type(
        TypeIdentity Identity,
        MemberPolicy Policy = MemberPolicy.All) : Root;

    /// <summary>Preserves a concrete method and its transitive execution dependencies.</summary>
    public sealed record Method(
        MethodIdentity Identity) : Root;

    /// <summary>Creates a method root from a managed assembly's entry point.</summary>
    public static Method EntryPoint(Model.Assembly assembly)
    {
        var module = assembly.ManifestModule;
        if (module.EntryPoint.IsNil) throw new NotSupportedException("The assembly has no managed entry point.");
        return new(MethodIdentity.FromDefinition(module, module.EntryPoint));
    }
}
