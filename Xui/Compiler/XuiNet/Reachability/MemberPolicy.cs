namespace Xui.Compiler.XuiNet.Reachability;

/// <summary>Which declarations an assembly or type root preserves, independently of ordinary dependencies.</summary>
public enum MemberPolicy
{
    /// <summary>All declared members, including private methods and constructors.</summary>
    All,
    /// <summary>Externally visible types and public/protected members, followed by their private dependencies.</summary>
    PublicApi
}
