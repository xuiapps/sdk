namespace Xui.Core.DI;

/// <summary>
/// Generic convenience wrappers over <see cref="IServiceProvider"/>,
/// mirroring the shape of Microsoft.Extensions.DependencyInjection without
/// pulling in that package.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Returns the service of type <typeparamref name="T"/>, or <c>null</c>
    /// if no such service is registered.
    /// </summary>
    public static T? GetService<T>(this IServiceProvider provider)
        => (T?)provider.GetService(typeof(T));

    /// <summary>
    /// Returns the service of type <typeparamref name="T"/>.
    /// Throws <see cref="InvalidOperationException"/> if the service is not found.
    /// </summary>
    public static T GetRequiredService<T>(this IServiceProvider provider)
        => provider.GetService<T>()
           ?? throw new InvalidOperationException(
               $"No service of type '{typeof(T).FullName}' is registered.");
}
