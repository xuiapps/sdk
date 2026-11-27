using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A closed generic virtual call mapped to its concrete receiver implementation.</summary>
/// <remarks>Index is relative to the specialized dispatch prefix of the receiver's function table.</remarks>
public readonly record struct GenericVirtualSlot(
    uint Index,
    MethodIdentity Declaration,
    MethodIdentity Implementation);
