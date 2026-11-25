using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>An ordered typed catch or a finally/fault cleanup with an independently seeded evaluation stack.</summary>
public sealed record ExceptionHandler(
    int Offset,
    ExceptionRegionKind Kind,
    TypeIdentity? CatchType,
    SequenceBlock Body);
