using RohanWorks.Net.Results;

namespace RohanWorks.Net.Sample.Api.Abstractions;

public interface IWeatherDomain
{
    Task<Result<IEnumerable<WeatherForecast>>> GetForecastAsync(CancellationToken ct = default);
    Task<Result<WeatherForecast>> CreateAsync(CancellationToken ct = default);
    Task<Result<WeatherForecast>> TriggerErrorAsync(int statusCode, CancellationToken ct = default);
}
