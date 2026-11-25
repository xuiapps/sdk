using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A reachable interface declaration mapped to a concrete receiver's implementation.</summary>
/// <remarks>Index is relative to the interface prefix of the receiver's function table.</remarks>
public readonly record struct InterfaceSlot(
    uint Index,
    MethodIdentity Declaration,
    MethodIdentity Implementation);
