namespace Xui.Core.Fonts;

public static class Inter
{
    private const string Assembly = "Xui.Core.Fonts";
    private const string Folder = "Inter";

    /// <summary>Declared Inter faces, without loading their font data.</summary>
    public static IEnumerable<Xui.Core.Canvas.FontResource> Resources
    {
        get
        {
            yield return Resource("Inter-Black.ttf", Xui.Core.Canvas.FontWeight.Black);
            yield return Resource("Inter-BlackItalic.ttf", Xui.Core.Canvas.FontWeight.Black, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Bold.ttf", Xui.Core.Canvas.FontWeight.Bold);
            yield return Resource("Inter-BoldItalic.ttf", Xui.Core.Canvas.FontWeight.Bold, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-ExtraBold.ttf", Xui.Core.Canvas.FontWeight.ExtraBold);
            yield return Resource("Inter-ExtraBoldItalic.ttf", Xui.Core.Canvas.FontWeight.ExtraBold, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-ExtraLight.ttf", Xui.Core.Canvas.FontWeight.ExtraLight);
            yield return Resource("Inter-ExtraLightItalic.ttf", Xui.Core.Canvas.FontWeight.ExtraLight, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Italic.ttf", Xui.Core.Canvas.FontWeight.Normal, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Light.ttf", Xui.Core.Canvas.FontWeight.Light);
            yield return Resource("Inter-LightItalic.ttf", Xui.Core.Canvas.FontWeight.Light, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Medium.ttf", Xui.Core.Canvas.FontWeight.Medium);
            yield return Resource("Inter-MediumItalic.ttf", Xui.Core.Canvas.FontWeight.Medium, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Regular.ttf", Xui.Core.Canvas.FontWeight.Normal);
            yield return Resource("Inter-SemiBold.ttf", Xui.Core.Canvas.FontWeight.SemiBold);
            yield return Resource("Inter-SemiBoldItalic.ttf", Xui.Core.Canvas.FontWeight.SemiBold, Xui.Core.Canvas.FontStyle.Italic);
            yield return Resource("Inter-Thin.ttf", Xui.Core.Canvas.FontWeight.Thin);
            yield return Resource("Inter-ThinItalic.ttf", Xui.Core.Canvas.FontWeight.Thin, Xui.Core.Canvas.FontStyle.Italic);
        }
    }

    /// <summary>Loads Inter data lazily from this assembly.</summary>
    public static Xui.Core.Canvas.IFontSource Source { get; } =
        new Xui.Core.Canvas.FontSource(Resources, Xui.Core.Canvas.FontSource.LoadEmbedded);

    /// <summary>
    /// Returns URIs for all embedded Inter font variants.
    /// </summary>
    public static IEnumerable<Uri> URIs
    {
        get
        {
            foreach (var resource in Resources)
                yield return resource.Uri;
        }
    }

    private static Xui.Core.Canvas.FontResource Resource(
        string fileName,
        Xui.Core.Canvas.FontWeight weight,
        Xui.Core.Canvas.FontStyle? style = null) =>
        new(
            new Xui.Core.Canvas.FontFaceDescriptor(
                "Inter", weight, style ?? Xui.Core.Canvas.FontStyle.Normal, Xui.Core.Canvas.FontStretch.Normal),
            new Uri($"embedded://{Assembly}/{Folder}/{fileName}"));
}
