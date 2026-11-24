using System.Collections.Immutable;

namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>An immutable, structurally comparable vector of generic type arguments.</summary>
public sealed class GenericArguments : IEquatable<GenericArguments>
{
    private readonly int hash;
    /// <summary>Copies argument values into an immutable vector; argument order contributes to identity.</summary>
    public GenericArguments(IEnumerable<SignatureType> arguments)
    {
        Values = arguments.ToImmutableArray();
        Depth = 1 + Values.Select(value => value.Depth).DefaultIfEmpty().Max();
        if (Depth > 32) throw new NotSupportedException("Generic specialization depth exceeds 32; expanding generic recursion is not supported.");
        var code = new HashCode();
        foreach (var value in Values) code.Add(value);
        hash = code.ToHashCode();
    }
    /// <summary>The argument types in parameter-index order.</summary>
    public ImmutableArray<SignatureType> Values { get; }
    /// <summary>Whether this vector still contains an unbound type or method parameter.</summary>
    public bool ContainsGenericParameters => Values.Any(value => value.ContainsGenericParameters);
    internal int Depth { get; }
    /// <summary>Compares all argument identities and qualifiers.</summary>
    public bool Equals(GenericArguments? other) => ReferenceEquals(this, other) ||
        (other is not null && hash == other.hash && Values.SequenceEqual(other.Values));
    public override bool Equals(object? obj) => obj is GenericArguments other && Equals(other);
    public override int GetHashCode() => hash;
}
