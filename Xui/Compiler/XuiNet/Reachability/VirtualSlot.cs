using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>A nominal class slot, independent of WASM indices and layout.</summary>
public readonly record struct VirtualSlot(
    uint Index,
    MethodIdentity Declaration,
    MethodIdentity Implementation);
