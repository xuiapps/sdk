using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A direct struct target, or an ordinary virtual target reached by loading a constrained reference receiver.</summary>
public sealed record ConstrainedCall(
    MethodIdentity Target,
    SignatureType? ReferenceReceiver = null);
