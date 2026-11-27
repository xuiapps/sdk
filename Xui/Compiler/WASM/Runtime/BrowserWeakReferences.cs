using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Explicitly binds CLR weak references to weak browser handles for WASM GC objects.</summary>
public static class BrowserWeakReferences
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Weak-reference bindings require CoreLib and the browser runtime implementation PEs.");
        return [Pair("WeakReference"), Pair("WeakReference`1")];
        TypeReplacement Pair(string name) => new(Find(coreLibrary, "System", name), Find(browserRuntime, "Xui.Compiler.Runtime", name));
    }

    private static TypeIdentity Find(Assembly assembly, string ns, string name)
    {
        var module = assembly.ManifestModule;
        return new(module, module.Types.Single(h => module.GetType(h).Namespace == ns && module.GetType(h).Name == name));
    }
}
