using RohanWorks.Net.Results;
using RohanWorks.Net.Sample.Api.Abstractions;

namespace RohanWorks.Net.Sample.Api.Domain;

public class WeatherDomain : IWeatherDomain
{
    private static readonly string[] Summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public async Task<Result<IEnumerable<WeatherForecast>>> GetForecastAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        Result<IEnumerable<WeatherForecast>> result = Enumerable.Range(1, 5).Select(i => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
        return result;
    }

    public Task<Result<WeatherForecast>> CreateAsync(CancellationToken ct = default)
    {
        Result<WeatherForecast> result = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = 20,
            Summary = "Mild"
        };
        return Task.FromResult(result);
    }

    public Task<Result<WeatherForecast>> TriggerErrorAsync(int statusCode, CancellationToken ct = default)
    {
        if (statusCode >= 500) throw new ArgumentOutOfRangeException(nameof(statusCode), "Server-side error (thrown)");
        if (statusCode >= 400) return Task.FromResult<Result<WeatherForecast>>(new ArgumentException("Client-side error (returned)"));
        return Task.FromResult<Result<WeatherForecast>>(new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now), TemperatureC = 22 });
    }
}
