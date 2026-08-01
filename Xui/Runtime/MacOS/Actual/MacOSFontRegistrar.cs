using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xui.Core.Canvas;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.CoreText;

namespace Xui.Runtime.MacOS.Actual;

/// <summary>Registers an application font only when one of its declared faces is requested.</summary>
internal sealed class MacOSFontRegistrar
{
    private const uint ProcessScope = 1;
    private static readonly object Sync = new();
    private static readonly HashSet<Uri> Registered = new();
    private readonly IFontSource? source;

    public MacOSFontRegistrar(IFontSource? source) => this.source = source;

    public void EnsureRegistered(in Font font)
    {
        if (source is null || !source.TryGet(in font, out var resource))
            return;

        lock (Sync)
        {
            if (!Registered.Add(resource.Uri))
                return;

            try
            {
                var path = Materialize(resource);
                using var pathRef = new CFStringRef(path);
                using var url = CFURLRef.CreateWithFileSystemPath(pathRef);
                if (!CTFontManagerRegisterFontsForURL(url, ProcessScope, out var error))
                {
                    if (error != 0)
                        CFRelease(error);
                    Debug.WriteLine($"CoreText could not register application font '{resource.Uri}'. Falling back to the installed font catalogue.");
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }

    private string Materialize(in FontResource resource)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(resource.Uri.AbsoluteUri)));
        var extension = Path.GetExtension(resource.Uri.AbsolutePath);
        var directory = Path.Combine(Path.GetTempPath(), "xui-fonts");
        var path = Path.Combine(directory, hash + (string.IsNullOrEmpty(extension) ? ".ttf" : extension));
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, source!.Load(in resource).ToArray());
        }
        return path;
    }
}
