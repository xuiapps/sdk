using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>A stable virtual slot inherited from its declaration, with this type's selected implementation.</summary>
public readonly record struct VirtualSlot(
    uint Index,
    MethodIdentity Declaration,
    MethodIdentity Implementation);
