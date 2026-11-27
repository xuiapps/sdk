using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>An opaque invariant formatting provider; culture data and arbitrary provider callbacks are not retained.</summary>
public static class InvariantCulture
{
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Globalization", "CultureInfo") ||
        FrameworkTypes.Is(type, "System.Globalization", "NumberFormatInfo");

    public static TypeIdentity Resolve(Module module)
    {
        var core = module.Compilation?.Assemblies.FirstOrDefault(a => a.Identity.Name is "System.Private.CoreLib" or "mscorlib")
            ?? throw new NotSupportedException("Invariant formatting requires linked CoreLib metadata.");
        var owner = core.ManifestModule;
        return new(owner, owner.Types.Single(h => { var type=owner.GetType(h); return type.Namespace=="System.Globalization" && type.Name=="CultureInfo"; }));
    }
}
