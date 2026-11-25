using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Resources;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet.Resources;

public class ResourceTests
{
    [Fact]
    public void EmbeddedResourcesExposeMetadataAndTypedStrings()
    {
        using var data = new MemoryStream();
        using (var writer = new ResourceWriter(data))
        {
            writer.AddResource("message", "From this PE: Ж😀\0done");
            writer.AddResource("empty", "");
            writer.AddResource("null", (string?)null);
            writer.AddResource("number", 42);
            writer.Generate();
        }
        byte[] payload = data.ToArray();
        byte[] image = Frontend.Roslyn.Compile("public class Fixture { }", OutputKind.DynamicallyLinkedLibrary,
            resources: [new ResourceDescription("Fixture.Strings.resources", () => new MemoryStream(payload), true)]);
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = Assert.Single(module.ManifestResources);
        var resource = module.GetManifestResource(handle);
        Assert.Equal("Fixture.Strings.resources", resource.Name);
        Assert.Equal(ManifestResourceAttributes.Public, resource.Attributes);
        Assert.True(resource.Implementation.IsNil);
        var bytes = resource.Data;
        Assert.Equal(payload, bytes.ReadBytes(bytes.RemainingBytes));
        Assert.Equal("From this PE: Ж😀\0done", resource.ReadString("message"));
        Assert.Equal("", resource.ReadString("empty"));
        Assert.Null(resource.ReadString("null"));
        Assert.Throws<NotSupportedException>(() => module.GetManifestResource(handle).ReadString("number"));
        Assert.Throws<KeyNotFoundException>(() => module.GetManifestResource(handle).ReadString("missing"));
        assembly.Dispose();
        Assert.Throws<ObjectDisposedException>(() => module.GetManifestResource(handle).ReadString("message"));
    }
}
