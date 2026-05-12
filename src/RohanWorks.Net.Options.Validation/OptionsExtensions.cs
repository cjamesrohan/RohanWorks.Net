using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RohanWorks.Net.Options.Validation;

public static class OptionsExtensions
{
    /// <summary>
    /// Binds configuration section named after <typeparamref name="T"/> and registers it with the DI container.
    /// Chain <see cref="ConfigResultExtensions.Validate{T}"/> to enforce startup validation.
    /// </summary>
    public static ConfigResult<T> ConfigureAndGet<T>(
        this IServiceCollection services,
        IConfiguration config) where T : class, new()
    {
        var configSection = config.GetSection(typeof(T).Name);
        return services.ConfigureAndGet<T>(configSection);
    }

    /// <summary>
    /// Binds an explicit configuration section and registers it with the DI container.
    /// Chain <see cref="ConfigResultExtensions.Validate{T}"/> to enforce startup validation.
    /// </summary>
    public static ConfigResult<T> ConfigureAndGet<T>(
        this IServiceCollection services,
        IConfigurationSection configSection) where T : class, new()
    {
        services.Configure<T>(configSection);
        return new ConfigResult<T>(configSection);
    }
}
