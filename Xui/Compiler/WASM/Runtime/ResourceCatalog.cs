using System.Text.Json;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Packages embedded PE resources beside WASM, preserving their owning module identity.</summary>
public static class ResourceCatalog
{
    internal static string AssemblyKey(Assembly assembly) => assembly.Identity.Name + "/" + assembly.ManifestModule.Mvid.ToString("N");

    /// <summary>Serializes original bytes; gzip decoding happens in the browser before module startup.</summary>
    public static string Json(IEnumerable<Assembly> assemblies)
    {
        var entries = new List<object>();
        foreach (var assembly in assemblies)
        {
            var module = assembly.ManifestModule;
            foreach (var handle in module.ManifestResources)
            {
                var resource = module.GetManifestResource(handle);
                if (!resource.Implementation.IsNil) throw new NotSupportedException($"Linked resource: {resource.Name}.");
                var reader = resource.Data;
                var bytes = reader.ReadBytes(reader.Length);
                entries.Add(new { assembly = AssemblyKey(assembly), name = resource.Name, data = Convert.ToBase64String(bytes),
                    gzip = resource.Name.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) });
            }
        }
        return JsonSerializer.Serialize(entries);
    }
}
