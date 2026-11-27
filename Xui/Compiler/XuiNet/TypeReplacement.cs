using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet;

/// <summary>Explicitly selects a target-runtime implementation for an open named type definition.</summary>
/// <remarks>Signatures and member references bind to the replacement throughout this compilation.
/// The replacement supplies its own layout and method bodies; missing members fail resolution.</remarks>
public readonly record struct TypeReplacement(
    TypeIdentity Source,
    TypeIdentity Implementation);
