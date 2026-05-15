using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RohanWorks.Net.Options.Validation;

public static class OptionsExtensions
{
    /// <summary>
    /// Binds the configuration section named after <typeparamref name="T"/>, registers it with the DI container,
    /// and returns the bound options instance for use at registration time.
    /// </summary>
    public static T ConfigureAndGet<T>(
        this IServiceCollection services,
        IConfiguration config) where T : class, new()
    {
        var configSection = config.GetSection(typeof(T).Name);
        return services.ConfigureAndGet<T>(configSection);
    }

    /// <summary>
    /// Binds an explicit configuration section, registers it with the DI container,
    /// and returns the bound options instance for use at registration time.
    /// </summary>
    public static T ConfigureAndGet<T>(
        this IServiceCollection services,
        IConfigurationSection configSection) where T : class, new()
    {
        services.Configure<T>(configSection);
        return configSection.Get<T>() ?? new T();
    }

}
