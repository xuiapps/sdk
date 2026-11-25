using System.Reflection.Metadata;
using System.Resources;
using System.Text;

namespace Xui.Compiler.XuiNet.DotNet;

public sealed partial class Module
{
    private readonly Dictionary<ManifestResourceHandle, StringResources> resourceReaders = new();

    /// <summary>The manifest's embedded and linked resource handles.</summary>
    public ManifestResourceHandleCollection ManifestResources => Metadata.ManifestResources;

    /// <summary>Creates a borrowed resource declaration view.</summary>
    public ManifestResource GetManifestResource(ManifestResourceHandle handle) => new(this, handle);

    internal BlobReader ResourceData(ManifestResourceHandle handle)
    {
        var resource = Metadata.GetManifestResource(handle);
        if (!resource.Implementation.IsNil)
            throw new NotSupportedException("Linked manifest resource data is not implemented.");
        var directory = assembly.PE.PEHeaders.CorHeader!.ResourcesDirectory;
        if (resource.Offset < 0 || resource.Offset > directory.Size - 4L)
            throw new BadImageFormatException("Resource offset is outside the managed resource directory.");
        int offset = (int)resource.Offset;
        var block = assembly.PE.GetSectionData(directory.RelativeVirtualAddress);
        var reader = block.GetReader(offset, directory.Size - offset);
        int length = reader.ReadInt32();
        if (length < 0 || length > reader.RemainingBytes)
            throw new BadImageFormatException("Resource length is outside the managed resource directory.");
        return block.GetReader(offset + 4, length);
    }

    internal string? ReadResourceString(ManifestResourceHandle handle, string key)
    {
        _ = Metadata; // Cached strings still obey the assembly's lifetime.
        if (!resourceReaders.TryGetValue(handle, out var reader))
        {
            var data = ResourceData(handle);
            resourceReaders.Add(handle, reader = new(data.ReadBytes(data.RemainingBytes)));
        }
        return reader.Read(key);
    }

    internal void DisposeResourceReaders()
    {
        foreach (var reader in resourceReaders.Values) reader.Dispose();
        resourceReaders.Clear();
    }

    private sealed class StringResources : IDisposable
    {
        private readonly ResourceReader reader;
        private readonly Dictionary<string, string?> strings = new(StringComparer.Ordinal);

        public StringResources(byte[] data)
        {
            var stream = new MemoryStream(data, writable: false);
            try { reader = new(stream); }
            catch { stream.Dispose(); throw; }
        }

        public string? Read(string key)
        {
            if (strings.TryGetValue(key, out var text)) return text;
            string type;
            byte[] data;
            try { reader.GetResourceData(key, out type, out data); }
            catch (ArgumentException error) { throw new KeyNotFoundException($"Resource string was not found: {key}.", error); }
            if (type == "ResourceTypeCode.Null") text = null;
            else if (type == "ResourceTypeCode.String")
            {
                using var bytes = new BinaryReader(new MemoryStream(data, writable: false), Encoding.UTF8);
                text = bytes.ReadString();
            }
            else throw new NotSupportedException($"Resource '{key}' is {type}; only strings are supported.");
            strings.Add(key, text);
            return text;
        }

        public void Dispose() => reader.Dispose();
    }
}
