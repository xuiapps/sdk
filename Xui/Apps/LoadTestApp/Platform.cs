using Xui.Core.Actual;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xui.Apps.LoadTestApp;

public static class Platform
{
    public static IHostBuilder UseRuntime(this IHostBuilder @this) =>
        @this.ConfigureServices(config =>
        {
            config.AddSingleton<Xui.Core.Canvas.IFontSource>(Xui.Core.Fonts.Inter.Source);
            IRuntime runtime;
#if MACOS && EMULATOR
            runtime = new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
                new Xui.Runtime.MacOS.Actual.MacOSPlatform());
#elif WINDOWS && EMULATOR
            runtime = new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
                new Xui.Runtime.Windows.Actual.Win32Platform());
#elif BROWSER && EMULATOR
            runtime = new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
                new Xui.Runtime.Browser.Actual.BrowserPlatform());
#elif IOS
            runtime = new Xui.Runtime.IOS.Actual.IOSPlatform();
#elif ANDROID
            runtime = new Xui.Runtime.Android.Actual.AndroidPlatform();
#elif MACOS
            runtime = new Xui.Runtime.MacOS.Actual.MacOSPlatform();
#elif WINDOWS
            runtime = new Xui.Runtime.Windows.Actual.Win32Platform();
#elif BROWSER
            runtime = new Xui.Runtime.Browser.Actual.BrowserPlatform();
#else
            throw new PlatformNotSupportedException();
#endif
#if DEVTOOLS
            runtime = new Xui.Middleware.DevTools.DevToolsPlatform(runtime);
#endif
            config.AddSingleton<IRuntime>(runtime);
        });
}
