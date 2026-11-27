namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>The exact element type and rank of a managed vector or rectangular array.</summary>
public sealed record ArraySignature(
    SignatureType Element,
    int Rank = 1,
    bool IsVector = true);
