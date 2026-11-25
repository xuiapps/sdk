namespace Xui.Compiler.XuiNet.Layout;

/// <summary>Controls storage calculation for value types without a metadata layout contract.</summary>
public enum AutoLayoutPolicy
{
    /// <summary>Reject arbitrary automatic layouts instead of implying a native interop contract.</summary>
    Reject,
    /// <summary>Assign declaration-order storage owned by this compiler, with target alignment rules.</summary>
    /// <remarks>These offsets describe the emitted module only, not CLR or marshalled storage.</remarks>
    DeclarationOrder,
}
