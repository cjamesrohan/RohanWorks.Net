using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RohanWorks.Net.Options.Validation;
using RohanWorks.Net.Options.Validation.FluentValidation;

namespace RohanWorks.Net.Options.Validation.Tests;

public class ConfigOptionsHealthCheckTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static HealthCheckContext FakeContext() =>
        new() { Registration = new HealthCheckRegistration("test", _ => null!, HealthStatus.Unhealthy, []) };

    [Fact]
    public async Task CheckHealth_ValidConfig_ReturnsHealthy()
    {
        var config = BuildConfig(new()
        {
            ["WeatherOptions:Url"] = "https://example.com"
        });

        var check = new ConfigOptionsHealthCheck<WeatherOptions>(config.GetSection("WeatherOptions"));
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealth_MissingSection_ReturnsUnhealthy()
    {
        var config = BuildConfig(new());

        var check = new ConfigOptionsHealthCheck<WeatherOptions>(config.GetSection("WeatherOptions"));
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("missing");
    }

    [Fact]
    public async Task CheckHealth_InvalidDataAnnotations_ReturnsUnhealthy()
    {
        var config = BuildConfig(new()
        {
            ["WeatherOptions:Url"] = "not-a-url"
        });

        var check = new ConfigOptionsHealthCheck<WeatherOptions>(config.GetSection("WeatherOptions"));
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealth_NestedObjectInvalid_ReturnsUnhealthy()
    {
        var config = BuildConfig(new()
        {
            ["ParentOptions:Url"] = "https://example.com",
            ["ParentOptions:Child:Name"] = null
        });

        var check = new ConfigOptionsHealthCheck<ParentOptions>(config.GetSection("ParentOptions"));
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("Child");
    }

    [Fact]
    public async Task CheckHealth_WithFluentValidator_ValidConfig_ReturnsHealthy()
    {
        var config = BuildConfig(new()
        {
            ["WeatherOptions:Url"] = "https://example.com"
        });

        var check = new ConfigOptionsHealthCheck<WeatherOptions>(config.GetSection("WeatherOptions"), new WeatherOptionsValidator());
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealth_WithFluentValidator_InvalidConfig_ReturnsUnhealthy()
    {
        var config = BuildConfig(new()
        {
            ["WeatherOptions:Url"] = "not-a-url"
        });

        var check = new ConfigOptionsHealthCheck<WeatherOptions>(config.GetSection("WeatherOptions"), new WeatherOptionsValidator());
        var result = await check.CheckHealthAsync(FakeContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    private class WeatherOptions
    {
        [Required]
        [HttpUrl]
        public string? Url { get; set; }
    }

    private class ParentOptions
    {
        [Required]
        [HttpUrl]
        public string? Url { get; set; }

        public ChildOptions? Child { get; set; }
    }

    private class ChildOptions
    {
        [Required] public string? Name { get; set; }
    }

    private class WeatherOptionsValidator : AbstractValidator<WeatherOptions>
    {
        public WeatherOptionsValidator()
        {
            RuleFor(x => x.Url).NotEmpty().HttpUrl();
        }
    }
}
