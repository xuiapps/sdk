using Xui.Core.Actual;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xui.Apps.XuiSDK;

public static class Platform
{
    public static IHostBuilder UseRuntime(this IHostBuilder @this) =>
        @this.ConfigureServices(config =>
        {
            config.AddSingleton<Xui.Core.Canvas.IFontSource>(Xui.Core.Fonts.Inter.Source);
#if MACOS && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
                new Xui.Runtime.MacOS.Actual.MacOSPlatform()));
#elif WINDOWS && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
                new Xui.Runtime.Windows.Actual.Win32Platform()));
#elif BROWSER && EMULATOR
            config.AddSingleton<IRuntime>(new Xui.Middleware.EmulatorLegacy.Actual.EmulatorPlatform(
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
