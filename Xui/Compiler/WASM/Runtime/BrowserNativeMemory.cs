using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Opts into an instance-owned unmanaged heap after the linear stack and static data.</summary>
public static class BrowserNativeMemory
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Native memory bindings require CoreLib and the browser runtime implementation PEs.");
        return [new(Find(coreLibrary, "System.Runtime.InteropServices"), Find(browserRuntime, "Xui.Compiler.Runtime.Memory"))];
        static TypeIdentity Find(Assembly assembly, string ns)
        {
            var module = assembly.ManifestModule;
            return new(module, module.Types.Single(h => { var t = module.GetType(h); return t.Namespace == ns && t.Name == "NativeMemory"; }));
        }
    }
}
