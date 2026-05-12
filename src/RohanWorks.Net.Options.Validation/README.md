# RohanWorks.Net.Options.Validation

Startup configuration validation for ASP.NET Core. Bind a configuration section, validate it with DataAnnotations (including nested objects), and fail fast at startup — or integrate with health checks for runtime monitoring.

## Installation

```
dotnet add package RohanWorks.Net.Options.Validation
```

## Quick Start

**Define your options class with DataAnnotations:**

```csharp
public class PaymentOptions
{
    [Required]
    [Url]
    public string? BaseUrl { get; set; }

    [Required]
    public string? ApiKey { get; set; }

    [Range(1, 30)]
    public int TimeoutSeconds { get; set; } = 10;
}
```

**Bind and validate at startup — fails immediately if config is missing or invalid:**

```csharp
builder.Services
    .ConfigureAndGet<PaymentOptions>(builder.Configuration)
    .Validate();
```

By default, `ConfigureAndGet` looks for a section named after the type (`PaymentOptions`). Pass a section explicitly if yours differs:

```csharp
builder.Services
    .ConfigureAndGet<PaymentOptions>(builder.Configuration.GetSection("Payment"))
    .Validate();
```

---

## Health Check Integration

Validate options on every `/health` call — catches configuration drift at runtime:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("payment-options", new ConfigOptionsHealthCheck<PaymentOptions>(builder.Configuration));

app.MapHealthChecks("/health");
```

The health check reports `Unhealthy` if the section is missing or any DataAnnotation fails.

---

## Manual Validation

Use `TryValidate` when you need the errors without throwing:

```csharp
var result = services.ConfigureAndGet<PaymentOptions>(config);

if (!result.TryValidate(out var errors))
{
    foreach (var error in errors)
        logger.LogWarning("Config issue: {Error}", error);
}
```

---

## Nested Object Validation

Validation recurses into nested class properties automatically:

```csharp
public class ServiceOptions
{
    [Required]
    public string? Name { get; set; }

    public RetryOptions? Retry { get; set; }
}

public class RetryOptions
{
    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 3;

    [Range(100, 5000)]
    public int DelayMs { get; set; } = 500;
}
```

Both `ServiceOptions` and `RetryOptions` are validated in a single `Validate()` call.

---

## API Reference

| Method | Description |
|---|---|
| `services.ConfigureAndGet<T>(config)` | Binds section named after `T`, registers with DI, returns `ConfigResult<T>` |
| `services.ConfigureAndGet<T>(configSection)` | Binds an explicit section |
| `configResult.Validate()` | Throws `ValidationException` if section is missing or invalid. Chainable. |
| `configResult.TryValidate(out errors)` | Returns `false` and populates error messages without throwing |
| `configResult.Options` | Accesses the bound options instance. Re-binds on each access to reflect live config. |
