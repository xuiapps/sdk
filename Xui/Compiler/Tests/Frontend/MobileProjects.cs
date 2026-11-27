using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Xui.Compiler.Tests.Frontend;

/// <summary>Separate implementation assemblies for the real mobile-shell readiness probe.</summary>
internal static class MobileProjects
{
    private static readonly Lazy<IReadOnlyDictionary<string, byte[]>> images = new(Compile);
    public static IReadOnlyDictionary<string, byte[]> Images => images.Value;

    private static IReadOnlyDictionary<string, byte[]> Compile()
    {
        // The SDK's ASP.NET shared framework supplies the Microsoft.Extensions
        // contracts used by Core.DI. No desktop/native runtime is source-linked.
        string shared = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "../.."));
        string extensions = Directory.GetDirectories(Path.Combine(shared, "Microsoft.AspNetCore.App"))
            .OrderByDescending(path => Version.Parse(Path.GetFileName(path).Split('-')[0])).First();
        List<MetadataReference> references = Directory.EnumerateFiles(extensions, "Microsoft.Extensions.*.dll")
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToList();
        var result = new Dictionary<string, byte[]> { ["Xui.Core"] = CoreCanvasProfile.Image };
        references.Add(MetadataReference.CreateFromImage(CoreCanvasProfile.Image));
        Add("Core/Core.DI", "Xui.Core.DI");
        Add("Core/Fonts", "Xui.Core.Fonts");
        Add("DevKit/UI", "Xui.DevKit.UI");
        Add("Runtime/Software", "Xui.Runtime.Software");
        Add("Middleware/Emulator", "Xui.Middleware.Emulator");
        Add("Apps/TestApp", "TestApp.Mobile", ["Program.cs", "Platform.cs"]);
        return result;

        void Add(string directory, string name, string[]? exclude = null)
        {
            byte[] image = SourceProject.Compile(directory, name, references,
                extraSources: name switch {
                    "TestApp.Mobile" => [File.ReadAllText(Path.Combine(SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Mobile/BrowserMenu.cs")),
                        File.ReadAllText(Path.Combine(SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Mobile/CataloguePage.cs"))],
                    "Xui.Middleware.Emulator" => [File.ReadAllText(Path.Combine(SourceProject.XuiRoot, "Compiler/Tests/Fixtures/TestApp/Mobile/PhoneHost.cs"))],
                    _ => null },
                exclude: exclude, generateShaders: name == "TestApp.Mobile",
                resources: name == "TestApp.Mobile" ? TestAppResources.Read() : null);
            result.Add(name, image); references.Add(MetadataReference.CreateFromImage(image));
        }
    }
}
