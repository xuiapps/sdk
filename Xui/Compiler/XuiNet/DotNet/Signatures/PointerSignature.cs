namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>The exact pointee of an unmanaged pointer, including void and nested pointers.</summary>
/// <remarks>Retaining a metadata signature does not grant support for pointer storage or execution.</remarks>
public sealed record PointerSignature(SignatureType Element);
