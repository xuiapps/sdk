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
                    Xui.Runtime.MacOS.Actual.MacOSPlatform.Instance);
#elif WINDOWS && EMULATOR
                new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                    Xui.Runtime.Windows.Actual.Win32Platform.Instance);
#elif BROWSER && EMULATOR
                new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                    Xui.Runtime.Browser.Actual.BrowserPlatform.Instance);
#elif IOS
                Xui.Runtime.IOS.Actual.IOSPlatform.Instance;
#elif ANDROID
                Xui.Runtime.Android.Actual.AndroidPlatform.Instance;
#elif MACOS
                Xui.Runtime.MacOS.Actual.MacOSPlatform.Instance;
#elif WINDOWS
                Xui.Runtime.Windows.Actual.Win32Platform.Instance;
#elif BROWSER
                Xui.Runtime.Browser.Actual.BrowserPlatform.Instance;
#else
                throw new PlatformNotSupportedException();
#endif
#if DEVTOOLS
            runtime = new Xui.Middleware.DevTools.DevToolsPlatform(runtime);
#endif
            config.AddSingleton<IRuntime>(runtime);
        });
}
