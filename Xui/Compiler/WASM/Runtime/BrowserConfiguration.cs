using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Opts into per-module managed AppContext switches for browser compilations.</summary>
public static class BrowserConfiguration
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Configuration bindings require CoreLib and the browser runtime implementation PEs.");
        return [new(Find(coreLibrary, "System"), Find(browserRuntime, "Xui.Compiler.Runtime"))];

        static TypeIdentity Find(Assembly assembly, string ns)
        {
            var module = assembly.ManifestModule;
            return new(module, module.Types.Single(h => { var t = module.GetType(h); return t.Namespace == ns && t.Name == "AppContext"; }));
        }
    }
}
