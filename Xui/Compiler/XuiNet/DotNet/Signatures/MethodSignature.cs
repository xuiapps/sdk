using System.Collections.Immutable;

namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>A decoded method signature. Parameters exclude the implicit instance receiver.</summary>
public readonly record struct MethodSignature(
    ImmutableArray<SignatureType> Parameters,
    SignatureType Result,
    bool HasThis,
    int GenericParameterCount = 0);
