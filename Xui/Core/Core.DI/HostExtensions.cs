using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Core.Abstract;

namespace Xui.Core.DI;

public static class HostExtensions
{
    public static int Run<TApplication>(this IHost host)
        where TApplication : Application
    {
        host.Start();
        var application = host.Services.GetRequiredService<TApplication>();
        application.DisposeQueue.Add(host);
        return application.Run();
    }
}
