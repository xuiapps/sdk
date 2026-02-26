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
                Xui.Runtime.MacOS.Actual.MacOSPlatform.Instance));
#elif WINDOWS && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                Xui.Runtime.Windows.Actual.Win32Platform.Instance));
#elif BROWSER && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.Emulator.Actual.EmulatorPlatform(
                Xui.Runtime.Browser.Actual.BrowserPlatform.Instance));
#elif IOS
            config.AddSingleton<IRuntime>(Xui.Runtime.IOS.Actual.IOSPlatform.Instance);
#elif ANDROID
            config.AddSingleton<IRuntime>(Xui.Runtime.Android.Actual.AndroidPlatform.Instance);
#elif MACOS
            config.AddSingleton<IRuntime>(Xui.Runtime.MacOS.Actual.MacOSPlatform.Instance);
#elif WINDOWS
            config.AddSingleton<IRuntime>(Xui.Runtime.Windows.Actual.Win32Platform.Instance);
#elif BROWSER
            config.AddSingleton<IRuntime>(Xui.Runtime.Browser.Actual.BrowserPlatform.Instance);
#endif
        });
}
