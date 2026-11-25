using System.Runtime.InteropServices;
using Xui.Compiler.XuiNet;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Supplies actual framework implementation PEs and their forwarding facades to compiler tests.</summary>
internal sealed class Framework(params string[] additionalAssemblies) : IDisposable
{
    public Model.Assembly[] Assemblies { get; } = new[] {
        "System.Private.CoreLib.dll", "System.Runtime.dll", "System.Runtime.InteropServices.dll"
    }.Concat(additionalAssemblies).Distinct(StringComparer.Ordinal).Select(name => {
        using var file = File.OpenRead(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), name));
        return AssemblyCompiler.Read(file);
    }).ToArray();

    public void Dispose() { foreach (var assembly in Assemblies) assembly.Dispose(); }
}
