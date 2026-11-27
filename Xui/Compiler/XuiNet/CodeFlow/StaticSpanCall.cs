using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A bounded static PE field consumed by a span constructor, CreateSpan, or InitializeArray.</summary>
public sealed record StaticSpanCall(
    FieldIdentity Field,
    SignatureType Result,
    int ByteLength,
    int ElementSize,
    int TokenOffset,
    int ElementCount,
    bool PointerConstructor = false,
    bool ArrayInitializer = false);
