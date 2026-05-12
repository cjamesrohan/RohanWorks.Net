using Microsoft.AspNetCore.Mvc;
using RohanWorks.Net.Results;
using RohanWorks.Net.Results.Http;
using RohanWorks.Net.Sample.Api.Abstractions;

namespace RohanWorks.Net.Sample.Api.Controllers;

[ApiController]
[Route("controller/[controller]")]
[Produces("application/json")]
public class WeatherController(IWeatherDomain domain) : ControllerBase
{
    // IActionResult pattern (controller-based)
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetForecast(CancellationToken ct = default)
        => domain.GetForecastAsync(ct)
            .OnSuccess(Ok)
            .ReturnAsync();

    [HttpPost("forecast")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status201Created)]
    public Task<IActionResult> Create(CancellationToken ct = default)
        => domain.CreateAsync(ct)
            .OnSuccess(f => Created("", f))
            .ReturnAsync();

    [HttpGet("trigger-error/{statusCode:int}")]
    public Task<IActionResult> TriggerError(int statusCode, CancellationToken ct = default)
        => domain.TriggerErrorAsync(statusCode, ct)
            .OnSuccess(Ok)
            .HandleException<ArgumentException>(ex => BadRequest(ex.Message))
            .HandleException<ArgumentOutOfRangeException>(ex => new FailedDependencyObjectResult(ex.Message))
            .ReturnAsync();
}
