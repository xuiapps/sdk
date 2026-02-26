using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

/// <summary>
/// Entry point for the Windows Imaging Component (WIC).
/// Wraps <c>IWICImagingFactory</c> creation via <c>CoCreateInstance</c>.
/// </summary>
public static partial class WIC
{
    private const string WinCodecLib = "WindowsCodecs.dll";

    // CLSID_WICImagingFactory2  {317d06e8-5f24-433d-bdf7-79ce68d8abc2}
    private static readonly Guid CLSID_WICImagingFactory2 =
        new Guid(0x317d06e8, 0x5f24, 0x433d, 0xbd, 0xf7, 0x79, 0xce, 0x68, 0xd8, 0xab, 0xc2);

    [LibraryImport("ole32.dll")]
    private static unsafe partial int CoCreateInstance(
        in Guid rclsid,
        void* pUnkOuter,
        uint dwClsContext,
        in Guid riid,
        out void* ppv);

    /// <summary>
    /// Creates and returns the WIC imaging factory.
    /// Dispose when done (releases the COM reference).
    /// </summary>
    public static unsafe ImagingFactory CreateImagingFactory()
    {
        const uint CLSCTX_INPROC_SERVER = 1u;
        Marshal.ThrowExceptionForHR(
            CoCreateInstance(
                in CLSID_WICImagingFactory2,
                null,
                CLSCTX_INPROC_SERVER,
                in ImagingFactory.IID,
                out void* ppv));
        return new ImagingFactory(ppv);
    }
}
