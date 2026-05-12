using Microsoft.Extensions.Configuration;

namespace RohanWorks.Net.Options.Validation;

public sealed class ConfigResult<T> where T : class, new()
{
    public readonly IConfigurationSection Config;

    // Lazily bound on each access — always reflects the current configuration state.
    public T Options => Config.Get<T>() ?? new T();

    [Obsolete("Default constructor disabled.", true)]
    public ConfigResult() => throw new InvalidOperationException("Default constructor disabled.");

    public ConfigResult(IConfigurationSection configSection)
    {
        Config = configSection;
    }
}
