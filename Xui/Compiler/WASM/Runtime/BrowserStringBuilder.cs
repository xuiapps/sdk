using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Opts into a managed character buffer with native JavaScript string materialization.</summary>
public static class BrowserStringBuilder
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("StringBuilder bindings require CoreLib and the browser runtime implementation PEs.");
        return [new(Find(coreLibrary, "System.Text"), Find(browserRuntime, "Xui.Compiler.Runtime.Text"))];
        static TypeIdentity Find(Assembly assembly, string ns)
        {
            var module = assembly.ManifestModule;
            return new(module, module.Types.Single(h => { var t = module.GetType(h); return t.Namespace == ns && t.Name == "StringBuilder"; }));
        }
    }
}
