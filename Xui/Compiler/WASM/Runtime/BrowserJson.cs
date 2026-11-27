using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Opt-in browser-backed JSON documents; preserves the original application's public signatures.</summary>
public static class BrowserJson
{
    public static TypeReplacement[] Replacements(Assembly json, Assembly browserRuntime)
    {
        if (json.Identity.Name != "System.Text.Json" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("JSON bindings require System.Text.Json and the compiler runtime implementation PEs.");
        var sourceElement = Find(json, "System.Text.Json", "JsonElement");
        var targetElement = Find(browserRuntime, "Xui.Compiler.Runtime.Json", "JsonElement");
        return [new(Find(json, "System.Text.Json", "JsonDocument"), Find(browserRuntime, "Xui.Compiler.Runtime.Json", "JsonDocument")),
            new(sourceElement, targetElement), new(Nested(sourceElement, "ArrayEnumerator"), Nested(targetElement, "ArrayEnumerator"))];
    }

    private static TypeIdentity Find(Assembly assembly, string ns, string name)
    {
        var module = assembly.ManifestModule;
        foreach (var handle in module.Types)
        { var type = module.GetType(handle); if (type.Namespace == ns && type.Name == name) return new(module, handle); }
        throw new InvalidOperationException($"Missing JSON profile type: {ns}.{name}.");
    }
    private static TypeIdentity Nested(TypeIdentity parent, string name)
    {
        foreach (var handle in parent.Module.Types)
        { var type = parent.Module.GetType(handle); if (type.DeclaringType == parent.Handle && type.Name == name) return new(parent.Module, handle); }
        throw new InvalidOperationException($"Missing JSON profile nested type: {name}.");
    }
}
