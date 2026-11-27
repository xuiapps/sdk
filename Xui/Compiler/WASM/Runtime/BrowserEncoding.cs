using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Opts into managed-span UTF-8 conversion without pinning WASM GC storage.</summary>
public static class BrowserEncoding
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Encoding bindings require CoreLib and the browser runtime implementation PEs.");
        return [Pair("Encoding"), Pair("UTF8Encoding"), Pair("UnicodeEncoding"), Pair("Decoder")];
        TypeReplacement Pair(string name) => new(Find(coreLibrary, "System.Text", name), Find(browserRuntime, "Xui.Compiler.Runtime.Text", name));
        static TypeIdentity Find(Assembly assembly, string ns, string name)
        {
            var module = assembly.ManifestModule;
            return new(module, module.Types.Single(h => { var t = module.GetType(h); return t.Namespace == ns && t.Name == name; }));
        }
    }
}
