using Xui.Core.Actual;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xui.Apps.BlankApp;

public static class Platform
{
    public static IHostBuilder UseRuntime(this IHostBuilder @this) =>
        @this.ConfigureServices(config =>
        {
            IRuntime runtime =
#if MACOS && EMULATOR
                new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                    new Xui.Runtime.MacOS.Actual.MacOSPlatform());
#elif WINDOWS && EMULATOR
                new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                    new Xui.Runtime.Windows.Actual.Win32Platform());
#elif BROWSER && EMULATOR
                new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                    new Xui.Runtime.Browser.Actual.BrowserPlatform());
#elif IOS
                new Xui.Runtime.IOS.Actual.IOSPlatform();
#elif ANDROID
                new Xui.Runtime.Android.Actual.AndroidPlatform();
#elif MACOS
                new Xui.Runtime.MacOS.Actual.MacOSPlatform();
#elif WINDOWS
                new Xui.Runtime.Windows.Actual.Win32Platform();
#elif BROWSER
                new Xui.Runtime.Browser.Actual.BrowserPlatform();
#else
                throw new PlatformNotSupportedException();
#endif
#if DEVTOOLS
            runtime = new Xui.Middleware.DevTools.DevToolsPlatform(runtime);
#endif
            config.AddSingleton<IRuntime>(runtime);
        });
}
