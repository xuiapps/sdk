using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Explicitly selects the single-agent task implementation for a linked compilation.</summary>
public static class BrowserTasks
{
    /// <summary>The caller supplies and owns both implementation PEs for the lifetime of the compilation.</summary>
    public static TypeReplacement[] Replacements(Assembly coreLibrary, Assembly browserRuntime)
    {
        if (coreLibrary.Identity.Name != "System.Private.CoreLib" || browserRuntime.Identity.Name != "Xui.Compiler.Runtime")
            throw new ArgumentException("Task bindings require System.Private.CoreLib and Xui.Compiler.Runtime implementation assemblies.");
        string[] taskNames = ["Task", "Task`1"];
        string[] compilerNames = ["TaskAwaiter", "TaskAwaiter`1", "ConfiguredTaskAwaitable", "ConfiguredTaskAwaitable`1",
            "ConfiguredTaskAwaitable+ConfiguredTaskAwaiter", "ConfiguredTaskAwaitable`1+ConfiguredTaskAwaiter",
            "AsyncTaskMethodBuilder", "AsyncTaskMethodBuilder`1"];
        return [.. taskNames.Select(name => Pair("System.Threading.Tasks." + name, "Xui.Compiler.Runtime.Tasks." + name)),
            .. compilerNames.Select(name => Pair("System.Runtime.CompilerServices." + name, "Xui.Compiler.Runtime.Tasks." + name)),
            Pair("System.Lazy`1", "Xui.Compiler.Runtime.Lazy`1")];

        TypeReplacement Pair(string source, string implementation) => new(Find(coreLibrary, source), Find(browserRuntime, implementation));
    }

    private static TypeIdentity Find(Assembly assembly, string name)
    {
        var module = assembly.ManifestModule;
        return new(module, module.Types.Single(handle => FullName(module, handle) == name));
    }

    private static string FullName(Module module, TypeDefinitionHandle handle)
    {
        var type = module.GetType(handle);
        return type.DeclaringType.IsNil ? type.Namespace + "." + type.Name : FullName(module, type.DeclaringType) + "+" + type.Name;
    }
}
