using Xui.Apps.TestApp.Pages.Floorplan;

if (!OperatingSystem.IsMacOS()) throw new PlatformNotSupportedException("Atlas generation uses CoreGraphics on macOS; generated assets run on all platforms.");
FloorplanDrawing.CompileGpuAssets(args.Length > 0 ? args[0] : "Xui/Apps/TestApp/Assets/Floorplan/Office.svg",
    args.Length > 1 ? args[1] : "Xui/Apps/TestApp/Assets/FloorplanGpu");
