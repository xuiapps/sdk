using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Explicit browser replacements for synchronous packaged-resource reads.</summary>
public static class BrowserResources
{
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly compression, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || compression.Identity.Name != "System.IO.Compression" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Resource bindings require CoreLib, Compression and the browser runtime implementation PEs.");
        return [Pair(coreLibrary, "System.Reflection", "Assembly", "Reflection", "Assembly"),
            Pair(coreLibrary, "System.IO", "Stream", "IO", "Stream"), Pair(coreLibrary, "System.IO", "BinaryReader", "IO", "BinaryReader"),
            Pair(compression, "System.IO.Compression", "GZipStream", "IO", "GZipStream")];

        TypeReplacement Pair(Assembly source, string ns, string name, string targetNamespace, string target) =>
            new(Find(source, ns, name), Find(browserRuntime, "Xui.Compiler.Runtime." + targetNamespace, target));
    }

    private static TypeIdentity Find(Assembly assembly, string ns, string name)
    {
        var module = assembly.ManifestModule;
        foreach (var handle in module.Types)
        {
            var type = module.GetType(handle);
            if (type.Namespace == ns && type.Name == name) return new(module, handle);
        }
        throw new InvalidOperationException($"Missing resource profile type: {ns}.{name}.");
    }
}
