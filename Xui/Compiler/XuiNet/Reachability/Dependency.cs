using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>A structurally identified requirement interned once within a dependency graph.</summary>
public abstract record Dependency
{
    /// <summary>An explicitly supplied root and its expansion policy.</summary>
    public sealed record Root(Reachability.Root Value) : Dependency;
    /// <summary>A nominal type identity; does not imply its methods execute or instances exist.</summary>
    public sealed record Type(TypeIdentity Identity) : Dependency;
    /// <summary>A complete instance layout, including fields that are never directly accessed.</summary>
    public sealed record Layout(TypeIdentity Identity) : Dependency;
    /// <summary>A concrete runtime type that can be allocated or supplied through a root boundary.</summary>
    public sealed record Instantiation(TypeIdentity Identity) : Dependency;
    /// <summary>A method body in its declaring type's specialization context.</summary>
    public sealed record Method(MethodIdentity Identity) : Dependency;
    /// <summary>A virtual slot that can be invoked on compatible instantiated classes.</summary>
    public sealed record VirtualCall(MethodIdentity Identity) : Dependency;
    /// <summary>A field declaration, with access kinds recorded on incoming edges.</summary>
    public sealed record Field(FieldIdentity Identity) : Dependency;
    /// <summary>Potential type initialization; preserving this does not eagerly execute the initializer.</summary>
    public sealed record Initialization(TypeIdentity Identity) : Dependency;
    /// <summary>A string used by reachable IL.</summary>
    public sealed record Literal(string Value) : Dependency;
    /// <summary>An instruction supplied by a host rewriter instead of its original managed dependencies.</summary>
    public sealed record Rewrite(
        MethodIdentity Identity,
        int Offset) : Dependency;
    /// <summary>A runtime operation whose managed body is supplied by the backend.</summary>
    public sealed record Intrinsic(string Name) : Dependency;
}
