using System;

namespace Xui.Core.Canvas;

/// <summary>
/// Parses the portable URI form used for application-packaged image assets.
/// <c>asset://icons/logo.png</c> identifies the logical asset
/// <c>icons/logo.png</c>; each runtime resolves it from its application package.
/// </summary>
public static class AssetUri
{
    private const string Scheme = "asset://";

    /// <summary>
    /// Gets an asset's logical relative path. Returns <see langword="false"/> when
    /// <paramref name="uri"/> is not a valid <c>asset://</c> URI.
    /// </summary>
    public static bool TryGetRelativePath(string uri, out string relativePath)
    {
        relativePath = string.Empty;
        if (!uri.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
            return false;

        var path = uri[Scheme.Length..].Replace('\\', '/').TrimStart('/');
        if (path.Length == 0)
            return false;

        foreach (var segment in path.Split('/'))
        {
            if (segment.Length == 0 || segment is "." or "..")
                return false;
        }

        relativePath = path;
        return true;
    }
}
