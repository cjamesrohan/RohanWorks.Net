using System.ComponentModel.DataAnnotations;
using RohanWorks.Net.Options.Validation;
using RohanWorks.Net.Results;
using RohanWorks.Net.Sample.Api.Abstractions;
using RohanWorks.Net.Sample.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IWeatherDomain, WeatherDomain>();

builder.Services.ConfigureAndGet<WeatherOptions>(builder.Configuration);

// Health check — validates WeatherOptions via DataAnnotations on every /health call.
builder.Services.AddHealthChecks()
    .AddCheck("weather-options", new ConfigOptionsHealthCheck<WeatherOptions>(builder.Configuration.GetSection("WeatherOptions")));

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Minimal API endpoints (IResult pattern) ───────────────────────────────────

var weather = app.MapGroup("/weather");

weather.MapGet("/forecast", async (IWeatherDomain domain, CancellationToken ct) =>
    await domain.GetForecastAsync(ct)
        .OnSuccess(Results.Ok)
        .ReturnAsync());

weather.MapPost("/forecast", async (IWeatherDomain domain, CancellationToken ct) =>
    await domain.CreateAsync(ct)
        .OnSuccess(f => Results.Created("", f))
        .ReturnAsync());

app.Run();

// ── Local types ───────────────────────────────────────────────────────────────

public class WeatherOptions
{
    [Required]
    [HttpUrl]
    public string? Url { get; set; }
}
