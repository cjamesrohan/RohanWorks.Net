# RohanWorks.Net.Options.Validation

Configuration binding helpers and health check validation for ASP.NET Core. Bind options at startup, validate them at runtime via health checks — not by throwing on boot.

## Installation

```
dotnet add package RohanWorks.Net.Options.Validation
```

Requires `FluentValidation` (included as a dependency — no separate install needed).

---

## Quick Start

```csharp
// Program.cs
var weatherOptions = services.ConfigureAndGet<WeatherOptions>(builder.Configuration);

services.AddHealthChecks()
    .AddCheck("weather-config", new ConfigOptionsHealthCheck<WeatherOptions>(
        builder.Configuration.GetSection("WeatherOptions")));
```

---

## Binding Options — `ConfigureAndGet<T>`

Binds a configuration section, registers `IOptions<T>` in the DI container, and returns the bound instance for use at registration time.

```csharp
// Binds config section named after the type ("WeatherOptions")
var opts = services.ConfigureAndGet<WeatherOptions>(config);

// Or with an explicit section
var opts = services.ConfigureAndGet<WeatherOptions>(config.GetSection("Weather"));
```

The returned `T` is useful when you need the options value during service registration — for example, passing a connection string to `AddDbContext`:

```csharp
var dbOptions = services.ConfigureAndGet<DatabaseOptions>(config);
services.AddDbContext<AppDbContext>(o => o.UseSqlServer(dbOptions.ConnectionString));
```

`IOptions<T>` is still registered in DI, so constructor injection works as normal everywhere else.

---

## Validating Config — `ConfigOptionsHealthCheck<T>`

Instead of throwing on startup, validate config at runtime through ASP.NET Core's health check system. Unhealthy = misconfigured = your monitoring/alerting catches it.

```csharp
services.AddHealthChecks()
    .AddCheck("weather-config", new ConfigOptionsHealthCheck<WeatherOptions>(
        configuration.GetSection("WeatherOptions")));
```

### DataAnnotations (default)

Decorate your options class and `ConfigOptionsHealthCheck` validates automatically, including nested objects:

```csharp
public class WeatherOptions
{
    [Required]
    [HttpUrl]
    public string? ApiUrl { get; set; }

    public RetryOptions? Retry { get; set; }
}

public class RetryOptions
{
    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 3;
}
```

Nested object failures appear in the health check description (`The WeatherOptions.RetryOptions object is invalid: ...`).

### FluentValidation

Pass a validator to use FluentValidation instead of DataAnnotations:

```csharp
services.AddHealthChecks()
    .AddCheck("weather-config", new ConfigOptionsHealthCheck<WeatherOptions>(
        configuration.GetSection("WeatherOptions"), new WeatherOptionsValidator()));
```

```csharp
public class WeatherOptionsValidator : AbstractValidator<WeatherOptions>
{
    public WeatherOptionsValidator()
    {
        RuleFor(x => x.ApiUrl).NotEmpty().HttpUrl();
    }
}
```

---

## Validation Attributes

### `[HttpUrl]`

Validates that a string property is a well-formed absolute HTTP or HTTPS URL.

```csharp
[Required]
[HttpUrl]
public string? ApiUrl { get; set; }
```

Accepts `null` (use `[Required]` to reject null). Rejects relative URLs, non-HTTP schemes, and malformed strings.

### `HttpUrl()` — FluentValidation

```csharp
RuleFor(x => x.ApiUrl).NotEmpty().HttpUrl();
```

Same validation logic as the attribute, available as a FluentValidation rule.

---

## API Reference

| Member | Description |
|---|---|
| `services.ConfigureAndGet<T>(config)` | Binds section named after `T`, registers `IOptions<T>`, returns bound `T` |
| `services.ConfigureAndGet<T>(section)` | Same with an explicit `IConfigurationSection` |
| `new ConfigOptionsHealthCheck<T>(section, validator?)` | Health check that validates config at runtime |
| `[HttpUrl]` | DataAnnotations attribute — validates http/https URL |
| `.HttpUrl()` | FluentValidation rule — same validation as `[HttpUrl]` |
