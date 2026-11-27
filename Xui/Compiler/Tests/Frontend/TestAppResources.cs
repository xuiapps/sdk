using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Imports TestApp's literal embedded-resource items and their declared logical names.</summary>
internal static class TestAppResources
{
    public static IEnumerable<ResourceDescription> Read()
    {
        string root = Path.Combine(SourceProject.XuiRoot, "Apps/TestApp");
        var project = XDocument.Load(Path.Combine(root, "TestApp.targets"));
        foreach (var item in project.Descendants("EmbeddedResource"))
            foreach (string pattern in item.Attribute("Include")!.Value.Split(';'))
                foreach (string path in Directory.GetFiles(Path.Combine(root, Path.GetDirectoryName(pattern)!), Path.GetFileName(pattern)).Order(StringComparer.Ordinal))
                {
                    string name = item.Attribute("LogicalName")!.Value.Replace("%(Filename)", Path.GetFileNameWithoutExtension(path))
                        .Replace("%(Extension)", Path.GetExtension(path));
                    yield return new(name, () => File.OpenRead(path), true);
                }
    }
}
