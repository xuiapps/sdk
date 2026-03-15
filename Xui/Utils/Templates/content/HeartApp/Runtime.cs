using Xui.Core.Actual;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NewBlankApp;

public static class Runtime
{
    public static IHostBuilder UseRuntime(this IHostBuilder @this) =>
        @this.ConfigureServices(config =>
        {
#if MACOS && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                new Xui.Runtime.MacOS.Actual.MacOSPlatform()));
#elif WINDOWS && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                new Xui.Runtime.Windows.Actual.Win32Platform()));
#elif BROWSER && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                new Xui.Runtime.Browser.Actual.BrowserPlatform()));
#elif IOS
            config.AddSingleton<IRuntime>(new Xui.Runtime.IOS.Actual.IOSPlatform());
#elif ANDROID
            config.AddSingleton<IRuntime>(new Xui.Runtime.Android.Actual.AndroidPlatform());
#elif MACOS
            config.AddSingleton<IRuntime>(new Xui.Runtime.MacOS.Actual.MacOSPlatform());
#elif WINDOWS
            config.AddSingleton<IRuntime>(new Xui.Runtime.Windows.Actual.Win32Platform());
#elif BROWSER
            config.AddSingleton<IRuntime>(new Xui.Runtime.Browser.Actual.BrowserPlatform());
#endif
        });
}
