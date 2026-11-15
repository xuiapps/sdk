using System.Reflection;

namespace Xui.Core.Canvas;

/// <summary>Describes one concrete font face made available by an application.</summary>
public readonly struct FontFaceDescriptor
{
    /// <summary>The case-insensitive family name.</summary>
    public string Family { get; }
    /// <summary>The numeric weight.</summary>
    public FontWeight Weight { get; }
    /// <summary>The italic or oblique style.</summary>
    public FontStyle Style { get; }
    /// <summary>The width/stretch percentage.</summary>
    public FontStretch Stretch { get; }

    /// <summary>Initializes a concrete face descriptor.</summary>
    public FontFaceDescriptor(string family, FontWeight weight, FontStyle style, FontStretch stretch)
    {
        Family = family;
        Weight = weight;
        Style = style;
        Stretch = stretch;
    }

    /// <summary>Returns whether this exact face satisfies <paramref name="font"/>.</summary>
    public bool Matches(in Font font) =>
        string.Equals(Family, font.FontFamily, StringComparison.OrdinalIgnoreCase)
        && Weight == font.FontWeight
        && Style.IsItalic == font.FontStyle.IsItalic
        && Style.IsOblique == font.FontStyle.IsOblique
        && Style.ObliqueAngle == font.FontStyle.ObliqueAngle
        && Stretch == font.FontStretch;
}

/// <summary>Identifies one font resource without loading its data.</summary>
public readonly struct FontResource
{
    /// <summary>The concrete face represented by this resource.</summary>
    public FontFaceDescriptor Face { get; }
    /// <summary>The source location understood by the supplying source.</summary>
    public Uri Uri { get; }

    /// <summary>Initializes a font resource.</summary>
    public FontResource(FontFaceDescriptor face, Uri uri)
    {
        Face = face;
        Uri = uri;
    }
}

/// <summary>
/// Supplies declared application fonts and loads their bytes on demand.
/// A platform must fall back to its installed font catalogue when no resource exactly matches a request.
/// </summary>
public interface IFontSource
{
    /// <summary>Gets all declared resources without loading their data.</summary>
    IEnumerable<FontResource> Resources { get; }

    /// <summary>Finds the exact application-provided face for a request.</summary>
    bool TryGet(in Font font, out FontResource resource);

    /// <summary>Loads a resource's bytes when a consumer needs them.</summary>
    ReadOnlyMemory<byte> Load(in FontResource resource);
}

/// <summary>URI-backed implementation of <see cref="IFontSource"/>.</summary>
public sealed class FontSource : IFontSource
{
    private readonly FontResource[] resources;
    private readonly Func<Uri, ReadOnlyMemory<byte>> loader;

    /// <summary>Initializes a URI-backed source with its resource declarations and loader.</summary>
    public FontSource(IEnumerable<FontResource> resources, Func<Uri, ReadOnlyMemory<byte>> loader)
    {
        this.resources = resources.ToArray();
        this.loader = loader;
    }

    /// <inheritdoc/>
    public IEnumerable<FontResource> Resources => resources;

    /// <inheritdoc/>
    public bool TryGet(in Font font, out FontResource resource)
    {
        foreach (var candidate in resources)
        {
            if (candidate.Face.Matches(in font))
            {
                resource = candidate;
                return true;
            }
        }

        resource = default;
        return false;
    }

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Load(in FontResource resource) => loader(resource.Uri);

    /// <summary>Loads a resource addressed as <c>embedded://Assembly/Path/File.ttf</c>.</summary>
    public static ReadOnlyMemory<byte> LoadEmbedded(Uri uri)
    {
        if (!string.Equals(uri.Scheme, "embedded", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Unsupported embedded font URI scheme: {uri.Scheme}");

        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            string.Equals(a.GetName().Name, uri.Authority, StringComparison.OrdinalIgnoreCase))
            ?? Assembly.Load(new AssemblyName(uri.Authority));
        var resourceName = $"{assembly.GetName().Name}.{uri.AbsolutePath.TrimStart('/').Replace('/', '.')}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded font resource '{resourceName}' was not found.");
        using var data = new MemoryStream();
        stream.CopyTo(data);
        return data.ToArray();
    }
}
