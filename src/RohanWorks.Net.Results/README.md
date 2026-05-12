# RohanWorks.Net.Results

`Result<T>` pattern for ASP.NET Core APIs. Return exceptions instead of throwing them — with a fluent builder that maps results directly to HTTP responses.

## Installation

```
dotnet add package RohanWorks.Net.Results
```

## Quick Start

**Step 1 — Change your service return type from `T` to `Result<T>`:**

```csharp
// Before
public async Task<WeatherForecast> GetForecastAsync(CancellationToken ct)
{
    if (notFound) throw new NotFoundException("No forecast available.");
    return forecast;
}

// After — change throw to return, change return type
public async Task<Result<WeatherForecast>> GetForecastAsync(CancellationToken ct)
{
    if (notFound) return new NotFoundException("No forecast available.");
    return forecast;
}
```

**Step 2 — Replace try/catch in your controller with the builder chain:**

```csharp
// Before
public async Task<IActionResult> GetForecast(CancellationToken ct)
{
    try
    {
        var forecast = await _domain.GetForecastAsync(ct);
        return Ok(forecast);
    }
    catch (NotFoundException ex)
    {
        return NotFound(ex.Message);
    }
}

// After
public Task<IActionResult> GetForecast(CancellationToken ct)
    => _domain.GetForecastAsync(ct)
        .OnSuccess(Ok)
        .HandleException<NotFoundException>(ex => NotFound(ex.Message))
        .ReturnAsync();
```

That's the migration. No new concepts — just `return` instead of `throw`, and a builder chain instead of try/catch.

---

## Controller Pattern (IActionResult)

```csharp
[HttpGet("{id}")]
public Task<IActionResult> Get(int id, CancellationToken ct)
    => _service.GetAsync(id, ct)
        .OnSuccess(Ok)
        .HandleException<NotFoundException>(ex => NotFound(ex.Message))
        .HandleException<ValidationException>(ex => BadRequest(ex.Message))
        .HandleException<Exception>(ex => StatusCode(500, ex.Message))
        .ReturnAsync();
```

Handlers are evaluated in order. The first matching type wins.

## Minimal API Pattern (IResult)

```csharp
app.MapGet("/forecast", async (IWeatherDomain domain, CancellationToken ct) =>
    await domain.GetForecastAsync(ct)
        .OnSuccess(Results.Ok)
        .HandleException<NotFoundException>(ex => Results.NotFound(ex.Message))
        .ReturnAsync());
```

## Synchronous Services

If your service method returns `Result<T>` (not `Task<Result<T>>`), use the sync overloads:

```csharp
// OnSuccess on Result<T> (not Task<Result<T>>) gives you both .Return() and .ReturnAsync()
public IActionResult Get(int id)
    => _service.Get(id)
        .OnSuccess(Ok)
        .HandleException<NotFoundException>(ex => NotFound(ex.Message))
        .Return();
```

---

## Handling Both Returned and Thrown Exceptions

The builder captures both:

```csharp
// returned — no exception overhead, handled by HandleException
return new NotFoundException("not found");

// thrown — still captured and routed through HandleException
throw new NotFoundException("not found");
```

Unhandled exceptions (no matching `HandleException` registration) propagate normally.

---

## 424 Failed Dependency

Helpers for signaling that a downstream dependency caused the failure:

```csharp
using RohanWorks.Net.Results.Http;

.HandleException<HttpRequestException>(ex => new FailedDependencyObjectResult(ex.Message))
// or, no body:
.HandleException<HttpRequestException>(_ => new FailedDependencyResult())
```

---

## Result\<T\> Reference

```csharp
public readonly struct Result<T>
{
    public readonly bool IsSuccess;
    public readonly T? Value;
    public readonly Exception? Exception;

    public static implicit operator Result<T>(T value);       // success
    public static implicit operator Result<T>(Exception ex);  // failure
}
```

A `Result<T>` is always in one of two states:

| State | `IsSuccess` | `Value` | `Exception` |
|---|---|---|---|
| Success | `true` | set | `null` |
| Failure | `false` | `null` | set |

Nullable value types are supported — `Result<decimal?>` succeeding with `null` is valid.

---

## Builder Reference

| Method | Description |
|---|---|
| `.OnSuccess(Func<T, IActionResult>)` | Maps the success value to an HTTP response |
| `.OnSuccess(Func<T, IResult>)` | Minimal API variant |
| `.HandleException<TEx>(Func<TEx, ...>)` | Maps a specific exception type to a response. First match wins. |
| `.Return()` | Resolves synchronously. Use with already-resolved `Result<T>`. |
| `.ReturnAsync()` | Resolves asynchronously. Use with `Task<Result<T>>`. |
